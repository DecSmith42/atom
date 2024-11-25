namespace DecSm.Atom.Module.DevopsWorkflows.Triggers;

[PublicAPI]
public sealed record DevopsScheduleTrigger(string CronExpression) : IWorkflowTrigger;
