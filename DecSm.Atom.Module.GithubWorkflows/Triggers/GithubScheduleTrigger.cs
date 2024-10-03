namespace DecSm.Atom.Module.GithubWorkflows.Triggers;

public sealed record GithubScheduleTrigger(string CronExpression) : IWorkflowTrigger;
