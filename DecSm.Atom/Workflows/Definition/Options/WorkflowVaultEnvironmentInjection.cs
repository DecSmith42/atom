namespace DecSm.Atom.Workflows.Definition.Options;

[PublicAPI]
public sealed record WorkflowVaultEnvironmentInjection : WorkflowOption<string, WorkflowVaultEnvironmentInjection>
{
    public override bool AllowMultiple => true;
}
