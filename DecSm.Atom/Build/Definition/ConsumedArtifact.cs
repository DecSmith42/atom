namespace DecSm.Atom.Build.Definition;

/// <summary>
///     Represents an artifact that is consumed by a target during the build process.
/// </summary>
/// <param name="TargetName">The name of the target that produced the artifact.</param>
/// <param name="ArtifactName">The name of the artifact being consumed.</param>
[PublicAPI]
public sealed record ConsumedArtifact(string TargetName, string ArtifactName);