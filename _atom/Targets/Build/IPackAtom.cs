namespace Atom.Targets.Build;

[TargetDefinition]
internal partial interface IPackAtom : IDotnetPackHelper
{
    public const string AtomProjectName = "DecSm.Atom";

    Target PackAtom =>
        t => t
            .DescribedAs("Builds the Atom project into a nuget package")
            .ProducesArtifact(AtomProjectName)
            .Executes(() => DotnetPackProject(new(AtomProjectName)));
}
