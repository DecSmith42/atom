namespace DecSm.Atom.Artifacts;

[TargetDefinition]
public partial interface IArtifactHelper
{
    [ParamDefinition("atom-artifacts", "The name of the artifact/s to work with, use ',' to separate multiple artifacts")]
    string[] AtomArtifacts => GetParam(() => AtomArtifacts, []);
}
