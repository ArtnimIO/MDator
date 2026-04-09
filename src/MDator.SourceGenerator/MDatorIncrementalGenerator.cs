using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MDator.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public sealed class MDatorIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Handlers and closed behaviors: pattern-match every class declaration with
        // a base list and classify its interfaces.
        var handlers = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax { BaseList: { } bl } && bl.Types.Count > 0,
                transform: static (ctx, ct) => ExtractHandlerInfos(ctx, ct))
            .SelectMany(static (infos, _) => infos)
            .Collect()
            .Select(static (arr, _) => new EquatableArray<HandlerInfo>(arr.ToArray()));

        var closedBehaviors = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax { BaseList: { } bl } && bl.Types.Count > 0,
                transform: static (ctx, ct) => ExtractBehaviorInfos(ctx, ct))
            .SelectMany(static (infos, _) => infos)
            .Collect()
            .Select(static (arr, _) => new EquatableArray<BehaviorInfo>(arr.ToArray()));

        // Open behaviors declared via [assembly: MDator.OpenBehavior(typeof(...))].
        // We read these from the compilation's assembly attributes. The Select
        // materializes an EquatableArray so downstream stages are cached even when
        // unrelated parts of the compilation change.
        var openBehaviors = context.CompilationProvider
            .Select(static (c, ct) => ExtractOpenBehaviors(c, ct));

        // Assembly name — we use it as the namespace for the generated mediator so
        // it doesn't collide across projects.
        var assemblyName = context.CompilationProvider
            .Select(static (c, _) => c.AssemblyName ?? "GeneratedMDator");

        var combined = handlers
            .Combine(closedBehaviors)
            .Combine(openBehaviors)
            .Combine(assemblyName)
            .Select(static (t, _) =>
            {
                var ((hAndCb, open), asm) = t;
                var (h, cb) = hAndCb;
                var all = cb.Concat(open).ToArray();
                return new PipelineModel(asm, h, new EquatableArray<BehaviorInfo>(all));
            });

        context.RegisterSourceOutput(combined, static (spc, model) =>
        {
            if (model.Handlers.Count == 0) return; // nothing to do — consumer hasn't declared any handlers yet.
            var code = MediatorEmitter.Emit(model);
            spc.AddSource("MDatorGenerated.g.cs", code);
        });
    }

    private static IReadOnlyList<HandlerInfo> ExtractHandlerInfos(GeneratorSyntaxContext ctx, CancellationToken ct)
    {
        var symbol = ctx.SemanticModel.GetDeclaredSymbol((ClassDeclarationSyntax)ctx.Node, ct) as INamedTypeSymbol;
        if (symbol is null) return System.Array.Empty<HandlerInfo>();
        var list = new List<HandlerInfo>();
        foreach (var info in HandlerDiscovery.Classify(symbol)) list.Add(info);
        return list;
    }

    private static IReadOnlyList<BehaviorInfo> ExtractBehaviorInfos(GeneratorSyntaxContext ctx, CancellationToken ct)
    {
        var symbol = ctx.SemanticModel.GetDeclaredSymbol((ClassDeclarationSyntax)ctx.Node, ct) as INamedTypeSymbol;
        if (symbol is null) return System.Array.Empty<BehaviorInfo>();
        var list = new List<BehaviorInfo>();
        foreach (var info in BehaviorDiscovery.ClassifyClosed(symbol)) list.Add(info);
        return list;
    }

    private static EquatableArray<BehaviorInfo> ExtractOpenBehaviors(Compilation compilation, CancellationToken ct)
    {
        var openBehaviorAttr = compilation.GetTypeByMetadataName("MDator.OpenBehaviorAttribute");
        if (openBehaviorAttr is null) return EquatableArray<BehaviorInfo>.Empty;

        var result = new List<BehaviorInfo>();
        foreach (var attr in compilation.Assembly.GetAttributes())
        {
            ct.ThrowIfCancellationRequested();
            if (!SymbolEqualityComparer.Default.Equals(attr.AttributeClass, openBehaviorAttr)) continue;
            if (attr.ConstructorArguments.Length == 0) continue;
            if (attr.ConstructorArguments[0].Value is not INamedTypeSymbol behaviorType) continue;

            var order = 0;
            foreach (var named in attr.NamedArguments)
            {
                if (named.Key == "Order" && named.Value.Value is int io) order = io;
            }

            // Detect whether this is a stream behavior by checking its interfaces.
            var kind = BehaviorKind.Request;
            foreach (var iface in behaviorType.AllInterfaces)
            {
                if (iface.IsMDatorInterface("IStreamPipelineBehavior`2")) { kind = BehaviorKind.Stream; break; }
            }

            result.Add(new BehaviorInfo(
                kind,
                behaviorType.ToTypeRef(),
                ClosedRequestType: null,
                ClosedResponseType: null,
                IsOpenGeneric: true,
                Order: order));
        }

        return new EquatableArray<BehaviorInfo>(result.ToArray());
    }
}
