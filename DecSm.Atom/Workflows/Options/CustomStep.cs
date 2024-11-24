namespace DecSm.Atom.Workflows.Options;

public abstract record CustomStep : IWorkflowOption
{
    public string? Name { get; init; }

    public bool AllowMultiple => true;
}
