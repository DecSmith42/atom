namespace DecSm.Atom.BuildInfo;

/// <summary>
///     Provides a semantic version for the build.
/// </summary>
/// <remarks>
///     Implementations should consistently return the build version
///     following Semantic Versioning (SemVer) specifications.
/// </remarks>
/// <example>
///     Example usage:
///     <code>
/// var buildVersion = buildVersionProvider.Version;
/// // e.g., buildVersion = 1.2.3
/// </code>
/// </example>
[PublicAPI]
public interface IBuildVersionProvider
{
    /// <summary>
    ///     Gets the version of the current build.
    /// </summary>
    SemVer Version { get; }
}
