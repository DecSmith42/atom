namespace DecSm.Atom.Extensions.DevopsWorkflows.Triggers;

public sealed record DevopsScheduleTrigger(string CronExpression) : IWorkflowTrigger;
