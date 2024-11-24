namespace DecSm.Atom.Build;

public interface IBuildIdProvider
{
    string? BuildId { get; }

    string? GetBuildIdPathPrefix(string buildId) =>
        null;
}
