namespace DecSm.Atom.GithubWorkflows.Triggers;

public sealed record GithubManualBoolInput(string Name, string Description, bool Required, bool? DefaultValue = null)
    : ManualInput(Name, Description, Required);