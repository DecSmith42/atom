namespace DecSm.Atom.Workflows.Definition.Options;

[PublicAPI]
public abstract record WorkflowOption<TData, TSelf> : IWorkflowOption
    where TSelf : WorkflowOption<TData, TSelf>, new()
{
    public virtual TData? Value { get; init; }

    public virtual bool AllowMultiple => false;

    public static TSelf Create(TData value) =>
        new()
        {
            Value = value,
        };
}
