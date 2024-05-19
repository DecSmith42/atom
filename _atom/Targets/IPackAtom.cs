namespace Atom.Targets;

[TargetDefinition]
internal partial interface IPackAtom : IDotnetPackHelper
{
    public const string AtomProjectName = "DecSm.Atom";
    
    Target PackAtom =>
        d => d
            .Produces(AtomProjectName)
            .Executes(() => DotnetPackProject(AtomProjectName));
}