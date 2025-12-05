namespace DecSm.Atom.Module.GithubWorkflows;

/// <summary>
///     Represents a workflow option for specifying a custom snapshot image for GitHub Actions runners.
/// </summary>
/// <remarks>
///     This option allows users to define a specific Docker image and an optional version
///     to be used as the runner environment for a GitHub Actions job, enabling custom
///     toolchains or environments.
/// </remarks>
[PublicAPI]
public sealed record GithubSnapshotImageOption : WorkflowOption<GithubSnapshotImageOption.GithubSnapshotImageValues,
    GithubSnapshotImageOption>
{
    /// <inheritdoc />
    public override bool AllowMultiple => false;

    /// <summary>
    ///     Defines the values for specifying a custom snapshot image.
    /// </summary>
    /// <param name="ImageName">The name of the Docker image to use (e.g., "ubuntu-22.04").</param>
    /// <param name="Version">An optional version tag for the Docker image (e.g., "latest", "20231026").</param>
    public sealed record GithubSnapshotImageValues(string ImageName, string? Version);
}
