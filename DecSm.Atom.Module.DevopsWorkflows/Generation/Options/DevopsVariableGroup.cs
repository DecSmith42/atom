namespace DecSm.Atom.Module.DevopsWorkflows.Generation.Options;

public record DevopsVariableGroup(string Name) : IWorkflowOption
{
    public bool AllowMultiple => true;
}
