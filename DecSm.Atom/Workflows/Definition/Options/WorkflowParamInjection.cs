namespace DecSm.Atom.Workflows.Definition.Options;

[PublicAPI]
public sealed record WorkflowParamInjection(string Name, string Value) : IWorkflowOption
{
    public bool AllowMultiple => true;

    public static IEnumerable<T> MergeWith<T>(IEnumerable<T> entries)
        where T : IWorkflowOption =>
        entries
            .OfType<WorkflowParamInjection>()
            .GroupBy(x => x.Name)
            .Select(x => x.Last())
            .Cast<T>();
}
