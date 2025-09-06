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
using System.Linq;
using System.Diagnostics;
using YamlDotNet.Core.Tokens;

class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Compile);

    [Solution] readonly Solution Solution;

    [Parameter("Build configuration - Debug or Release")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Target environment - dev, staging, production")]
    readonly string Env = "dev";
    [Parameter("Service name for Docker build (e.g., AuthService, UserService)")]
    string Service;
    [Parameter("Azure Container Registry Name")]
    readonly string AcrName;
    [Parameter("Docker Image Tag")]
    readonly string ImageTag = "latest";

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

    //Target DockerBuild => _ => _
    //.Requires(() => Service)
    //.Executes(() =>
    //{
    //    var sourceRoot = RootDirectory / ".environments" / Service;
    //    var targetRoot = RootDirectory / "services" / Service;

    //    var envMap = new Dictionary<string, string>
    //    {
    //        ["dev"] = "Development",
    //        ["stage"] = "Staging",
    //        ["prod"] = "Production"
    //    };

    //    if (!envMap.ContainsKey(Env.ToLower()))
    //        Log.Information($"❌ Invalid env '{Env}'. Allowed: dev, stage, prod");

    //    var suffix = envMap[Env.ToLower()];
    //    var sourceFile = sourceRoot / Env.ToLower() / "appsettings.json";
    //    var targetFile = targetRoot / $"appsettings.json";
    //    if (!File.Exists(sourceFile))
    //        Log.Information($"❌ Missing source: {sourceFile}");

    //    File.Copy(sourceFile, targetFile, true);
    //    Serilog.Log.Information($"✅ Copied: {targetFile}");

    //    var dockerfilePath = SourceDir / Service / "Dockerfile";
    //    if (!File.Exists(dockerfilePath))
    //        throw new Exception($"❌ Dockerfile not found for service: {Service}");

    //    Log.Information($"🐳 Building Docker image for service: {Service}");

    //    DockerTasks.DockerBuild(s => s
    //        .SetFile(dockerfilePath)
    //        .SetPath(".")
    //        .SetTag(Service.ToLower())
    //        .SetNoCache(true));

    //    Log.Information($"✅ Docker image built: {Service.ToLower()}");

    //});

    //Target DockerBuildAllServices => _ => _
    //.Executes(() =>
    //    {
    //        var services = new[] { "AuthService", "UserService" };
    //        foreach (var service in services)
    //        {
    //            var sourceRoot = RootDirectory / ".environments" / service;
    //            var targetRoot = RootDirectory / "services" / service;

    //            var envMap = new Dictionary<string, string>
    //            {
    //                ["dev"] = "Development",
    //                ["stage"] = "Staging",
    //                ["prod"] = "Production"
    //            };

    //            if (!envMap.ContainsKey(Env.ToLower()))
    //                Log.Information($"❌ Invalid env '{Env}'. Allowed: dev, stage, prod");

    //            var suffix = envMap[Env.ToLower()];
    //            var sourceFile = sourceRoot / Env.ToLower() / "appsettings.json";
    //            var targetFile = targetRoot / $"appsettings.json";
    //            if (!File.Exists(sourceFile))
    //                Log.Information($"❌ Missing source: {sourceFile}");

    //            File.Copy(sourceFile, targetFile, true);
    //            Serilog.Log.Information($"✅ Copied: {targetFile}");

    //            var dockerfilePath = SourceDir / service / "Dockerfile";
    //            if (!File.Exists(dockerfilePath))
    //            {
    //                Log.Warning($"⚠️ Skipping {service}, Dockerfile not found.");
    //                continue;
    //            }

    //            Log.Information($"🐳 Building Docker image for: {service}");

    //            DockerTasks.DockerBuild(s => s
    //                .SetFile(dockerfilePath)
    //                .SetPath(".")
    //                .SetTag(service.ToLower())
    //                .SetNoCache(true));
    //        }
    //    });

    //Target DockerPush => _ => _
    // .Requires(() => Service)
    // .Requires(() => Env)
    // .Requires(() => AcrName)    // e.g., myregistry.azurecr.io
    // .Requires(() => ImageTag)
    // .Executes(() =>
    // {
    //     var lowerService = Service.ToLowerInvariant();
    //     var lowerEnv = Env.ToLowerInvariant();

    //     var envMap = new Dictionary<string, string>
    //     {
    //         ["dev"] = "Development",
    //         ["stage"] = "Staging",
    //         ["prod"] = "Production"
    //     };

    //     if (!envMap.ContainsKey(lowerEnv))
    //         throw new Exception($"❌ Invalid env '{Env}'. Allowed: dev, stage, prod");

    //     // Copy environment-specific appsettings.json
    //     var sourceFile = RootDirectory / ".environments" / Service / lowerEnv / "appsettings.json";
    //     var targetFile = RootDirectory / "services" / Service / "appsettings.json";

    //     if (!File.Exists(sourceFile))
    //         throw new Exception($"❌ Missing source config: {sourceFile}");

    //     File.Copy(sourceFile, targetFile, true);
    //     Serilog.Log.Information($"✅ Copied env config to {targetFile}");

    //     // Build full image name for ACR
    //     var imageName = $"{AcrName}/{lowerService}-{Env}:{ImageTag}";
    //     var dockerfilePath = RootDirectory / "services" / Service / "Dockerfile";

    //     if (!File.Exists(dockerfilePath))
    //         throw new Exception($"⚠️ Dockerfile not found for {Service} at {dockerfilePath}");

    //     // Login to Azure ACR
    //     var acrLoginName = AcrName.Split('.')[0]; // e.g. "myregistry" from "myregistry.azurecr.io"
    //     ProcessTasks.StartProcess("az", $"acr login --name {acrLoginName}")
    //         .AssertZeroExitCode();

    //     // Build image directly with full ACR path
    //     DockerTasks.DockerBuild(s => s
    //         .SetPath(".")
    //         .SetFile(dockerfilePath)
    //         .SetTag(imageName)
    //         .SetNoCache(true)
    //     );

    //     // Push image to ACR
    //     DockerTasks.DockerPush(s => s
    //         .SetName(imageName)
    //     );

    //     Serilog.Log.Information($"✅ Successfully pushed {imageName} to ACR [{envMap[lowerEnv]}]");
    // });

    Target DockerBuild => _ => _
        .Requires(() => Service)
        .Executes(() =>
        {
            BuildDockerImage(Service, ImageTag);
        });

    Target DockerPush => _ => _
        .Requires(() => Service)
        .Requires(() => AcrName)
        .Executes(() =>
        {
            BuildDockerImage(Service, ImageTag, pushToAcr: true);
        });

    Target DockerBuildChangedServices => _ => _
        .Executes(() =>
        {
            var changed = DetectChangedServices();
            if (changed.Length == 0)
            {
                Log.Warning("⚠️ No changed services detected");
                return;
            }

            foreach (var svc in changed)
            {
                BuildDockerImage(svc, ImageTag);
            }
        });

    void CopyEnvConfigFile(string serviceName)
    {
        var lowerEnv = Env.ToLowerInvariant();
        var envMap = new Dictionary<string, string>
        {
            ["dev"] = "Development",
            ["stage"] = "Staging",
            ["prod"] = "Production"
        };

        if (!envMap.ContainsKey(lowerEnv))
            throw new Exception($"❌ Invalid env '{Env}'. Allowed: dev, stage, prod");

        var sourceFile = RootDirectory / ".environments" / serviceName / lowerEnv / "appsettings.json";
        var targetFile = SourceDir / serviceName / "appsettings.json";

        if (!File.Exists(sourceFile))
            throw new Exception($"❌ Missing source config: {sourceFile}");

        File.Copy(sourceFile, targetFile, true);
        Serilog.Log.Information($"✅ Copied {sourceFile} → {targetFile}");
    }

    void BuildDockerImage(string serviceName, string imageTag, bool pushToAcr = false)
    {
        CopyEnvConfigFile(serviceName);
        var dockerfilePath = SourceDir / serviceName / "Dockerfile";
        if (!File.Exists(dockerfilePath))
            throw new Exception($"❌ Dockerfile not found for {serviceName}");

        var lowerService = serviceName.ToLowerInvariant();
        var tag = pushToAcr
            ? $"{AcrName}/{lowerService}-{Env}:{imageTag}"
            : $"{lowerService}:{imageTag}";

        Log.Information($"🐳 Building {tag}");
        if (pushToAcr)
        {
            DockerTasks.DockerBuild(s => s
               .SetFile(dockerfilePath)
               .SetPath(".") // service folder as context
               .SetTag(tag)
               .SetNoCache(true));
        }
        else
        {
            DockerTasks.DockerBuild(s => s
              .SetFile(dockerfilePath)
              .SetPath(".") // service folder as context
              .SetTag(lowerService)
              .SetNoCache(true));
        }


        Log.Information($"✅ Docker image built: {serviceName.ToLower()}");
        if (pushToAcr)
        {
            var acrLoginName = AcrName.Split('.')[0];
            ProcessTasks.StartProcess("az", $"acr login --name {acrLoginName}")
                .AssertZeroExitCode();

            DockerTasks.DockerPush(s => s.SetName(tag));
            Log.Information($"✅ Pushed {tag} to {AcrName}");
        }
    }

    string[] DetectChangedServices()
    {
        var process = ProcessTasks.StartProcess("git", "diff --name-only HEAD~1 HEAD");
        process.AssertZeroExitCode();

        var changedFiles = process.Output
            .Select(o => o.Text)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToArray();

        var changedServices = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in changedFiles)
        {
            if (path.StartsWith("services/AuthService/", StringComparison.OrdinalIgnoreCase))
                changedServices.Add("AuthService");
            else if (path.StartsWith("services/UserService/", StringComparison.OrdinalIgnoreCase))
                changedServices.Add("UserService");
        }
        Log.Information($"Changed Services: {string.Join(", ", changedServices)}");

        return changedServices.ToArray();
    }

    Target PrintChangedServices => _ => _
       .Executes(() =>
       {
           var changedServices = DetectChangedServices();

           if (changedServices.Length == 0)
               Log.Information("No services changed.");


       });

    Target Compile => _ => _
        .DependsOn(LogEnvironment, Clean, BuildAuthService, BuildUserService);
}
