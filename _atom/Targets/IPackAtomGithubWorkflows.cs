namespace Atom.Targets;

[TargetDefinition]
internal partial interface IPackAtomGithubWorkflows : IDotnetPackHelper
{
    public const string AtomGithubWorkflowsProjectName = "DecSm.Atom.GithubWorkflows";
    
    Target PackAtomGithubWorkflows =>
        d => d
            .ProducesArtifact(AtomGithubWorkflowsProjectName)
            .Executes(() => DotnetPackProject(AtomGithubWorkflowsProjectName));
}