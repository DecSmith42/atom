namespace DecSm.Atom.Paths;

public interface IPathProvider
{
    int Priority { get; }

    AbsolutePath? Locate(string key, Func<string, AbsolutePath> locator);
}

internal sealed class PathProvider : IPathProvider
{
    public required Func<string, Func<string, AbsolutePath>, AbsolutePath?> Locator { get; init; }

    public required int Priority { get; init; }

    public AbsolutePath? Locate(string key, Func<string, AbsolutePath> locator) =>
        Locator(key, locator);
}
