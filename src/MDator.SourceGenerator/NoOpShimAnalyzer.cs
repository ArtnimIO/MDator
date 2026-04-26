using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace MDator.SourceGenerator;

/// <summary>
/// Reports calls to MediatR-compatibility shim methods on
/// <c>MDator.MDatorConfiguration</c> that have no effect at compile time or
/// runtime. MDator's source generator scans handlers from the consuming
/// compilation directly; the shims exist purely so that MediatR config code
/// continues to compile during migration.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NoOpShimAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "MDATOR0001";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "MDator MediatR-compat shim has no effect",
        messageFormat: "'{0}' is a MediatR source-compatibility shim and has no effect; MDator's source generator discovers handlers automatically",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description:
            "MDatorConfiguration.RegisterServicesFromAssembly, RegisterServicesFromAssemblies, " +
            "and RegisterServicesFromAssemblyContaining<T> exist for MediatR migration ergonomics " +
            "but do not affect handler discovery. The generator scans the consuming compilation " +
            "directly. The call can be removed.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(static startCtx =>
        {
            var configType = startCtx.Compilation.GetTypeByMetadataName("MDator.MDatorConfiguration");
            if (configType is null) return;

            startCtx.RegisterOperationAction(opCtx =>
            {
                var inv = (IInvocationOperation)opCtx.Operation;
                var method = inv.TargetMethod;

                if (!SymbolEqualityComparer.Default.Equals(method.ContainingType, configType)) return;

                var name = method.Name;
                if (name == "RegisterServicesFromAssembly" ||
                    name == "RegisterServicesFromAssemblies" ||
                    name == "RegisterServicesFromAssemblyContaining")
                {
                    opCtx.ReportDiagnostic(Diagnostic.Create(Rule, inv.Syntax.GetLocation(), name));
                }
            }, OperationKind.Invocation);
        });
    }
}
