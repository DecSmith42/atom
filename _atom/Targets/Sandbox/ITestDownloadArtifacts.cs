namespace Atom.Targets.Sandbox;

[TargetDefinition]
public partial interface ITestDownloadArtifacts
{
    Target TestDownloadArtifacts =>
        d => d
            .ConsumesArtifact<IPackAtom>(IPackAtom.AtomProjectName)
            .ConsumesArtifact<IPackAtomGithubWorkflows>(IPackAtomGithubWorkflows.AtomGithubWorkflowsProjectName)
            .ConsumesArtifact<IPackAtomSourceGenerators>(IPackAtomSourceGenerators.AtomSourceGeneratorsProjectName)
            .ConsumesArtifact<IPackAtomTool>(IPackAtomTool.AtomToolProjectName)
            .Executes(() =>
            {
                Services
                    .GetRequiredService<ILogger<ITestDownloadArtifacts>>()
                    .LogInformation(":)");
                
                return Task.CompletedTask;
            });
}