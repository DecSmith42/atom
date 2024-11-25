namespace DecSm.Atom.Module.GithubWorkflows.Triggers;

[PublicAPI]
public sealed record GithubScheduleTrigger(string CronExpression) : IWorkflowTrigger;
