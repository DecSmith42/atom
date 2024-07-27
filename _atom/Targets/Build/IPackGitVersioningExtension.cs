namespace Atom.Targets.Build;

[TargetDefinition]
internal partial interface IPackGitVersioningExtension : IDotnetPackHelper
{
    public const string GitVersioningExtensionProjectName = "DecSm.Atom.Extensions.GitVersioning";

    Target PackGitVersioningExtension =>
        d => d
            .WithDescription("Builds the GitVersioning extension project into a nuget package")
            .ProducesArtifact(GitVersioningExtensionProjectName)
            .Executes(() => DotnetPackProject(GitVersioningExtensionProjectName));
}