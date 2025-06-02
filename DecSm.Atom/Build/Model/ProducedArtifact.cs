namespace DecSm.Atom.Build.Model;

/// <summary>
///     Represents an artifact produced by a target during the build process.
/// </summary>
/// <param name="ArtifactName">The name of the artifact being produced.</param>
/// <param name="BuildSlice">The build slice that produced the artifact, if applicable.</param>
[PublicAPI]
public sealed record ProducedArtifact(string ArtifactName, string? BuildSlice = null);
