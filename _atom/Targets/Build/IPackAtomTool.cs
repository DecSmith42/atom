namespace Atom.Targets.Build;

[TargetDefinition]
internal partial interface IPackAtomTool : IDotnetPackHelper
{
    public const string AtomToolProjectName = "DecSm.Atom.Tool";

    Target PackAtomTool =>
        t => t
            .DescribedAs("Builds the DecSm.Atom.Tool project into a nuget package")
            .ProducesArtifact(AtomToolProjectName)
            .Executes(() => DotnetPackProject(new(AtomToolProjectName)));
}
