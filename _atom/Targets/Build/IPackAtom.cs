namespace Atom.Targets.Build;

[TargetDefinition]
internal partial interface IPackAtom : IDotnetPackHelper
{
    public const string AtomProjectName = "DecSm.Atom";

    Target PackAtom =>
        d => d
            .WithDescription("Builds the Atom project into a nuget package")
            .ProducesArtifact(AtomProjectName)
            .Executes(() => DotnetPackProject(new(AtomProjectName)));
}
