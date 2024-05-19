namespace DecSm.Atom.GithubWorkflows.Triggers;

public sealed record GithubScheduleTrigger(string CronExpression) : IWorkflowTrigger;