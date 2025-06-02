namespace DecSm.Atom.Workflows.Model;

/// <summary>
///     Represents the definition of a workflow, including its name, triggers, options, and jobs.
///     This model is used to generate workflow files for CI/CD systems.
/// </summary>
[PublicAPI]
public sealed record WorkflowModel(string Name)
{
    /// <summary>
    ///     Gets the collection of triggers that define when and how the workflow is initiated.
    /// </summary>
    /// <remarks>
    ///     Triggers can include events like code pushes, pull requests, schedules, or manual dispatches.
    /// </remarks>
    public required IReadOnlyList<IWorkflowTrigger> Triggers { get; init; }

    /// <summary>
    ///     Gets the collection of options or parameters that can be configured for the workflow.
    /// </summary>
    /// <remarks>
    ///     Options can include input parameters, environment variables, or other settings
    ///     that customize the workflow's behavior.
    /// </remarks>
    public required IReadOnlyList<IWorkflowOption> Options { get; init; }

    /// <summary>
    ///     Gets the collection of jobs that define the sequence of tasks to be executed by the workflow.
    /// </summary>
    /// <remarks>
    ///     Each job can consist of multiple steps and can have dependencies on other jobs.
    /// </remarks>
    public required IReadOnlyList<WorkflowJobModel> Jobs { get; init; }
}
