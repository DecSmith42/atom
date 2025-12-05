namespace DecSm.Atom.Paths;

/// <summary>
///     Provides constants for key directory paths and an extension method for path provider registration.
/// </summary>
[PublicAPI]
public static class AtomPaths
{
    /// <summary>
    ///     Represents the key for the root directory of the Atom project.
    /// </summary>
    /// <seealso cref="IAtomFileSystem.AtomRootDirectory" />
    public const string Root = "Root";

    /// <summary>
    ///     Represents the key for the directory where build artifacts are stored.
    /// </summary>
    /// <seealso cref="IAtomFileSystem.AtomArtifactsDirectory" />
    public const string Artifacts = "Artifacts";

    /// <summary>
    ///     Represents the key for the directory where build outputs are published.
    /// </summary>
    /// <seealso cref="IAtomFileSystem.AtomPublishDirectory" />
    public const string Publish = "Publish";

    /// <summary>
    ///     Represents the key for the temporary directory used during builds.
    /// </summary>
    /// <seealso cref="IAtomFileSystem.AtomTempDirectory" />
    public const string Temp = "Temp";

    /// <summary>
    ///     Registers a custom path provider with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add the provider to.</param>
    /// <param name="locate">A function that resolves a <see cref="RootedPath" /> based on a key.</param>
    /// <param name="priority">The priority of the provider. Higher values take precedence.</param>
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
