namespace DecSm.Atom.Workflows.Definition;

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

public abstract record WorkflowOption<TData, TSelf> : IWorkflowOption
    where TSelf : WorkflowOption<TData, TSelf>, new()
{
    public virtual bool AllowMultiple => false;

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

    // We don't need a type parameter for this method
#pragma warning disable RCS1158
    public static bool IsEnabled(IEnumerable<IWorkflowOption> options) =>
        options
            .OfType<TSelf>()
            .Any(x => x.Value);
#pragma warning restore RCS1158
}
