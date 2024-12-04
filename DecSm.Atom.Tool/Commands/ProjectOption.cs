namespace DecSm.Atom.Tool.Commands;

internal sealed record ProjectOption(string? Project);

internal sealed class ProjectOptionBinder(Option<string> projectOption) : BinderBase<ProjectOption>
{
    protected override ProjectOption GetBoundValue(BindingContext bindingContext) =>
        new(bindingContext.ParseResult.GetValueForOption(projectOption));
}
