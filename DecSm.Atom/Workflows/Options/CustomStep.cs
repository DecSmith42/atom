namespace DecSm.Atom.Workflows.Options;

[PublicAPI]
public abstract record CustomStep : IWorkflowOption
{
    public string? Name { get; init; }

    public bool AllowMultiple => true;
}
