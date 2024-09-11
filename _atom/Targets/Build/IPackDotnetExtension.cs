namespace Atom.Targets.Build;

[TargetDefinition]
internal partial interface IPackDotnetExtension : IDotnetPackHelper
{
    public const string DotnetExtensionProjectName = "DecSm.Atom.Extensions.Dotnet";

    Target PackDotnetExtension =>
        d => d
            .WithDescription("Builds the Dotnet extension project into a nuget package")
            .ProducesArtifact(DotnetExtensionProjectName)
            .Executes(() => DotnetPackProject(new(DotnetExtensionProjectName)));
}
