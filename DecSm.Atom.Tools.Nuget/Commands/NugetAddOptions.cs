namespace DecSm.Atom.Tools.Nuget.Commands;

public sealed record NugetAddOptions(string? FeedConnectionString)
{
    public sealed record NugetFeed(string Name, string Url);

    // Feed to add in the format: Name;Url
    public NugetFeed GetFeed()
    {
        var parts = FeedConnectionString?.Split(';') ?? throw new InvalidOperationException("FeedConnectionString is null");

        return new(parts[0], parts[1]);
    }

    public int Validate()
    {
        var hasErrors = false;

        if (FeedConnectionString is not { Length: > 0 })
        {
            Console.WriteLine("The feeds option is required.");
            hasErrors = true;
        }

        if (FeedConnectionString is { Length: > 0 } &&
            FeedConnectionString.Split(';')
                .Length is not 2)
        {
            Console.WriteLine($"Invalid feed to add '{FeedConnectionString}'. Feed to add should be of format: Name;Url");
            hasErrors = true;
        }

        return hasErrors
            ? 1
            : 0;
    }
}

public sealed class NugetAddOptionsBinder(Option<string> feedsOption) : BinderBase<NugetAddOptions>
{
    protected override NugetAddOptions GetBoundValue(BindingContext bindingContext) =>
        new(bindingContext.ParseResult.GetValueForOption(feedsOption));
}
