namespace DecSm.Atom.Module.GithubWorkflows;

/// <summary>
///     Represents a workflow option for configuring the `actions/checkout` step in GitHub Actions.
/// </summary>
/// <remarks>
///     This option allows customization of the checkout action, such as specifying the version,
///     enabling LFS, handling submodules, and providing a custom token.
/// </remarks>
[PublicAPI]
public sealed record
    GithubCheckoutOption : WorkflowOption<GithubCheckoutOption.GithubCheckoutOptionValues, GithubCheckoutOption>
{
    /// <inheritdoc />
    public override bool AllowMultiple => false;

    /// <summary>
    ///     Defines the configurable values for the `actions/checkout` step.
    /// </summary>
    /// <param name="Version">The version of the `actions/checkout` action to use (e.g., "v4").</param>
    /// <param name="Lfs">Whether to enable Git LFS support.</param>
    /// <param name="Submodules">How to handle submodules (e.g., "true", "recursive", "false").</param>
    /// <param name="Token">An optional GitHub token to use for checkout, overriding the default `GITHUB_TOKEN`.</param>
    [PublicAPI]
    public sealed record GithubCheckoutOptionValues(
        string Version = "v4",
        bool Lfs = false,
        string? Submodules = null,
        string? Token = null
    );
}
