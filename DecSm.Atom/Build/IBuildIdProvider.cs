namespace DecSm.Atom.Build;

public interface IBuildIdProvider
{
    string BuildId { get; }

    string? GetBuildIdGroup(string buildId) =>
        null;
}
