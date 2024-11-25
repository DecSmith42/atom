namespace DecSm.Atom.Module.GithubWorkflows.Triggers;

[PublicAPI]
public abstract record ManualInput(string Name, string Description, bool Required = false);
