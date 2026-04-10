using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Serilog;

class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Pack);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("NuGet API key for publishing packages")]
    [Secret]
    readonly string? NuGetApiKey;

    [Solution]
    readonly Solution Solution = null!;

    AbsolutePath OutputDirectory => RootDirectory / "output";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            OutputDirectory.CreateOrCleanDirectory();
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetTasks.DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetTasks.DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetNoRestore(true));
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTasks.DotNetTest(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetNoBuild(true));
        });

    Target Pack => _ => _
        .DependsOn(Test)
        .Executes(() =>
        {
            AbsolutePath[] packableProjects =
            [
                RootDirectory / "src" / "MDator" / "MDator.csproj",
                RootDirectory / "src" / "MDator.Abstractions" / "MDator.Abstractions.csproj",
            ];

            foreach (var project in packableProjects)
            {
                DotNetTasks.DotNetPack(s => s
                    .SetProject(project)
                    .SetConfiguration(Configuration)
                    .SetNoBuild(true)
                    .SetOutputDirectory(OutputDirectory));
            }
        });

    Target Publish => _ => _
        .DependsOn(Pack)
        .Requires(() => NuGetApiKey)
        .Executes(() =>
        {
            var packages = OutputDirectory.GlobFiles("*.nupkg");
            Log.Information("Publishing {Count} package(s)", packages.Count);

            foreach (var package in packages)
            {
                DotNetTasks.DotNetNuGetPush(s => s
                    .SetTargetPath(package)
                    .SetSource("https://api.nuget.org/v3/index.json")
                    .SetApiKey(NuGetApiKey)
                    .SetSkipDuplicate(true));
            }
        });
}
