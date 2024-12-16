namespace DecSm.Atom.Workflows.Definition.Triggers;

[PublicAPI]
public sealed record ManualTrigger(IReadOnlyList<ManualInput>? Inputs = null) : IWorkflowTrigger
{
    public static ManualTrigger Empty { get; } = new();
}
