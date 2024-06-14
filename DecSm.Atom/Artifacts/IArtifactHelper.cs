namespace DecSm.Atom.Artifacts;

[TargetDefinition]
public interface IArtifactHelper : IBuildDefinition
{
    [ParamDefinition("atom-artifacts", "The name of the artifact/s to work with. Use ';' to separate multiple artifacts.")]
    string AtomArtifacts => GetParam(() => AtomArtifacts)!;

    string[] AtomArtifactNames => AtomArtifacts.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
}