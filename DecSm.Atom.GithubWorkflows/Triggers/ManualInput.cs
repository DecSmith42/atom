namespace DecSm.Atom.GithubWorkflows.Triggers;

public abstract record ManualInput(string Name, string Description, bool Required = false);