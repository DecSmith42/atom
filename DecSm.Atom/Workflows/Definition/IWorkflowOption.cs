namespace DecSm.Atom.Workflows.Definition;

public interface IWorkflowOption
{
    protected static virtual bool AllowMultiple => false;

    public static IEnumerable<IWorkflowOption> GetOptionsForCurrentTarget(IBuildDefinition buildDefinition) =>
        MergeOptions(buildDefinition.DefaultWorkflowOptions.Concat(buildDefinition
            .Workflows
            .Where(workflow => workflow.WorkflowTypes.Any(workflowType => workflowType.IsRunning))
            .SelectMany(workflow => workflow.Options)));

    static IEnumerable<T> MergeOptions<T>(IEnumerable<T> entries)
        where T : IWorkflowOption =>
        entries
            .GroupBy(x => x.GetType())
            .SelectMany(x => x
                .First()
                .Merge(x.Skip(1)));

    IEnumerable<T> Merge<T>(IEnumerable<T> entries)
        where T : IWorkflowOption =>
        entries.ToArray() is { Length: > 0 } entriesArray
            ? AllowMultiple
                ? entriesArray.Prepend((T)this)
                : [entriesArray.Last()]
            : [(T)this];
}

public abstract record WorkflowOption<TData, TSelf> : IWorkflowOption
    where TSelf : WorkflowOption<TData, TSelf>, new()
{
    public virtual TData? Value { get; init; }

    public static TSelf Create(TData value) =>
        new()
        {
            Value = value,
        };
}

public abstract record ToggleWorkflowOption<TSelf> : WorkflowOption<bool, TSelf>
    where TSelf : WorkflowOption<bool, TSelf>, new()
{
    public static readonly TSelf Enabled = new()
    {
        Value = true,
    };

    public static readonly TSelf Disabled = new()
    {
        Value = false,
    };
}
