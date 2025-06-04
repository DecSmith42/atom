namespace Atom.Targets;

internal interface IDeployTargets : INugetHelper, IBuildTargets
{
    [ParamDefinition("nuget-push-feed", "The Nuget feed to push to.", "https://api.nuget.org/v3/index.json")]
    string NugetFeed => GetParam(() => NugetFeed)!;

    [SecretDefinition("nuget-push-api-key", "The API key to use to push to Nuget.")]
    string NugetApiKey => GetParam(() => NugetApiKey)!;

    Target PushToNuget =>
        t => t
            .DescribedAs("Pushes the Atom projects to Nuget")
            .ConsumesArtifact(nameof(PackAtom), AtomProjectName)
            .ConsumesArtifact(nameof(PackAtomTool), AtomToolProjectName)
            .ConsumesArtifact(nameof(PackAzureKeyVaultModule), AzureKeyVaultModuleProjectName)
            .ConsumesArtifact(nameof(PackAzureStorageModule), AzureStorageModuleProjectName)
            .ConsumesArtifact(nameof(PackDevopsWorkflowsModule), AtomDevopsWorkflowsModuleProjectName)
            .ConsumesArtifact(nameof(PackDotnetModule), DotnetModuleProjectName)
            .ConsumesArtifact(nameof(PackGithubWorkflowsModule), AtomGithubWorkflowsModuleProjectName)
            .ConsumesArtifact(nameof(PackGitVersionModule), GitVersionModuleProjectName)
            .RequiresParam(nameof(NugetFeed))
            .RequiresParam(nameof(NugetApiKey))
            .Executes(async cancellationToken =>
            {
                await PushProject(AtomProjectName, NugetFeed, NugetApiKey, cancellationToken: cancellationToken);
                await PushProject(AtomToolProjectName, NugetFeed, NugetApiKey, cancellationToken: cancellationToken);
                await PushProject(AzureKeyVaultModuleProjectName, NugetFeed, NugetApiKey, cancellationToken: cancellationToken);
                await PushProject(AzureStorageModuleProjectName, NugetFeed, NugetApiKey, cancellationToken: cancellationToken);
                await PushProject(AtomDevopsWorkflowsModuleProjectName, NugetFeed, NugetApiKey, cancellationToken: cancellationToken);
                await PushProject(DotnetModuleProjectName, NugetFeed, NugetApiKey, cancellationToken: cancellationToken);
                await PushProject(AtomGithubWorkflowsModuleProjectName, NugetFeed, NugetApiKey, cancellationToken: cancellationToken);
                await PushProject(GitVersionModuleProjectName, NugetFeed, NugetApiKey, cancellationToken: cancellationToken);
            });
}
