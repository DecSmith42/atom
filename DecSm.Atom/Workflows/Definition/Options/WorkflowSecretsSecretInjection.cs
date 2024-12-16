namespace DecSm.Atom.Workflows.Definition.Options;

[PublicAPI]
public sealed record WorkflowSecretsSecretInjection : WorkflowOption<string, WorkflowSecretsSecretInjection>
{
    public override bool AllowMultiple => true;
}
