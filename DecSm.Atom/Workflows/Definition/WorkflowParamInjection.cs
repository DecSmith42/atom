namespace DecSm.Atom.Workflows.Definition;

public sealed record WorkflowParamInjection(string Name, string Value) : IWorkflowOption
{
    public IEnumerable<T> Merge<T>(IEnumerable<T> entries)
        where T : IWorkflowOption =>
        entries
            .OfType<WorkflowParamInjection>()
            .GroupBy(x => x.Name)
            .Select(x => x.Last())
            .Cast<T>();
}
