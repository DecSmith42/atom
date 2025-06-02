namespace Atom.Targets;

[TargetDefinition]
internal partial interface IPushToNuget : INugetHelper
{
    [ParamDefinition("nuget-push-feed", "The Nuget feed to push to.", "https://api.nuget.org/v3/index.json")]
    string NugetFeed => GetParam(() => NugetFeed)!;

    [SecretDefinition("nuget-push-api-key", "The API key to use to push to Nuget.")]
    string NugetApiKey => GetParam(() => NugetApiKey)!;

    Target PushToNuget =>
        d => d
            .WithDescription("Pushes the Atom projects to Nuget")
            .ConsumesArtifact(nameof(IPackAtom.PackAtom), IPackAtom.AtomProjectName)
            .ConsumesArtifact(nameof(IPackAtomTool.PackAtomTool), IPackAtomTool.AtomToolProjectName)
            .ConsumesArtifact(nameof(IPackAzureKeyVaultModule.PackAzureKeyVaultModule),
                IPackAzureKeyVaultModule.AzureKeyVaultModuleProjectName)
            .ConsumesArtifact(nameof(IPackAzureStorageModule.PackAzureStorageModule), IPackAzureStorageModule.AzureStorageModuleProjectName)
            .ConsumesArtifact(nameof(IPackDevopsWorkflowsModule.PackDevopsWorkflowsModule),
                IPackDevopsWorkflowsModule.AtomDevopsWorkflowsModuleProjectName)
            .ConsumesArtifact(nameof(IPackDotnetModule.PackDotnetModule), IPackDotnetModule.DotnetModuleProjectName)
            .ConsumesArtifact(nameof(IPackGithubWorkflowsModule.PackGithubWorkflowsModule),
                IPackGithubWorkflowsModule.AtomGithubWorkflowsModuleProjectName)
            .ConsumesArtifact(nameof(IPackGitVersionModule.PackGitVersionModule), IPackGitVersionModule.GitVersionModuleProjectName)
            .RequiresParam(nameof(NugetFeed))
            .RequiresParam(nameof(NugetApiKey))
            .Executes(async () =>
            {
                await PushProject(IPackAtom.AtomProjectName, NugetFeed, NugetApiKey);
                await PushProject(IPackAtomTool.AtomToolProjectName, NugetFeed, NugetApiKey);
                await PushProject(IPackAzureKeyVaultModule.AzureKeyVaultModuleProjectName, NugetFeed, NugetApiKey);
                await PushProject(IPackAzureStorageModule.AzureStorageModuleProjectName, NugetFeed, NugetApiKey);
                await PushProject(IPackDevopsWorkflowsModule.AtomDevopsWorkflowsModuleProjectName, NugetFeed, NugetApiKey);
                await PushProject(IPackDotnetModule.DotnetModuleProjectName, NugetFeed, NugetApiKey);
                await PushProject(IPackGithubWorkflowsModule.AtomGithubWorkflowsModuleProjectName, NugetFeed, NugetApiKey);
                await PushProject(IPackGitVersionModule.GitVersionModuleProjectName, NugetFeed, NugetApiKey);
            });
}
