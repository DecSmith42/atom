namespace Atom.Targets;

[TargetDefinition]
internal partial interface IPushToNuget : INugetHelper
{
    [ParamDefinition("nuget-feed", "The Nuget feed to push to.", "https://api.nuget.org/v3/index.json")]
    string NugetFeed => GetParam(() => NugetFeed) ?? "https://api.nuget.org/v3/index.json";

    [SecretDefinition("nuget-api-key", "The API key to use to push to Nuget.")]
    string NugetApiKey => GetParam(() => NugetApiKey)!;

    Target PushToNuget =>
        d => d
            .WithDescription("Pushes the Atom projects to Nuget")
            .ConsumesArtifact(Commands.PackAtom, IPackAtom.AtomProjectName)
            .ConsumesArtifact(Commands.PackAtomTool, IPackAtomTool.AtomToolProjectName)
            .ConsumesArtifact(Commands.PackAzureKeyVaultExtension, IPackAzureKeyVaultExtension.AzureKeyVaultExtensionProjectName)
            .ConsumesArtifact(Commands.PackAzureStorageExtension, IPackAzureStorageExtension.AzureStorageExtensionProjectName)
            .ConsumesArtifact(Commands.PackDotnetExtension, IPackDotnetExtension.DotnetExtensionProjectName)
            .ConsumesArtifact(Commands.PackGithubWorkflowsExtension, IPackGithubWorkflowsExtension.AtomGithubWorkflowsExtensionProjectName)
            .ConsumesArtifact(Commands.PackGitVersionExtension, IPackGitVersionExtension.GitVersionExtensionProjectName)
            .RequiresParam(Params.NugetFeed)
            .RequiresParam(Params.NugetApiKey)
            .Executes(async () =>
            {
                await PushProject(IPackAtom.AtomProjectName, NugetFeed, NugetApiKey);
                await PushProject(IPackAtomTool.AtomToolProjectName, NugetFeed, NugetApiKey);
                await PushProject(IPackAzureKeyVaultExtension.AzureKeyVaultExtensionProjectName, NugetFeed, NugetApiKey);
                await PushProject(IPackAzureStorageExtension.AzureStorageExtensionProjectName, NugetFeed, NugetApiKey);
                await PushProject(IPackDotnetExtension.DotnetExtensionProjectName, NugetFeed, NugetApiKey);
                await PushProject(IPackGithubWorkflowsExtension.AtomGithubWorkflowsExtensionProjectName, NugetFeed, NugetApiKey);
                await PushProject(IPackGitVersionExtension.GitVersionExtensionProjectName, NugetFeed, NugetApiKey);
            });
}
