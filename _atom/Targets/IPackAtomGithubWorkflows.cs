namespace Atom.Targets;

[TargetDefinition]
internal interface IPackAtomGithubWorkflows : IDotnetPackHelper
{
    public const string AtomGithubWorkflowsProjectName = "DecSm.Atom.GithubWorkflows";
    
    Target PackAtomGithubWorkflows =>
        d => d
            .Produces(AtomGithubWorkflowsProjectName)
            .Executes(() => DotnetPackProject(AtomGithubWorkflowsProjectName));
}