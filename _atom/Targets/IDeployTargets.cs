namespace Atom.Targets;

internal interface IDeployTargets : INugetHelper, IGithubReleaseHelper, ISetupBuildInfo
{
    [ParamDefinition("nuget-push-feed", "The Nuget feed to push to.")]
    string NugetFeed => GetParam(() => NugetFeed, "https://api.nuget.org/v3/index.json");

    [SecretDefinition("nuget-push-api-key", "The API key to use to push to Nuget.")]
    string NugetApiKey => GetParam(() => NugetApiKey)!;

    List<RootedPath> ProjectsToPush =>
    [
        FileSystem.GetPath<Projects.DecSm_Atom>(),
        FileSystem.GetPath<Projects.DecSm_Atom_Module_AzureKeyVault>(),
        FileSystem.GetPath<Projects.DecSm_Atom_Module_AzureStorage>(),
        FileSystem.GetPath<Projects.DecSm_Atom_Module_DevopsWorkflows>(),
        FileSystem.GetPath<Projects.DecSm_Atom_Module_Dotnet>(),
        FileSystem.GetPath<Projects.DecSm_Atom_Module_DotnetCli>(),
        FileSystem.GetPath<Projects.DecSm_Atom_Module_GitVersion>(),
        FileSystem.GetPath<Projects.DecSm_Atom_Module_GithubWorkflows>(),
    ];

    Target PushToNuget =>
        t => t
            .DescribedAs("Pushes the packages to Nuget")
            .RequiresParam(nameof(NugetFeed))
            .RequiresParam(nameof(NugetApiKey))
            .ConsumesVariable(nameof(SetupBuildInfo), nameof(BuildId))
            .ConsumesArtifacts(nameof(IBuildTargets.PackProjects),
                ProjectsToPush.Select(project => project.FileNameWithoutExtension))
            .ConsumesArtifact(nameof(IBuildTargets.PackTool),
                Projects.DecSm_Atom_Tool.Name,
                IBuildTargets.BuildPlatformNames)
            .DependsOn(nameof(ITestTargets.TestProjects))
            .Executes(async cancellationToken =>
            {
                foreach (var project in ProjectsToPush)
                    await PushProject(project.FileNameWithoutExtension,
                        NugetFeed,
                        NugetApiKey,
                        cancellationToken: cancellationToken);

                await PushAtomTool(cancellationToken);
            });

    Target PushToRelease =>
        d => d
            .DescribedAs("Pushes the packages to the release feed.")
            .RequiresParam(nameof(GithubToken))
            .ConsumesVariable(nameof(SetupBuildInfo), nameof(BuildVersion))
            .ConsumesArtifacts(nameof(IBuildTargets.PackProjects),
                ProjectsToPush.Select(project => project.FileNameWithoutExtension))
            .ConsumesArtifact(nameof(IBuildTargets.PackTool), Projects.DecSm_Atom_Tool.Name)
            .DependsOn(nameof(ITestTargets.TestProjects))
            .Executes(async () =>
            {
                if (BuildVersion.IsPreRelease)
                {
                    Logger.LogInformation("Skipping release push for pre-release version");

                    return;
                }

                var releaseTag = $"v{BuildVersion}";

                foreach (var project in ProjectsToPush)
                    await UploadArtifactToRelease(project.FileNameWithoutExtension, releaseTag);

                await UploadArtifactToRelease(Projects.DecSm_Atom_Tool.Name, releaseTag);
            });

    private async Task PushAtomTool(CancellationToken cancellationToken)
    {
        var atomToolDirectory = FileSystem.AtomArtifactsDirectory / Projects.DecSm_Atom_Tool.Name;

        var atomToolPackagePaths =
            FileSystem.Directory.GetFiles(atomToolDirectory, "*.nupkg", SearchOption.AllDirectories);

        foreach (var atomToolPackagePath in atomToolPackagePaths)
            await PushPackageToNuget(atomToolDirectory / atomToolPackagePath,
                NugetFeed,
                NugetApiKey,
                cancellationToken: cancellationToken);
    }
}
