namespace DecSm.Atom.Module.LocalWorkflows;

[PublicAPI]
public sealed record PowershellWorkflowType : IWorkflowType
{
    public bool IsRunning => true;
}
