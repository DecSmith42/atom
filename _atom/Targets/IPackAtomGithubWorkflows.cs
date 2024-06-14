namespace Atom.Targets;

[TargetDefinition]
internal partial interface IPackAtomGithubWorkflows : IDotnetPackHelper
{
    public const string AtomGithubWorkflowsProjectName = "DecSm.Atom.GithubWorkflows";

    Target PackAtomGithubWorkflows =>
        d => d
            .WithDescription("Builds the Atom.GithubWorkflows project into a nuget package")
            .ProducesArtifact(AtomGithubWorkflowsProjectName)
            .Executes(() => DotnetPackProject(AtomGithubWorkflowsProjectName));
}