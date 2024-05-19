namespace Atom.Targets;

[TargetDefinition]
internal partial interface IPackAtomSourceGenerators : IDotnetPackHelper
{
    public const string AtomSourceGeneratorsProjectName = "DecSm.Atom.SourceGenerators";
    
    Target PackAtomSourceGenerators =>
        d => d
            .Produces(AtomSourceGeneratorsProjectName)
            .Executes(() => DotnetPackProject(AtomSourceGeneratorsProjectName));
}