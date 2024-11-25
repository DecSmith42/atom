namespace DecSm.Atom.Module.DevopsWorkflows.Triggers;

[PublicAPI]
public sealed record DevopsManualTrigger(IReadOnlyList<ManualInput>? Inputs = null) : IWorkflowTrigger;
