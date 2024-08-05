namespace DecSm.Atom.Build;

public interface IBuildVersionProvider
{
    SemVer Version { get; }
}