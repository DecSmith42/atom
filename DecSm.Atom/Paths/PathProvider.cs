namespace DecSm.Atom.Paths;

/// <summary>
///     Provides a path to the Atom file system based on a given key.
/// </summary>
[PublicAPI]
public interface IPathProvider
{
    /// <summary>
    ///     The priority of this path provider. Higher priority takes precedence over lower priority.
    /// </summary>
    int Priority { get; }

    /// <summary>
    ///     Locates a rooted path in the Atom file system based on the provided key and locator function.
    /// </summary>
    /// <param name="key">The key to identify the specific path to locate.</param>
    /// <param name="locator">
    ///     A function that resolves keys to rooted paths, enabling dynamic path resolution within the file system.
    /// </param>
    /// <returns>
    ///     A <see cref="RootedPath" /> object representing the located path if found;
    ///     otherwise, null if no matching path could be resolved.
    /// </returns>
    /// <example>
    ///     <code lang="csharp">
    ///  class MyPathProvider : IPathProvider
    ///  {
    ///      public int Priority => 1;
    /// 
    ///      public RootedPath? Locate(string key, Func&lt;string, RootedPath&gt; locator)
    ///      {
    ///          // Example logic to locate a path based on the key
    ///          if (key == "example")
    ///          {
    ///              return locator("example/path");
    ///          }
    ///          return null; // No path found for the given key
    ///      }
    /// }
    ///  </code>
    /// </example>
    RootedPath? Locate(string key, Func<string, RootedPath> locator);
}

/// <inheritdoc />
public sealed class PathProvider : IPathProvider
{
    /// <summary>
    ///     Delegate used to resolve a rooted path based on a provided key and a fallback mechanism.
    ///     The function takes a string key and a fallback function, returning a <see cref="RootedPath" /> if available or null.
    /// </summary>
    public required Func<string, Func<string, RootedPath>, RootedPath?> Locator { get; init; }

    /// <inheritdoc />
    public required int Priority { get; init; }

    /// <inheritdoc />
    public RootedPath? Locate(string key, Func<string, RootedPath> locator) =>
        Locator(key, locator);
}
