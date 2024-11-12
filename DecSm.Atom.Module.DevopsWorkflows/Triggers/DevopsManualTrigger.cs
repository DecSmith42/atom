namespace DecSm.Atom.Module.DevopsWorkflows.Triggers;

public sealed record DevopsManualTrigger(IReadOnlyList<ManualInput>? Inputs = null) : IWorkflowTrigger;
