namespace DecSm.Atom.Build.Definition;

/// <summary>
///     Represents an artifact that is consumed by a target during the build process.
/// </summary>
/// <param name="TargetName">The name of the target that produced the artifact.</param>
/// <param name="ArtifactName">The name of the artifact being consumed.</param>
/// <param name="BuildSlice">The build slice associated with the artifact, if any.</param>
[PublicAPI]
public sealed record ConsumedArtifact(string TargetName, string ArtifactName, string? BuildSlice = null);
