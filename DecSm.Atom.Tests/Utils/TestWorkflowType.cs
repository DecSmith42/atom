namespace DecSm.Atom.Tests.Utils;

public sealed record TestWorkflowType : IWorkflowType
{
    public bool IsRunning { get; set; } = true;
}
