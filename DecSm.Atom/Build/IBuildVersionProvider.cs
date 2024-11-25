namespace DecSm.Atom.Build;

[PublicAPI]
public interface IBuildVersionProvider
{
    SemVer Version { get; }
}
