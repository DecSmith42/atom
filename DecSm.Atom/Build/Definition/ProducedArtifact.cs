namespace DecSm.Atom.Build.Definition;

/// <summary>
///     Represents an artifact that is produced by a target during the build process.
/// </summary>
/// <param name="ArtifactName">The name of the artifact being produced.</param>
[PublicAPI]
public sealed record ProducedArtifact(string ArtifactName, string? BuildSlice = null);
