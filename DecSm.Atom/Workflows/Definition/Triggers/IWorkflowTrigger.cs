namespace DecSm.Atom.Workflows.Definition.Triggers;

/// <summary>
///     Represents a trigger that can initiate a workflow.
/// </summary>
/// <remarks>
///     <para>This interface is a marker interface for different types of workflow triggers.</para>
///     <para>Implementations of this interface define the specific conditions or events that cause a workflow to start.</para>
///     <para>
///         Available trigger types include:
///         <list type="bullet">
///             <item>
///                 <term>GitPullRequestTrigger</term>
///                 <description>Triggers a workflow when a pull request is created or updated in a Git repository.</description>
///             </item>
///             <item>
///                 <term>GitPushTrigger</term>
///                 <description>Triggers a workflow when changes are pushed to a Git repository.</description>
///             </item>
///             <item>
///                 <term>ManualTrigger</term>
///                 <description>Allows a workflow to be triggered manually, optionally with inputs.</description>
///             </item>
///         </list>
///     </para>
///     <para>Workflows can be configured with a list of triggers, any of which can initiate the workflow execution.</para>
/// </remarks>
[PublicAPI]
public interface IWorkflowTrigger;
