namespace DecSm.Atom.Workflows.Definition.Options;

[PublicAPI]
public sealed record WorkflowSecretsEnvironmentInjection : WorkflowOption<string, WorkflowSecretsEnvironmentInjection>
{
    public override bool AllowMultiple => true;
}
