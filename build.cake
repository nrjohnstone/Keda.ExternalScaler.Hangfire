#addin "Cake.Incubator&version=3.1.0"
#addin "Cake.Docker&version=0.11.0"
#addin "Cake.Powershell"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var _target = Argument("target", "Default");
var _configuration = Argument("configuration", "Debug");
var _verbosity = Argument<string>("verbosity", "Minimal");

// MSBuild settings
var _msBuildVerbosity = Verbosity.Minimal;
var _targetPlatform = PlatformTarget.x64;

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var _solutionDir = Directory(".");
var _solutionFile = _solutionDir + File($"Keda.ExternalScaler.Hangfire.sln");
var _mainProjDir = _solutionDir + Directory("src") + Directory ("Keda.ExternalScaler.Hangfire");
var _consumerProjDir = _solutionDir + Directory("src") + Directory ("Hangfire.Consumer");
var _dashboardProjDir = _solutionDir + Directory("src") + Directory ("Hangfire.Dashboard");

var _artifactDir = _solutionDir + Directory("artifacts");
var _scalerArtifactDir = _solutionDir + Directory("artifacts") + Directory("Keda.ExternalScaler.Hangfire");
var _consumerArtifactDir = _solutionDir + Directory("artifacts") + Directory("Hangfire.Consumer");
var _dashboardArtifactDir = _solutionDir + Directory("artifacts") + Directory("Hangfire.Dashboard");
var _version = EnvironmentVariable("build_number");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    var objDirs = GetDirectories($"{_solutionDir}/**/obj/**/{_configuration}");
    var binDirs = GetDirectories($"{_solutionDir}/**/bin/**/{_configuration}");

    var directories = binDirs + objDirs;
	CleanDirectory(_artifactDir);
    CleanDirectories(directories);
});

Task("Restore-NuGet-Packages")
    .Does(() =>
{
    DotNetCoreRestore(_solutionFile);
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    var settings = new DotNetCoreBuildSettings
	{
		Configuration = _configuration,
	};

    DotNetCoreBuild(_solutionFile, settings);
});

Task("Rebuild")
    .IsDependentOn("Clean")
    .IsDependentOn("Build");

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    var testAssemblies = GetFiles($"{_solutionDir}/tests/**/*.Tests.Unit.csproj");
    if(testAssemblies.Count == 0)
    {
        Information("No tests found");
        return;
    }

    var settings = new DotNetCoreTestSettings()
    {
        Configuration = _configuration,
        NoBuild = true,
        Logger = "console;verbosity=normal"
    };

    foreach(var project in testAssemblies)
    {
        DotNetCoreTest(project.FullPath, settings);
    }
});

Task("Publish-ExternalScaler")
    .IsDependentOn("Build")
    .IsDependentOn("Run-Unit-Tests")
    .Does(() => 
{
    EnsureDirectoryExists(_scalerArtifactDir);
    CleanDirectory(_scalerArtifactDir);

    var settings = new DotNetCorePublishSettings
                       {
                           Configuration = _configuration,
                           OutputDirectory = _scalerArtifactDir
                       };

    DotNetCorePublish(_mainProjDir, settings);
});

Task("Publish-Consumer")
    .IsDependentOn("Build")
    .IsDependentOn("Run-Unit-Tests")
    .Does(() => 
{
    EnsureDirectoryExists(_consumerArtifactDir);
    CleanDirectory(_consumerArtifactDir);

    var settings = new DotNetCorePublishSettings
                       {
                           Configuration = _configuration,
                           OutputDirectory = _consumerArtifactDir
                       };

    DotNetCorePublish(_consumerProjDir, settings);
});

Task("Publish-Dashboard")
    .IsDependentOn("Build")
    .IsDependentOn("Run-Unit-Tests")
    .Does(() => 
{
    EnsureDirectoryExists(_dashboardArtifactDir);
    CleanDirectory(_dashboardArtifactDir);

    var settings = new DotNetCorePublishSettings
                       {
                           Configuration = _configuration,
                           OutputDirectory = _dashboardArtifactDir
                       };

    DotNetCorePublish(_dashboardProjDir, settings);
});

Task("Docker-Build-ExternalScaler")
    .IsDependentOn("Publish-ExternalScaler")    
    .Does(() => 
{
    string containerTag = "keda-externalscaler-hangfire:latest";
    
    DockerBuild(new DockerImageBuildSettings(){
        File = _mainProjDir + File("Dockerfile"),
        Tag = new [] { containerTag }
    }, _scalerArtifactDir );
});


Task("Docker-Build-Consumer")
    .IsDependentOn("Publish-Consumer")
    .Does(() => 
{
    string containerTag = "hangfire-consumer:latest";
    
    DockerBuild(new DockerImageBuildSettings(){
        File = _consumerProjDir + File("Dockerfile"),
        Tag = new [] { containerTag }
    }, _consumerArtifactDir );
});

Task("Docker-Build-Dashboard")
    .IsDependentOn("Publish-Dashboard")
    .Does(() => 
{
    string containerTag = "hangfire-dashboard:latest";
    
    DockerBuild(new DockerImageBuildSettings(){
        File = _dashboardProjDir + File("Dockerfile"),
        Tag = new [] { containerTag }
    }, _dashboardArtifactDir );
});

string GetContainerRepository()
{
    var repository = Environment.GetEnvironmentVariable("CONTAINER_REGISTRY");
    if (string.IsNullOrEmpty(repository))
    {
        throw new Exception("Environment variable CONTAINER_REGISTRY must be defined");
    }
    return repository;
}

Task("Test-Powershell")
    .Does(() => 
{
    var repo = GetContainerRepository();
});

Task("Docker-Push-Consumer")
    .IsDependentOn("Docker-Build-Consumer")
    .Does(() => 
{
    var repository = GetContainerRepository();
    string repositoryTag = $"{repository}/hangfire-consumer:latest";
    DockerTag("hangfire-consumer:latest", repositoryTag);
    DockerPush(repositoryTag);
});

Task("Docker-Push-ExternalScaler")
    .IsDependentOn("Docker-Build-ExternalScaler")
    .Does(() => 
{
    var repository = GetContainerRepository();
    string repositoryTag = $"{repository}/keda-externalscaler-hangfire:latest";
    DockerTag("keda-externalscaler-hangfire:latest", repositoryTag);
    DockerPush(repositoryTag);
});

Task("Docker-Push-Dashboard")
    .IsDependentOn("Docker-Build-Dashboard")
    .Does(() => 
{
    var repository = GetContainerRepository();
    string repositoryTag = $"{repository}/hangfire-dashboard:latest";
    DockerTag("hangfire-dashboard:latest", repositoryTag);
    DockerPush(repositoryTag);
});

Task("Docker-Push-All")
    .IsDependentOn("Docker-Push-Dashboard")
    .IsDependentOn("Docker-Push-Consumer")
    .IsDependentOn("Docker-Push-ExternalScaler");

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Run-Unit-Tests");

Task("Build-All")
    .IsDependentOn("Build")
    .IsDependentOn("Run-Unit-Tests")
    .IsDependentOn("Docker-Build-Consumer")
    .IsDependentOn("Docker-Build-Dashboard")
    .IsDependentOn("Docker-Build-ExternalScaler");


//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(_target);