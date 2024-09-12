namespace DecSm.Atom.Workflows.Definition;

public sealed record WorkflowSecretInjection(string Param) : IWorkflowOption
{
    public bool AllowMultiple => true;
}
