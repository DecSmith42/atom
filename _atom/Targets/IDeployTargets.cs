namespace Atom.Targets;

internal interface IDeployTargets : INugetHelper, IGithubReleaseHelper, ISetupBuildInfo
{
    [ParamDefinition("nuget-push-feed", "The Nuget feed to push to.")]
    string NugetFeed => GetParam(() => NugetFeed, "https://api.nuget.org/v3/index.json");

    [SecretDefinition("nuget-push-api-key", "The API key to use to push to Nuget.")]
    string NugetApiKey => GetParam(() => NugetApiKey)!;

    static readonly string[] ProjectsToPush =
    [
        Projects.DecSm_Atom.Name,
        Projects.DecSm_Atom_Module_AzureKeyVault.Name,
        Projects.DecSm_Atom_Module_AzureStorage.Name,
        Projects.DecSm_Atom_Module_DevopsWorkflows.Name,
        Projects.DecSm_Atom_Module_Dotnet.Name,
        Projects.DecSm_Atom_Module_GitVersion.Name,
        Projects.DecSm_Atom_Module_GithubWorkflows.Name,
    ];

    static readonly string[] TestArtifactsToUpload =
    [
        Projects.DecSm_Atom_Tests.Name,
        Projects.DecSm_Atom_Analyzers_Tests.Name,
        Projects.DecSm_Atom_SourceGenerators_Tests.Name,
        Projects.DecSm_Atom_Module_GithubWorkflows_Tests.Name,
    ];

    Target PushToNuget =>
        t => t
            .DescribedAs("Pushes the packages to Nuget")
            .RequiresParam(nameof(NugetFeed))
            .RequiresParam(nameof(NugetApiKey))
            .ConsumesVariable(nameof(SetupBuildInfo), nameof(BuildId))
            .ConsumesArtifacts(nameof(IBuildTargets.PackProjects), ProjectsToPush)
            .ConsumesArtifact(nameof(IBuildTargets.PackTool), Projects.DecSm_Atom_Tool.Name)
            .DependsOn(nameof(ITestTargets.TestProjects))
            .Executes(async cancellationToken =>
            {
                // Push project packages
                foreach (var project in ProjectsToPush)
                    await PushProject(project, NugetFeed, NugetApiKey, cancellationToken: cancellationToken);

                // Push Atom tool package - platform-specific + multi-targeted
                foreach (var atomToolPackagePath in FileSystem.Directory.GetFiles(
                             FileSystem.AtomArtifactsDirectory / Projects.DecSm_Atom_Tool.Name,
                             "*.nupkg",
                             SearchOption.AllDirectories))
                    await PushPackageToNuget(
                        FileSystem.AtomArtifactsDirectory / Projects.DecSm_Atom_Tool.Name / atomToolPackagePath,
                        NugetFeed,
                        NugetApiKey,
                        cancellationToken: cancellationToken);
            });

    Target PushToRelease =>
        d => d
            .DescribedAs("Pushes the packages to the release feed.")
            .RequiresParam(nameof(GithubToken))
            .ConsumesVariable(nameof(SetupBuildInfo), nameof(BuildVersion))
            .ConsumesArtifacts(nameof(IBuildTargets.PackProjects), ProjectsToPush)
            .ConsumesArtifact(nameof(IBuildTargets.PackTool), Projects.DecSm_Atom_Tool.Name)
            .ConsumesArtifacts(nameof(ITestTargets.TestProjects), TestArtifactsToUpload)
            .Executes(async () =>
            {
                foreach (var artifact in ProjectsToPush.Concat(TestArtifactsToUpload))
                    await UploadArtifactToRelease(artifact, $"v{BuildVersion}");
            });
}
