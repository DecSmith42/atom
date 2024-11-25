namespace DecSm.Atom.Workflows.Definition.Options;

[PublicAPI]
public sealed record WorkflowSecretInjection(string Param) : IWorkflowOption
{
    public bool AllowMultiple => true;
}
