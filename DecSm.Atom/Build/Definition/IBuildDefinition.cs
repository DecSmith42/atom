namespace DecSm.Atom.Build.Definition;

/// <summary>
///     Represents the primary interface for defining build processes within the Atom framework.
///     Recommended to inherit from specialized implementations such as <see cref="BuildDefinition" /> or
///     <see cref="DefaultBuildDefinition" /> instead of directly using this interface.
/// </summary>
/// <example>
///     <code>
/// [BuildDefinition]
/// internal partial class Build : BuildDefinition
/// {
///     public override IReadOnlyList&lt;WorkflowDefinition&gt; Workflows =>
///     [
///         new("Build")
///         {
///             Triggers = [...],
///             StepDefinitions =
///             [
///                 Commands.Setup,
///                 Commands.BuildProject,
///                 Commands.PushToNuget,
///             ],
///             WorkflowTypes = [...],
///         },
///         new("OtherWorkflow")
///         {
///             ...
///         },
///     ];
/// }
/// </code>
/// </example>
[PublicAPI]
public interface IBuildDefinition
{
    /// <summary>
    ///     All workflows defined for a build. Inheritors should override this property to provide a list of workflows.
    /// </summary>
    IReadOnlyList<WorkflowDefinition> Workflows => [];

    /// <summary>
    ///     All targets defined for a build. Inheritors should override this property to provide a list of targets.
    /// </summary>
    IReadOnlyDictionary<string, Target> TargetDefinitions { get; }

    /// <summary>
    ///     All parameters defined for a build. Inheritors should override this property to provide a list of parameters.
    /// </summary>
    IReadOnlyDictionary<string, ParamDefinition> ParamDefinitions { get; }

    /// <summary>
    ///     Workflow options that are applied to all targets in all workflows.
    /// </summary>
    IReadOnlyList<IWorkflowOption> DefaultWorkflowOptions => [];
}
