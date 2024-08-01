namespace Atom.Targets.Build;

[TargetDefinition]
internal partial interface IPackGithubWorkflowsExtension : IDotnetPackHelper
{
    public const string AtomGithubWorkflowsExtensionProjectName = "DecSm.Atom.Extensions.GithubWorkflows";

    Target PackGithubWorkflowsExtension =>
        d => d
            .WithDescription("Builds the GithubWorkflows extension project into a nuget package")
            .ProducesArtifact(AtomGithubWorkflowsExtensionProjectName)
            .Executes(() => DotnetPackProject(new(AtomGithubWorkflowsExtensionProjectName)));
}