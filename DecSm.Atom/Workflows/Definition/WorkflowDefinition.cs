namespace DecSm.Atom.Workflows.Definition;

/// <summary>
///     Represents the definition of a workflow.
/// </summary>
/// <param name="Name">The name of the workflow.</param>
/// <remarks>
///     <para>
///         A <c>WorkflowDefinition</c> encapsulates all the necessary information to define a workflow,
///         including its triggers, options, steps (targets), and types.
///         <para>
///         </para>
///         Special Behaviors and Edge Cases:
///         - The <c>Name</c> property is a crucial identifier for the workflow.
///         - <c>Triggers</c>: Defines what events will start the workflow (e.g., a git push, a manual trigger). If empty, the workflow might
///         need to be invoked via other means or is a sub-workflow.
///         - <c>Options</c>: Allows for configuring various aspects of the workflow's behavior or environment.
///         - <c>StepDefinitions</c>: A list of <see cref="WorkflowTargetDefinition" /> which represent the actual tasks or operations the
///         workflow will perform. The order is generally significant.
///         - <c>WorkflowTypes</c>: Can be used to categorize or specialize workflows, potentially influencing how they are processed or where
///         they are deployed (e.g., differentiating between a standard build workflow and a Dependabot workflow).
///     </para>
///     Examples of Usage:
///     - Defining a "Build" workflow triggered by a push to the main branch:
///     <code>
///   new WorkflowDefinition("Build")
///   {
///       Triggers = [GitPushTrigger.ToMain],
///       StepDefinitions = [Targets.SetupBuildInfo, Targets.PackAtom, ...],
///       // ... other properties
///   }
///   </code>
///     - Defining a "Validate" workflow triggered by a pull request into the main branch:
///     <code>
///   new WorkflowDefinition("Validate")
///   {
///       Triggers = [GitPullRequestTrigger.IntoMain],
///       StepDefinitions = [Targets.SetupBuildInfo, Targets.PackAtom.WithSuppressedArtifactPublishing, ...],
///       // ... other properties
///   }
///   </code>
///     - Defining a workflow with manual trigger inputs:
///     <code>
///   new WorkflowDefinition("Test_ManualParams")
///   {
///       Triggers = [new ManualTrigger { Inputs = [ManualStringInput.ForParam(ParamDefinitions[Params.TestStringParam])] }],
///       StepDefinitions = [Targets.TestManualParamsTarget],
///       // ... other properties
///   }
///   </code>
///     - Defining a specialized Dependabot workflow:
///     <code>
///   WorkflowDefinition.DependabotDefaultWorkflow()
///   </code>
///     or
///     <code>
///   new WorkflowDefinition("dependabot")
///   {
///       Options = [dependabotOptions],
///       WorkflowTypes = [new DependabotWorkflowType()],
///   }
///   </code>
///     The properties <c>Triggers</c>, <c>Options</c>, <c>StepDefinitions</c>, and <c>WorkflowTypes</c> are initialized as empty lists by
///     default but can be set during object initialization.
/// </remarks>
[PublicAPI]
public sealed record WorkflowDefinition(string Name)
{
    public IReadOnlyList<IWorkflowTrigger> Triggers { get; init; } = [];

    public IReadOnlyList<IWorkflowOption> Options { get; init; } = [];

    public IReadOnlyList<WorkflowTargetDefinition> StepDefinitions { get; init; } = [];

    public IReadOnlyList<IWorkflowType> WorkflowTypes { get; init; } = [];
}
