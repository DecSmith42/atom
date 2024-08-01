namespace DecSm.Atom.Extensions.GithubWorkflows.Triggers;

public sealed record GithubScheduleTrigger(string CronExpression) : IWorkflowTrigger;