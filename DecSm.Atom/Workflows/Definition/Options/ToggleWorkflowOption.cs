namespace DecSm.Atom.Workflows.Definition.Options;

[PublicAPI]
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
