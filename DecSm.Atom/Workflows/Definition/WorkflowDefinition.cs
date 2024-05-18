namespace DecSm.Atom.Workflows.Definition;

public sealed record WorkflowDefinition(string Name, IReadOnlyList<IWorkflowStepDefinition> StepDefinitions, IReadOnlyList<BatWorkflowType> WorkflowTypes);