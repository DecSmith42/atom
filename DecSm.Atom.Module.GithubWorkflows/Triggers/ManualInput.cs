namespace DecSm.Atom.Module.GithubWorkflows.Triggers;

public abstract record ManualInput(string Name, string Description, bool Required = false);
