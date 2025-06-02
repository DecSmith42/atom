namespace DecSm.Atom.Tool.Commands;

internal sealed record NugetAddOptions(string Name, string Url)
{
    public int Validate()
    {
        var hasErrors = false;

        if (Name is not { Length: > 0 })
        {
            Console.WriteLine("The Name option is required.");
            hasErrors = true;
        }

        // ReSharper disable once InvertIf
        if (Url is not { Length: > 0 })
        {
            Console.WriteLine("The Url option is required.");
            hasErrors = true;
        }

        return hasErrors
            ? 1
            : 0;
    }
}

internal sealed class NugetAddOptionsBinder(Option<string> nameOption, Option<string> urlOption) : BinderBase<NugetAddOptions>
{
    protected override NugetAddOptions GetBoundValue(BindingContext bindingContext) =>
        new(bindingContext.ParseResult.GetValueForOption(nameOption) ?? string.Empty,
            bindingContext.ParseResult.GetValueForOption(urlOption) ?? string.Empty);
}
