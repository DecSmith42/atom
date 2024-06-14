namespace DecSm.Atom.Artifacts;

public static class Artifacts
{
    public static readonly UseArtifactProvider UseArtifactProvider = new();

    public static IAtomConfiguration AddFileArtifactProvider(this IAtomConfiguration atom)
    {
        atom.Builder.Services.AddSingleton<IArtifactProvider, FileArtifactProvider>();

        return atom;
    }
}