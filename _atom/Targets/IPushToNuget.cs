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
            .ConsumesArtifact<IPackAtom>(IPackAtom.AtomProjectName)
            .ConsumesArtifact<IPackAtomTool>(IPackAtomTool.AtomToolProjectName)
            .ConsumesArtifact<IPackAzureKeyVaultExtension>(IPackAzureKeyVaultExtension.AzureKeyVaultExtensionProjectName)
            .ConsumesArtifact<IPackAzureStorageExtension>(IPackAzureStorageExtension.AzureStorageExtensionProjectName)
            .ConsumesArtifact<IPackDotnetExtension>(IPackDotnetExtension.DotnetExtensionProjectName)
            .ConsumesArtifact<IPackGithubWorkflowsExtension>(IPackGithubWorkflowsExtension.AtomGithubWorkflowsExtensionProjectName)
            .ConsumesArtifact<IPackGitVersionExtension>(IPackGitVersionExtension.GitVersionExtensionProjectName)
            .RequiresParam(Params.NugetFeed)
            .RequiresParam(Secrets.NugetApiKey)
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