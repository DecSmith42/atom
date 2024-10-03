namespace DecSm.Atom.Module.DevopsWorkflows.Triggers;

public sealed record DevopsScheduleTrigger(string CronExpression) : IWorkflowTrigger;
