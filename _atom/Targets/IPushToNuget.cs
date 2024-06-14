namespace Atom.Targets;

[TargetDefinition]
public partial interface IPushToNuget : INugetHelper
{
    [ParamDefinition("nuget-feed", "The Nuget feed to push to.", "https://api.nuget.org/v3/index.json")]
    string NugetFeed => GetParam(() => NugetFeed) ?? "https://api.nuget.org/v3/index.json";

    [SecretDefinition("nuget-api-key", "The API key to use to push to Nuget.")]
    string NugetApiKey => GetParam(() => NugetApiKey)!;

    Target PushToNuget =>
        d => d
            .WithDescription("Pushes the Atom projects to Nuget")
            .ConsumesArtifact<IPackAtom>(IPackAtom.AtomProjectName)
            .ConsumesArtifact<IPackAtomGithubWorkflows>(IPackAtomGithubWorkflows.AtomGithubWorkflowsProjectName)
            .ConsumesArtifact<IPackAtomSourceGenerators>(IPackAtomSourceGenerators.AtomSourceGeneratorsProjectName)
            .ConsumesArtifact<IPackAtomTool>(IPackAtomTool.AtomToolProjectName)
            .RequiresParam(Build.Params.NugetFeed)
            .RequiresParam(Build.Secrets.NugetApiKey)
            .Executes(async () =>
            {
                await PushProject(IPackAtom.AtomProjectName, NugetFeed, NugetApiKey);
                await PushProject(IPackAtomSourceGenerators.AtomSourceGeneratorsProjectName, NugetFeed, NugetApiKey);
                await PushProject(IPackAtomGithubWorkflows.AtomGithubWorkflowsProjectName, NugetFeed, NugetApiKey);
                await PushProject(IPackAtomTool.AtomToolProjectName, NugetFeed, NugetApiKey);
            });
}