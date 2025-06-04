namespace Atom.Targets.Build;

[TargetDefinition]
internal partial interface IPackGithubWorkflowsModule : IDotnetPackHelper
{
    public const string AtomGithubWorkflowsModuleProjectName = "DecSm.Atom.Module.GithubWorkflows";

    Target PackGithubWorkflowsModule =>
        t => t
            .DescribedAs("Builds the GithubWorkflows extension project into a nuget package")
            .ProducesArtifact(AtomGithubWorkflowsModuleProjectName)
            .Executes(() => DotnetPackProject(new(AtomGithubWorkflowsModuleProjectName)));
}
