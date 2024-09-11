namespace Atom.Targets.Build;

[TargetDefinition]
internal partial interface IPackAtomTool : IDotnetPackHelper
{
    public const string AtomToolProjectName = "DecSm.Atom.Tool";

    Target PackAtomTool =>
        d => d
            .WithDescription("Builds the DecSm.Atom.Tool project into a nuget package")
            .ProducesArtifact(AtomToolProjectName)
            .Executes(() => DotnetPackProject(new(AtomToolProjectName)));
}
