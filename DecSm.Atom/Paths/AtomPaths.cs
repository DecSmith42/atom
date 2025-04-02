namespace DecSm.Atom.Paths;

/// <summary>
///     Provides constants representing key paths used within the Atom system.
/// </summary>
[PublicAPI]
public static class AtomPaths
{
    /// <summary>
    ///     See <see cref="IAtomFileSystem.AtomRootDirectory" />.
    /// </summary>
    public const string Root = "Root";

    /// <summary>
    ///     See <see cref="IAtomFileSystem.AtomArtifactsDirectory" />.
    /// </summary>
    public const string Artifacts = "Artifacts";

    /// <summary>
    ///     See <see cref="IAtomFileSystem.AtomPublishDirectory" />.
    /// </summary>
    public const string Publish = "Publish";

    /// <summary>
    ///     See <see cref="IAtomFileSystem.AtomTempDirectory" />.
    /// </summary>
    public const string Temp = "Temp";

    /// <summary>
    ///     Registers a default path provider with the specified service collection.
    /// </summary>
    /// <param name="services">The service collection used for dependency injection.</param>
    /// <param name="locate">A function that resolves a <see cref="RootedPath" /> based on a key and a locator function.</param>
    /// <param name="priority">The priority of the path provider, defaulting to 1. Higher priority takes precedence.</param>
    public static void ProvidePath(
        this IServiceCollection services,
        Func<string, Func<string, RootedPath>, RootedPath?> locate,
        int priority = 1) =>
        services.AddSingleton<IPathProvider>(new PathProvider
        {
            Priority = priority,
            Locator = locate,
        });
}
