namespace Atom.Targets.Build;

[TargetDefinition]
internal partial interface IPackAtomSourceGenerators : IDotnetPackHelper
{
    public const string AtomSourceGeneratorsProjectName = "DecSm.Atom.SourceGenerators";

    Target PackAtomSourceGenerators =>
        d => d
            .WithDescription("Builds the DecSm.Atom.SourceGenerators project into a nuget package")
            .ProducesArtifact(AtomSourceGeneratorsProjectName)
            .Executes(() => DotnetPackProject(new(AtomSourceGeneratorsProjectName)));
}