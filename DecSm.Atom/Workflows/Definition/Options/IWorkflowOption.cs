namespace DecSm.Atom.Workflows.Definition.Options;

/// <summary>
///     Represents a workflow option that can be applied to control workflow behavior and configuration.
///     Workflow options provide a way to customize and extend workflow execution through various settings,
///     parameters, and custom steps.
/// </summary>
/// <remarks>
///     <para>
///         This interface serves as the base for all workflow configuration options in the DecSm.Atom workflow system.
///         Implementations can represent various types of workflow customizations including:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>Custom workflow steps (via <see cref="CustomStep" /> implementations)</description>
///         </item>
///         <item>
///             <description>Parameter injections (via <see cref="WorkflowParamInjection" />)</description>
///         </item>
///         <item>
///             <description>Workflow behavior modifications</description>
///         </item>
///     </list>
///     <para>
///         The interface provides built-in support for option merging and deduplication through the
///         <see cref="GetOptionsForCurrentTarget" /> and <see cref="Merge{T}" /> methods.
///     </para>
/// </remarks>
[PublicAPI]
public interface IWorkflowOption
{
    /// <summary>
    ///     Gets a value indicating whether multiple instances of this workflow option type are allowed
    ///     to coexist in the same workflow configuration.
    /// </summary>
    /// <value>
    ///     <c>true</c> if multiple instances of this option type can be present simultaneously;
    ///     otherwise, <c>false</c>. The default value is <c>false</c>.
    /// </value>
    /// <remarks>
    ///     <para>
    ///         When <c>false</c>, only the last occurrence of this option type will be retained during merging.
    ///         When <c>true</c>, all instances will be preserved, allowing for scenarios like multiple
    ///         custom steps or parameter injections.
    ///     </para>
    ///     <para>
    ///         Examples of options that typically allow multiple instances:
    ///         <list type="bullet">
    ///             <item>
    ///                 <description><see cref="CustomStep" /> - Multiple custom steps can be added to a workflow</description>
    ///             </item>
    ///             <item>
    ///                 <description><see cref="WorkflowParamInjection" /> - Multiple parameters can be injected</description>
    ///             </item>
    ///         </list>
    ///     </para>
    /// </remarks>
    public bool AllowMultiple => false;

    /// <summary>
    ///     Retrieves and merges all workflow options applicable to the currently running workflows
    ///     within the specified build definition.
    /// </summary>
    /// <param name="buildDefinition">The build definition containing default options and active workflows.</param>
    /// <returns>
    ///     An enumerable collection of merged workflow options from both the build definition's
    ///     default options and the options of currently running workflows.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         This method combines options from two sources:
    ///     </para>
    ///     <list type="number">
    ///         <item>
    ///             <description>Default workflow options from the build definition</description>
    ///         </item>
    ///         <item>
    ///             <description>Options from workflows that have at least one running workflow type</description>
    ///         </item>
    ///     </list>
    ///     <para>
    ///         The resulting options are then merged using the <see cref="Merge{T}" /> method to handle
    ///         deduplication and multiple instance scenarios based on each option's <see cref="AllowMultiple" /> property.
    ///     </para>
    /// </remarks>
    /// <seealso cref="Merge{T}" />
    public static IEnumerable<IWorkflowOption> GetOptionsForCurrentTarget(IBuildDefinition buildDefinition) =>
        Merge(buildDefinition.GlobalWorkflowOptions.Concat(buildDefinition
            .Workflows
            .Where(workflow => workflow.WorkflowTypes.Any(workflowType => workflowType.IsRunning))
            .SelectMany(workflow => workflow.Options)));

    /// <summary>
    ///     Merges a collection of workflow options of the same type, handling deduplication
    ///     and multiple instance scenarios based on each option's <see cref="AllowMultiple" /> property.
    /// </summary>
    /// <typeparam name="T">The specific type of workflow option being merged. Must implement <see cref="IWorkflowOption" />.</typeparam>
    /// <param name="entries">The collection of workflow options to merge.</param>
    /// <returns>
    ///     An enumerable collection of merged workflow options with duplicates resolved according
    ///     to each option type's <see cref="AllowMultiple" /> configuration.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         The merging process works as follows:
    ///     </para>
    ///     <list type="number">
    ///         <item>
    ///             <description>Options are grouped by their exact type</description>
    ///         </item>
    ///         <item>
    ///             <description>For each group, the first option's <see cref="MergeWith{T}" /> method is called</description>
    ///         </item>
    ///         <item>
    ///             <description>If <see cref="AllowMultiple" /> is <c>true</c>, all instances are preserved</description>
    ///         </item>
    ///         <item>
    ///             <description>If <see cref="AllowMultiple" /> is <c>false</c>, only the last instance is kept</description>
    ///         </item>
    ///     </list>
    ///     <para>
    ///         This method ensures that workflow option collections maintain consistency and prevent
    ///         unintended duplicates while allowing controlled multiple instances where appropriate.
    ///     </para>
    /// </remarks>
    /// <seealso cref="AllowMultiple" />
    /// <seealso cref="MergeWith{T}" />
    public static IEnumerable<T> Merge<T>(IEnumerable<T> entries)
        where T : IWorkflowOption =>
        entries
            .GroupBy(x => x.GetType())
            .SelectMany(x => x
                .First()
                .MergeWith(x.Skip(1)));

    /// <summary>
    ///     Merges the current workflow option instance with additional instances of the same type,
    ///     respecting the <see cref="AllowMultiple" /> configuration.
    /// </summary>
    /// <typeparam name="T">The specific type of workflow option being merged. Must implement <see cref="IWorkflowOption" />.</typeparam>
    /// <param name="entries">Additional instances of the same option type to merge with the current instance.</param>
    /// <returns>
    ///     An enumerable collection containing either all instances (if <see cref="AllowMultiple" /> is <c>true</c>)
    ///     or just the last instance (if <see cref="AllowMultiple" /> is <c>false</c>).
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         This method implements the core merging logic for workflow options:
    ///     </para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>When <see cref="AllowMultiple" /> is <c>true</c>: Returns the current instance followed by all provided entries</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 When <see cref="AllowMultiple" /> is <c>false</c>: Returns only the last entry, or the current instance if no
    ///                 entries are provided
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>When no additional entries are provided: Always returns the current instance</description>
    ///         </item>
    ///     </list>
    ///     <para>
    ///         This method is called internally by <see cref="Merge{T}" /> and can be overridden in derived types
    ///         to provide custom merging behavior (as seen in <see cref="WorkflowParamInjection" />).
    ///     </para>
    /// </remarks>
    /// <seealso cref="AllowMultiple" />
    /// <seealso cref="Merge{T}" />
    private IEnumerable<T> MergeWith<T>(IEnumerable<T> entries)
        where T : IWorkflowOption =>
        entries.ToArray() is { Length: > 0 } entriesArray
            ? AllowMultiple
                ? entriesArray.Prepend((T)this)
                : [entriesArray[^1]]
            : [(T)this];
}
