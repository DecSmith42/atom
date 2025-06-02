namespace DecSm.Atom.Workflows.Definition.Triggers;

/// <summary>
///     Represents a trigger for a GitHub workflow that fires based on a defined CRON schedule.
/// </summary>
/// <remarks>
///     This trigger is used to automatically run a workflow at specified times or intervals,
///     as dictated by the provided CRON expression. It is typically used within a workflow
///     definition to specify when the workflow should be initiated by GitHub Actions.
///     The validity and interpretation of the CRON expression are subject to GitHub's CRON parser.
/// </remarks>
/// <param name="CronExpression">
///     A string representing the CRON schedule for the trigger.
///     For example, "0 0 * * *" triggers an event at midnight (UTC) every day.
///     Refer to GitHub Actions documentation for the specific CRON syntax supported.
/// </param>
[PublicAPI]
public sealed record GithubScheduleTrigger(string CronExpression) : IWorkflowTrigger;
