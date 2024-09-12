namespace DecSm.Atom.Workflows.Definition;

public sealed record WorkflowVaultSecretInjection(string Param) : IWorkflowOption
{
    public bool AllowMultiple => true;
}
