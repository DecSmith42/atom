namespace DecSm.Atom.Extensions.DevopsWorkflows.Generation;

public sealed record DevopsWorkflowType : IWorkflowType
{
    public bool IsRunning => Devops.IsDevopsPipelines;
}
