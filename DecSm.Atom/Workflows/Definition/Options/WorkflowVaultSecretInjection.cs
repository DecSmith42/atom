namespace DecSm.Atom.Workflows.Definition.Options;

[PublicAPI]
public sealed record WorkflowVaultSecretInjection : WorkflowOption<string, WorkflowVaultSecretInjection>
{
    public override bool AllowMultiple => true;
}
