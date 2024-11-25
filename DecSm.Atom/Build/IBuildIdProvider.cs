namespace DecSm.Atom.Build;

[PublicAPI]
public interface IBuildIdProvider
{
    string? BuildId { get; }

    string? GetBuildIdPathPrefix(string buildId) =>
        null;
}
