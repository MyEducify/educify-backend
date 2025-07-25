using System;
using System.IO;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Compile);

    [Solution] readonly Solution Solution;

    [Parameter("Configuration to build - Debug or Release")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    AbsolutePath SourceDir => RootDirectory / "services";
    AbsolutePath SharedDir => RootDirectory / "shared";
    AbsolutePath OutputDir => RootDirectory / "output";

    AbsolutePath AuthServiceProj => SourceDir / "AuthService" / "AuthService.csproj";
    AbsolutePath DatabaseProj => SharedDir / "Database" / "Database.csproj";
    AbsolutePath UtilsProj => SharedDir / "Utils" / "Utils.csproj";
    AbsolutePath ModelsProj => SharedDir / "Models" / "Models.csproj";
    AbsolutePath ServicesProj => SharedDir / "Services" / "Services.csproj";
    AbsolutePath MonitoringProj => SharedDir / "Monitoring" / "Monitoring.csproj";

    Target Clean => _ => _
        .Executes(() =>
        {
            if (Directory.Exists(OutputDir))
                Directory.Delete(OutputDir, recursive: true);

            Directory.CreateDirectory(OutputDir);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s.SetProjectFile(Solution));
        });

    Target BuildShared => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(DatabaseProj)
                .SetConfiguration(Configuration)
                .EnableNoRestore());

            DotNetBuild(s => s
                .SetProjectFile(UtilsProj)
                .SetConfiguration(Configuration)
                .EnableNoRestore());

            DotNetBuild(s => s
                .SetProjectFile(ModelsProj)
                .SetConfiguration(Configuration)
                .EnableNoRestore());

            DotNetBuild(s => s
                .SetProjectFile(ServicesProj)
                .SetConfiguration(Configuration)
                .EnableNoRestore());

            DotNetBuild(s => s
                .SetProjectFile(MonitoringProj)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });

    Target BuildAuthService => _ => _
        .DependsOn(BuildShared)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(AuthServiceProj)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });

    Target Compile => _ => _
        .DependsOn(Clean, BuildAuthService);
}
