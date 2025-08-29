using System;
using System.IO;
using Serilog;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using System.Collections.Generic;
using Nuke.Common.Tools.Docker;

class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Compile);

    [Solution] readonly Solution Solution;

    [Parameter("Build configuration - Debug or Release")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Target environment - dev, staging, production")]
    readonly string Env = "dev";
    [Parameter("Service name for Docker build (e.g., AuthService, UserService)")]
    readonly string Service;

    string DotNetEnvironment => Env.ToLower() switch
    {
        "dev" => "Development",
        "staging" => "Staging",
        "production" => "Production",
        _ => throw new Exception($"Unsupported environment: {Env}")
    };

    AbsolutePath SourceDir => RootDirectory / "services";
    AbsolutePath SharedDir => RootDirectory / "shared";
    AbsolutePath OutputDir => RootDirectory / "output" / Env;

    AbsolutePath AuthServiceProj => SourceDir / "AuthService" / "AuthService.csproj";
    AbsolutePath UserServiceProj => SourceDir / "UserService" / "UserService.csproj";
    AbsolutePath DatabaseProj => SharedDir / "Database" / "Database.csproj";
    AbsolutePath UtilsProj => SharedDir / "Utils" / "Utils.csproj";
    AbsolutePath ModelsProj => SharedDir / "Models" / "Models.csproj";
    AbsolutePath ServicesProj => SharedDir / "Services" / "Services.csproj";
    AbsolutePath MonitoringProj => SharedDir / "Monitoring" / "Monitoring.csproj";
    AbsolutePath CacheProj => SharedDir / "cache" / "cache.csproj";
    AbsolutePath MessageQueueProj => SharedDir / "Message-Queue" / "Message-Queue.csproj";
    AbsolutePath MicroserviceProj => SharedDir / "Microservice" / "Microservice.csproj";
    AbsolutePath Auth0Proj => SharedDir / "Auth0" / "Auth0.csproj";

    Target LogEnvironment => _ => _
        .Executes(() =>
        {

            Log.Information($"🌍 Building for environment: {DotNetEnvironment}");
            Log.Information($"📦 Configuration: {Configuration}");
        });

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

            DotNetBuild(s => s
                .SetProjectFile(CacheProj)
                .SetConfiguration(Configuration)
                .EnableNoRestore());

            DotNetBuild(s => s
                .SetProjectFile(MessageQueueProj)
                .SetConfiguration(Configuration)
                .EnableNoRestore());

            DotNetBuild(s => s
                .SetProjectFile(MicroserviceProj)
                .SetConfiguration(Configuration)
                .EnableNoRestore());

            DotNetBuild(s => s
               .SetProjectFile(Auth0Proj)
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
                .EnableNoRestore()
                .SetOutputDirectory(OutputDir)
                .SetProcessEnvironmentVariable("ASPNETCORE_ENVIRONMENT", DotNetEnvironment));
        });

    Target BuildUserService => _ => _
      .DependsOn(BuildShared)
      .Executes(() =>
      {
          DotNetBuild(s => s
              .SetProjectFile(UserServiceProj)
              .SetConfiguration(Configuration)
              .EnableNoRestore()
              .SetOutputDirectory(OutputDir)
              .SetProcessEnvironmentVariable("ASPNETCORE_ENVIRONMENT", DotNetEnvironment));
      });

    Target GenerateEnv => _ => _
    .Executes(() =>
    {

        var services = new[] { "AuthService", "UserService" };
        foreach (var serviceName in services)
        {
            var sourceRoot = RootDirectory / ".environments" / serviceName;
            var targetRoot = RootDirectory / "services" / serviceName;

            var envMap = new Dictionary<string, string>
            {
                ["dev"] = "Development",
                ["stage"] = "Staging",
                ["prod"] = "Production"
            };


            if (!envMap.ContainsKey(Env.ToLower()))
                Log.Information($"❌ Invalid env '{Env}'. Allowed: dev, stage, prod");

            var suffix = envMap[Env.ToLower()];
            var sourceFile = sourceRoot / Env.ToLower() / "appsettings.json";
            var targetFile = targetRoot / $"appsettings.json";
            if (!File.Exists(sourceFile))
                Log.Information($"❌ Missing source: {sourceFile}");

            File.Copy(sourceFile, targetFile, true);
            Serilog.Log.Information($"✅ Copied: {targetFile}");
        }

    });

    Target DockerBuild => _ => _
    .Requires(() => Service)
    .Executes(() =>
    {
        var dockerfilePath = SourceDir / Service / "Dockerfile";
        if (!File.Exists(dockerfilePath))
            throw new Exception($"❌ Dockerfile not found for service: {Service}");

        Log.Information($"🐳 Building Docker image for service: {Service}");

        DockerTasks.DockerBuild(s => s
            .SetFile(dockerfilePath)
            .SetPath(".")
            .SetTag(Service.ToLower())
            .SetNoCache(true));

        Log.Information($"✅ Docker image built: {Service.ToLower()}");

    });

    Target DockerBuildAllServices => _ => _
    .Executes(() =>
        {
            var services = new[] { "AuthService", "UserService" };
            foreach (var service in services)
            {
                var dockerfilePath = SourceDir / service / "Dockerfile";

                if (!File.Exists(dockerfilePath))
                {
                    Log.Warning($"⚠️ Skipping {service}, Dockerfile not found.");
                    continue;
                }

                Log.Information($"🐳 Building Docker image for: {service}");

                DockerTasks.DockerBuild(s => s
                    .SetFile(dockerfilePath)
                    .SetPath(".")
                    .SetTag(service.ToLower())
                    .SetNoCache(true));
            }
        });

    Target Compile => _ => _
        .DependsOn(LogEnvironment, Clean, BuildAuthService, BuildUserService);
}
