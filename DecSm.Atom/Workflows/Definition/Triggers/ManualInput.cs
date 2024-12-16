namespace DecSm.Atom.Workflows.Definition.Triggers;

[PublicAPI]
public abstract record ManualInput(string Name, string Description, bool Required = true);
