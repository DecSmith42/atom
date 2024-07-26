namespace Atom.Targets;

[TargetDefinition]
internal partial interface IPushToNuget : INugetHelper, INugetCredentials
{
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