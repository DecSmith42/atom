namespace Atom.Targets;

[TargetDefinition]
internal partial interface IPushToNuget : INugetHelper
{
    [ParamDefinition("nuget-feed", "The Nuget feed to push to.", "https://api.nuget.org/v3/index.json")]
    string NugetFeed => GetParam(() => NugetFeed)!;

    [SecretDefinition("nuget-api-key", "The API key to use to push to Nuget.")]
    string NugetApiKey => GetParam(() => NugetApiKey)!;

    Target PushToNuget =>
        d => d
            .WithDescription("Pushes the Atom projects to Nuget")
            .ConsumesArtifact(Commands.PackAtom, IPackAtom.AtomProjectName)
            .ConsumesArtifact(Commands.PackAtomTool, IPackAtomTool.AtomToolProjectName)
            .ConsumesArtifact(Commands.PackAzureKeyVaultModule, IPackAzureKeyVaultModule.AzureKeyVaultModuleProjectName)
            .ConsumesArtifact(Commands.PackAzureStorageModule, IPackAzureStorageModule.AzureStorageModuleProjectName)
            .ConsumesArtifact(Commands.PackDevopsWorkflowsModule, IPackDevopsWorkflowsModule.AtomDevopsWorkflowsModuleProjectName)
            .ConsumesArtifact(Commands.PackDotnetModule, IPackDotnetModule.DotnetModuleProjectName)
            .ConsumesArtifact(Commands.PackGithubWorkflowsModule, IPackGithubWorkflowsModule.AtomGithubWorkflowsModuleProjectName)
            .ConsumesArtifact(Commands.PackGitVersionModule, IPackGitVersionModule.GitVersionModuleProjectName)
            .RequiresParam(Params.NugetFeed)
            .RequiresParam(Params.NugetApiKey)
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
