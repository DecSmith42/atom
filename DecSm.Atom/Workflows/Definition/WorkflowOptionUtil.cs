namespace DecSm.Atom.Workflows.Definition;

public static class WorkflowOptionUtil
{
    public static IEnumerable<IWorkflowOption> GetOptionsForCurrentTarget(IBuildDefinition buildDefinition) =>
        MergeOptions(buildDefinition.DefaultWorkflowOptions.Concat(buildDefinition
            .Workflows
            .Where(workflow => workflow.WorkflowTypes.Any(workflowType => workflowType.IsRunning))
            .SelectMany(workflow => workflow.Options)));

    public static IEnumerable<T> MergeOptions<T>(IEnumerable<T> entries)
        where T : IWorkflowOption =>
        entries
            .GroupBy(x => x.GetType())
            .SelectMany(x => x
                .First()
                .Merge(x.Skip(1)));

    public static IEnumerable<T> Merge<T>(this T workflowOption, IEnumerable<T> entries)
        where T : IWorkflowOption =>
        entries.ToArray() is { Length: > 0 } entriesArray
            ? T.AllowMultiple
                ? entriesArray.Prepend(workflowOption)
                : [entriesArray[^1]]
            : [workflowOption];
}
