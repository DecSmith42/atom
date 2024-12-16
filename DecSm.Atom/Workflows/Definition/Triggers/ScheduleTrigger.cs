namespace DecSm.Atom.Workflows.Definition.Triggers;

[PublicAPI]
public sealed record GithubScheduleTrigger(string CronExpression) : IWorkflowTrigger;
