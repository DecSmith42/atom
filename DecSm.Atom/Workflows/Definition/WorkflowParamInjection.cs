namespace DecSm.Atom.Workflows.Definition;

public sealed record WorkflowParamInjection(string Name, string Value, CommandDefinition? Command = null) : IWorkflowOption;