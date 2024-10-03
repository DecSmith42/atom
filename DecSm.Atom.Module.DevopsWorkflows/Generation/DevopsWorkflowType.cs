namespace DecSm.Atom.Module.DevopsWorkflows.Generation;

public sealed record DevopsWorkflowType : IWorkflowType
{
    public bool IsRunning => Devops.IsDevopsPipelines;
}
