namespace DecSm.Atom.Build;

/// <summary>
///     Provides convenient access to the current build version.
/// </summary>
/// <remarks>
///     Intended as a helper interface that simplifies obtaining the build version from the current service provider
///     context.
/// </remarks>
/// <example>
///     Example usage:
///     <code>
/// var currentVersion = versionHelper.Version;
/// // e.g., currentVersion = 1.0.0
/// </code>
/// </example>
[TargetDefinition]
public partial interface IVersionHelper
{
    /// <summary>
    ///     Gets the current build version from <see cref="IBuildVersionProvider" />.
    /// </summary>
    SemVer Version =>
        GetService<IBuildVersionProvider>()
            .Version;
}
