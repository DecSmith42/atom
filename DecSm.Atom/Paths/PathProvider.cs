namespace DecSm.Atom.Paths;

[PublicAPI]
public interface IPathProvider
{
    int Priority { get; }

    RootedPath? Locate(string key, Func<string, RootedPath> locator);
}

public sealed class PathProvider : IPathProvider
{
    public required Func<string, Func<string, RootedPath>, RootedPath?> Locator { get; init; }

    public required int Priority { get; init; }

    public RootedPath? Locate(string key, Func<string, RootedPath> locator) =>
        Locator(key, locator);
}
