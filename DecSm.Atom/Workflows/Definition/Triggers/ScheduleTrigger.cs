namespace DecSm.Atom.Workflows.Definition.Triggers;

/// <summary>
///     Represents a workflow trigger that fires based on a defined CRON schedule.
/// </summary>
/// <param name="CronExpression">
///     A string representing the CRON schedule for the trigger (e.g., "0 0 * * *" for daily midnight UTC).
/// </param>
/// <remarks>
///     This trigger is used to automatically run a workflow at specified times or intervals.
///     The validity and interpretation of the CRON expression are subject to the specific CI/CD platform's parser.
/// </remarks>
[PublicAPI]
public sealed record GithubScheduleTrigger(string CronExpression) : IWorkflowTrigger;
