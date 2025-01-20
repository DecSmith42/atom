namespace DecSm.Atom.Module.GithubWorkflows;

[PublicAPI]
public sealed record SetupDotnetStep(string? DotnetVersion = null) : CustomStep;
