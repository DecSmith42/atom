namespace DecSm.Atom.Workflows.Options;

[PublicAPI]
public sealed record SetupDotnetStep(string? DotnetVersion = null) : CustomStep;
