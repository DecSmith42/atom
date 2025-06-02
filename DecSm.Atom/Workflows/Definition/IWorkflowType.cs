namespace DecSm.Atom.Workflows.Definition;

/// <summary>
///     Defines a contract for a workflow type.
/// </summary>
/// <remarks>
///     This interface is typically implemented by classes representing specific types of workflows.
///     It provides a common way to identify and interact with workflow instances, for example,
///     to determine if a workflow is currently active.
///     It is used as a generic constraint in classes like <c>WorkflowFileWriter&lt;T&gt;</c>
///     to enable writing different workflow types to files.
/// </remarks>
[PublicAPI]
public interface IWorkflowType
{
    /// <summary>
    ///     Gets a value indicating whether the workflow instance is currently running.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this workflow is running; otherwise, <c>false</c>.
    /// </value>
    bool IsRunning { get; }
}
