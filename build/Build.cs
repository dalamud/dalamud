using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.Common;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Publish);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;

    [PathExecutable] readonly Tool Cargo;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath ArtifactsDirectory => Configuration switch
    {
        _ when Configuration == Configuration.Debug => RootDirectory / "artifacts" / "debug",
        _ when Configuration == Configuration.Release => RootDirectory / "artifacts" / "release",
        _ => throw new NotImplementedException(),
    };
    AbsolutePath LibraryDirectory => RootDirectory / "lib";

    Dictionary<string, string> EnvironmentVariables => new(EnvironmentInfo.Variables);

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target CompileDalamud => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });

    Target CompileDalamudBoot => _ => _
        .Executes(() =>
        {
            var cmd = _ switch {
                _ when Configuration == Configuration.Debug => "build --package dalamud_boot --all-features",
                _ when Configuration == Configuration.Release => "build --package dalamud_boot --all-features --release",
                _ => throw new NotImplementedException(),
            };

            var env = EnvironmentVariables;
            env["CARGO_TERM_COLOR"] = "always";

            Cargo(
                cmd,
                environmentVariables: env,
                customLogger: (type, output) => Logger.Normal(output) // just like many unix tools, it uses stderr to display messages
            );
        });

    Target Compile => _ => _
        .DependsOn(CompileDalamud)
        .DependsOn(CompileDalamudBoot);

    Target Publish => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            EnsureCleanDirectory(ArtifactsDirectory);

            // publish Dalamud
            DotNetPublish(s => s
                .SetProject(SourceDirectory / "Dalamud" / "Dalamud.csproj")
                .SetConfiguration(Configuration)
                .SetOutput(ArtifactsDirectory)
            );

            // Copy dalamud_boot.dll
            var cargoTargetDirectory = RootDirectory / "target" / Configuration.ToString().ToLower();
            CopyFileToDirectory(cargoTargetDirectory / "dalamud_boot.dll", ArtifactsDirectory);

            // Copy nethost.dll
            CopyFileToDirectory(LibraryDirectory / "dotnet" / "nethost.dll", ArtifactsDirectory);
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() => {
            // cargo test
            var cargoTestArg = _ switch
            {
                _ when Configuration == Configuration.Debug => "test --all-features",
                _ when Configuration == Configuration.Release => "test --all-features --release",
                _ => throw new NotImplementedException(),
            };

            var env = EnvironmentVariables;
            env["CARGO_TERM_COLOR"] = "always";
            env["Path"] += $";{LibraryDirectory}/dotnet";

            Cargo(
                cargoTestArg,
                environmentVariables: env,
                customLogger: (type, output) => Logger.Normal(output) // just like many unix tools, it uses stderr to display messages
            );

            // dotnet test
            DotNetTest();
        });
}
