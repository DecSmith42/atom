namespace Atom.Targets.Build;

[TargetDefinition]
internal partial interface IPackGitVersionModule : IDotnetPackHelper
{
    public const string GitVersionModuleProjectName = "DecSm.Atom.Module.GitVersion";

    Target PackGitVersionModule =>
        d => d
            .DescribedAs("Builds the GitVersion extension project into a nuget package")
            .ProducesArtifact(GitVersionModuleProjectName)
            .Executes(() => DotnetPackProject(new(GitVersionModuleProjectName)));
}
