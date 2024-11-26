namespace DecSm.Atom.Artifacts;

/// <summary>
///     Instructs the build to use a custom artifact provider if specified in the build / workflow options.
/// </summary>
[PublicAPI]
public sealed record UseCustomArtifactProvider : ToggleWorkflowOption<UseCustomArtifactProvider>;
