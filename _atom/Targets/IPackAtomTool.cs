namespace Atom.Targets;

[TargetDefinition]
internal partial interface IPackAtomTool : IDotnetPackHelper
{
    public const string AtomToolProjectName = "DecSm.Atom.Tool";
    
    Target PackAtomTool =>
        d => d
            .Produces(AtomToolProjectName)
            .Executes(() => DotnetPackProject(AtomToolProjectName));
}