namespace Atom.Targets;

[TargetDefinition]
internal partial interface IPackAtomTool : IDotnetPackHelper
{
    public const string AtomToolProjectName = "DecSm.Atom.Tool";

    Target PackAtomTool =>
        d => d
            .WithDescription("Builds the Atom.Tool project into a nuget package")
            .ProducesArtifact(AtomToolProjectName)
            .Executes(() => DotnetPackProject(AtomToolProjectName));
}