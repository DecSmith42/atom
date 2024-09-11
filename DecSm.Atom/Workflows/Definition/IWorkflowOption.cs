namespace DecSm.Atom.Workflows.Definition;

public interface IWorkflowOption
{
    public static virtual bool AllowMultiple => false;
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
