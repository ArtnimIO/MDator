using System.Collections.Immutable;
using System.Reflection;
using MDator.SourceGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MDator.Tests;

public sealed class NoOpShimAnalyzerTests
{
    private static async Task<ImmutableArray<Diagnostic>> Analyze(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        // Reference everything currently loaded into the test process plus
        // the MDator assembly explicitly — that's the simplest way to satisfy
        // the analyzer's GetTypeByMetadataName lookup for MDator.MDatorConfiguration.
        var trustedAssembliesPaths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(System.IO.Path.PathSeparator);
        var references = trustedAssembliesPaths
            .Select(p => MetadataReference.CreateFromFile(p))
            .Cast<MetadataReference>()
            .Append(MetadataReference.CreateFromFile(typeof(MDator.MDatorConfiguration).Assembly.Location))
            .ToArray();

        var compilation = CSharpCompilation.Create(
            assemblyName: "AnalyzerTest",
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Compilation must be diagnostic-free for our analyzer assertions to be meaningful.
        var compileErrors = compilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.Empty(compileErrors);

        var withAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new NoOpShimAnalyzer()));
        return await withAnalyzers.GetAnalyzerDiagnosticsAsync();
    }

    [Fact]
    public async Task RegisterServicesFromAssemblyContaining_call_is_flagged()
    {
        var diagnostics = await Analyze("""
            using MDator;
            public class Marker { }
            public static class Setup
            {
                public static void Configure(MDatorConfiguration cfg)
                {
                    cfg.RegisterServicesFromAssemblyContaining<Marker>();
                }
            }
            """);

        var hit = Assert.Single(diagnostics, d => d.Id == NoOpShimAnalyzer.DiagnosticId);
        Assert.Contains("RegisterServicesFromAssemblyContaining", hit.GetMessage());
        Assert.Equal(DiagnosticSeverity.Info, hit.Severity);
    }

    [Fact]
    public async Task RegisterServicesFromAssembly_call_is_flagged()
    {
        var diagnostics = await Analyze("""
            using System.Reflection;
            using MDator;
            public static class Setup
            {
                public static void Configure(MDatorConfiguration cfg)
                {
                    cfg.RegisterServicesFromAssembly(typeof(Setup).Assembly);
                }
            }
            """);

        var hit = Assert.Single(diagnostics, d => d.Id == NoOpShimAnalyzer.DiagnosticId);
        Assert.Contains("RegisterServicesFromAssembly", hit.GetMessage());
    }

    [Fact]
    public async Task RegisterServicesFromAssemblies_call_is_flagged()
    {
        var diagnostics = await Analyze("""
            using System.Reflection;
            using MDator;
            public static class Setup
            {
                public static void Configure(MDatorConfiguration cfg)
                {
                    cfg.RegisterServicesFromAssemblies(typeof(Setup).Assembly);
                }
            }
            """);

        Assert.Single(diagnostics, d => d.Id == NoOpShimAnalyzer.DiagnosticId);
    }

    [Fact]
    public async Task Unrelated_calls_on_MDatorConfiguration_are_not_flagged()
    {
        var diagnostics = await Analyze("""
            using MDator;
            using Microsoft.Extensions.DependencyInjection;
            public sealed class MyBehavior
            {
            }
            public static class Setup
            {
                public static void Configure(MDatorConfiguration cfg)
                {
                    cfg.AddBehavior<MyBehavior>(ServiceLifetime.Singleton);
                    cfg.FuseOnly = true;
                }
            }
            """);

        Assert.DoesNotContain(diagnostics, d => d.Id == NoOpShimAnalyzer.DiagnosticId);
    }
}
