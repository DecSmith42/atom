namespace DecSm.Atom.Extensions.GithubWorkflows.Triggers;

public abstract record ManualInput(string Name, string Description, bool Required = false);