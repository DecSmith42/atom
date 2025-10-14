namespace Atom.Targets;

internal interface IDeployTargets : INugetHelper, IBuildTargets, IGithubReleaseHelper, ISetupBuildInfo
{
    [ParamDefinition("nuget-push-feed", "The Nuget feed to push to.")]
    string NugetFeed => GetParam(() => NugetFeed, "https://api.nuget.org/v3/index.json");

    [SecretDefinition("nuget-push-api-key", "The API key to use to push to Nuget.")]
    string NugetApiKey => GetParam(() => NugetApiKey)!;

    Target PushToNuget =>
        t => t
            .DescribedAs("Pushes the Atom projects to Nuget")
            .RequiresParam(nameof(NugetFeed))
            .RequiresParam(nameof(NugetApiKey))
            .ConsumesVariable(nameof(SetupBuildInfo), nameof(BuildId))
            .ConsumesArtifact(nameof(PackAtom), AtomProjectName)
            .ConsumesArtifact(nameof(PackAtomTool), $"{AtomToolProjectName}-{IJobRunsOn.MacOsLatestTag}")
            .ConsumesArtifact(nameof(PackAtomTool), $"{AtomToolProjectName}-{IJobRunsOn.UbuntuLatestTag}")
            .ConsumesArtifact(nameof(PackAtomTool), $"{AtomToolProjectName}-{IJobRunsOn.WindowsLatestTag}")
            .ConsumesArtifact(nameof(PackAzureKeyVaultModule), AzureKeyVaultModuleProjectName)
            .ConsumesArtifact(nameof(PackAzureStorageModule), AzureStorageModuleProjectName)
            .ConsumesArtifact(nameof(PackDevopsWorkflowsModule), AtomDevopsWorkflowsModuleProjectName)
            .ConsumesArtifact(nameof(PackDotnetModule), DotnetModuleProjectName)
            .ConsumesArtifact(nameof(PackGithubWorkflowsModule), AtomGithubWorkflowsModuleProjectName)
            .ConsumesArtifact(nameof(PackGitVersionModule), GitVersionModuleProjectName)
            .DependsOn(nameof(ITestTargets.TestAtom))
            .Executes(async cancellationToken =>
            {
                await PushProject(AtomProjectName, NugetFeed, NugetApiKey, cancellationToken: cancellationToken);
                await PushProject(AzureKeyVaultModuleProjectName, NugetFeed, NugetApiKey, cancellationToken: cancellationToken);
                await PushProject(AzureStorageModuleProjectName, NugetFeed, NugetApiKey, cancellationToken: cancellationToken);
                await PushProject(AtomDevopsWorkflowsModuleProjectName, NugetFeed, NugetApiKey, cancellationToken: cancellationToken);
                await PushProject(DotnetModuleProjectName, NugetFeed, NugetApiKey, cancellationToken: cancellationToken);
                await PushProject(AtomGithubWorkflowsModuleProjectName, NugetFeed, NugetApiKey, cancellationToken: cancellationToken);
                await PushProject(GitVersionModuleProjectName, NugetFeed, NugetApiKey, cancellationToken: cancellationToken);
                await PushAtomTool(cancellationToken);
            });

    Target PushToRelease =>
        d => d
            .DescribedAs("Pushes the package to the release feed.")
            .RequiresParam(nameof(GithubToken))
            .ConsumesVariable(nameof(SetupBuildInfo), nameof(BuildVersion))
            .ConsumesArtifact(nameof(PackAtom), AtomProjectName)
            .ConsumesArtifact(nameof(PackAtomTool), AtomToolProjectName)
            .ConsumesArtifact(nameof(PackAzureKeyVaultModule), AzureKeyVaultModuleProjectName)
            .ConsumesArtifact(nameof(PackAzureStorageModule), AzureStorageModuleProjectName)
            .ConsumesArtifact(nameof(PackDevopsWorkflowsModule), AtomDevopsWorkflowsModuleProjectName)
            .ConsumesArtifact(nameof(PackDotnetModule), DotnetModuleProjectName)
            .ConsumesArtifact(nameof(PackGithubWorkflowsModule), AtomGithubWorkflowsModuleProjectName)
            .ConsumesArtifact(nameof(PackGitVersionModule), GitVersionModuleProjectName)
            .DependsOn(nameof(ITestTargets.TestAtom))
            .Executes(async () =>
            {
                if (BuildVersion.IsPreRelease)
                {
                    Logger.LogInformation("Skipping release push for pre-release version");

                    return;
                }

                var releaseTag = $"v{BuildVersion}";
                await UploadArtifactToRelease(AtomProjectName, releaseTag);
                await UploadArtifactToRelease(AtomToolProjectName, releaseTag);
                await UploadArtifactToRelease(AzureKeyVaultModuleProjectName, releaseTag);
                await UploadArtifactToRelease(AzureStorageModuleProjectName, releaseTag);
                await UploadArtifactToRelease(AtomDevopsWorkflowsModuleProjectName, releaseTag);
                await UploadArtifactToRelease(DotnetModuleProjectName, releaseTag);
                await UploadArtifactToRelease(AtomGithubWorkflowsModuleProjectName, releaseTag);
                await UploadArtifactToRelease(GitVersionModuleProjectName, releaseTag);
            });

    private async Task PushAtomTool(CancellationToken cancellationToken)
    {
        var atomToolDirectory = FileSystem.AtomArtifactsDirectory / AtomToolProjectName;
        var atomToolPackagePaths = FileSystem.Directory.GetFiles(atomToolDirectory, "*.nupkg", SearchOption.AllDirectories);

        foreach (var atomToolPackagePath in atomToolPackagePaths)
            await PushPackageToNuget(atomToolDirectory / atomToolPackagePath, NugetFeed, NugetApiKey, cancellationToken: cancellationToken);
    }
}
