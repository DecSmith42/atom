namespace DecSm.Atom.Extensions.DevopsWorkflows.Triggers;

public sealed record DevopsManualTrigger(IReadOnlyList<ManualInput>? Inputs = null) : IWorkflowTrigger;
