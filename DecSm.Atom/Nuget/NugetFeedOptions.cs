namespace DecSm.Atom.Nuget;

/// <summary>
///     Represents configuration options for a NuGet feed that needs to be added to a build workflow.
///     This class is used by the Atom build system to configure additional NuGet package sources
///     during the build process, typically for private or custom NuGet feeds that require authentication.
/// </summary>
/// <remarks>
///     <para>
///         This record is commonly used in workflow configurations where builds need access to
///         private NuGet repositories. The feed will be configured with the specified URL and
///         authenticated using credentials stored under the provided secret name.
///         <br /><br />
///         Example usage in build configuration:
///     </para>
///     <code>
/// new NugetFeedOptions
/// {
///     FeedName = "DecSm",
///     FeedUrl = "https://nuget.pkg.github.com/DecSmith42/index.json",
///     SecretName = "PRIVATE_NUGET_API_KEY"
/// }
/// </code>
/// </remarks>
[PublicAPI]
public sealed record NugetFeedOptions
{
    /// <summary>
    ///     Gets the display name for the NuGet feed.
    ///     This name is used to identify the feed in build logs and configuration.
    /// </summary>
    /// <value>A human-readable name for the NuGet feed.</value>
    public required string FeedName { get; init; }

    /// <summary>
    ///     Gets the URL of the NuGet feed.
    ///     This should be the complete URL to the NuGet package source endpoint.
    /// </summary>
    /// <value>The complete URL to the NuGet feed endpoint.</value>
    public required string FeedUrl { get; init; }

    /// <summary>
    ///     Gets the name of the secret that contains the authentication credentials for the NuGet feed.
    ///     This secret name is used to retrieve the API key or authentication token from the
    ///     workflow's secret store (e.g., GitHub Secrets, Azure DevOps Variables).
    /// </summary>
    /// <value>The name of the secret containing the NuGet feed authentication credentials.</value>
    public required string SecretName { get; init; }
}
