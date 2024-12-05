namespace DecSm.Atom.Workflows.Definition.Options;

[PublicAPI]
public sealed record WorkflowEnvironmentInjection : WorkflowOption<string, WorkflowEnvironmentInjection>
{
    public override bool AllowMultiple => true;
}
