namespace Atom.Targets.Build;

[TargetDefinition]
internal partial interface IPackGitVersionExtension : IDotnetPackHelper
{
    public const string GitVersionExtensionProjectName = "DecSm.Atom.Extensions.GitVersion";

    Target PackGitVersionExtension =>
        d => d
            .WithDescription("Builds the GitVersion extension project into a nuget package")
            .ProducesArtifact(GitVersionExtensionProjectName)
            .Executes(() => DotnetPackProject(new(GitVersionExtensionProjectName)));
}
