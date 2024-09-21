namespace DecSm.Atom.Workflows.Definition;

public sealed record WorkflowVaultSecretInjection : WorkflowOption<string, WorkflowVaultSecretInjection>
{
    public override bool AllowMultiple => true;
}
