namespace DecSm.Atom.Paths;

/// <summary>
///     Defines a provider for locating paths within the Atom file system.
/// </summary>
[PublicAPI]
public interface IPathProvider
{
    /// <summary>
    ///     Gets the priority of this provider. Providers with higher priority values are queried first.
    /// </summary>
    int Priority { get; }

    /// <summary>
    ///     Attempts to locate a path based on a given key.
    /// </summary>
    /// <param name="key">The key identifying the path to locate (e.g., "Root", "Artifacts").</param>
    /// <param name="locator">A function that can be used to resolve other paths by key, enabling chained lookups.</param>
    /// <returns>A <see cref="RootedPath" /> if the provider can resolve the key; otherwise, <c>null</c>.</returns>
    RootedPath? Locate(string key, Func<string, RootedPath> locator);
}

/// <summary>
///     A concrete implementation of <see cref="IPathProvider" /> that uses a delegate to locate paths.
/// </summary>
public sealed class PathProvider : IPathProvider
{
    /// <summary>
    ///     Gets the function that implements the path location logic.
    /// </summary>
    public required Func<string, Func<string, RootedPath>, RootedPath?> Locator { get; init; }

    /// <inheritdoc />
    public required int Priority { get; init; }

    /// <inheritdoc />
    public RootedPath? Locate(string key, Func<string, RootedPath> locator) =>
        Locator(key, locator);
}
