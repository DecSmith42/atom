namespace DecSm.Atom.GithubWorkflows.Triggers;

public sealed record GithubManualStringInput(string Name, string Description, bool Required, string? DefaultValue = null)
    : ManualInput(Name, Description, Required);