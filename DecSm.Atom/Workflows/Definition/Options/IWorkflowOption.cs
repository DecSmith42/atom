namespace DecSm.Atom.Workflows.Definition.Options;

[PublicAPI]
public interface IWorkflowOption
{
    public bool AllowMultiple => false;

    public static IEnumerable<IWorkflowOption> GetOptionsForCurrentTarget(IBuildDefinition buildDefinition) =>
        Merge(buildDefinition.DefaultWorkflowOptions.Concat(buildDefinition
            .Workflows
            .Where(workflow => workflow.WorkflowTypes.Any(workflowType => workflowType.IsRunning))
            .SelectMany(workflow => workflow.Options)));

    public static IEnumerable<T> Merge<T>(IEnumerable<T> entries)
        where T : IWorkflowOption =>
        entries
            .GroupBy(x => x.GetType())
            .SelectMany(x => x
                .First()
                .MergeWith(x.Skip(1)));

    private IEnumerable<T> MergeWith<T>(IEnumerable<T> entries)
        where T : IWorkflowOption =>
        entries.ToArray() is { Length: > 0 } entriesArray
            ? AllowMultiple
                ? entriesArray.Prepend((T)this)
                : [entriesArray[^1]]
            : [(T)this];
}
