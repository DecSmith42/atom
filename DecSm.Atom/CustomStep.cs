namespace DecSm.Atom;

public abstract record CustomStep : IWorkflowOption
{
    public bool AllowMultiple => true;

    public string? Name { get; init; }
}

public sealed record AddNugetFeedsStep : CustomStep
{
    public IReadOnlyList<NugetFeedOptions> FeedsToAdd { get; init; } = [];

    public static string EnvVar(string feedName) =>
        $"NUGET_TOKEN_{feedName.Replace(" ", "_").ToUpper()}";
}

public sealed record NugetFeedOptions
{
    public required string FeedName { get; init; }

    public required string FeedUrl { get; init; }

    public required string SecretName { get; init; }
}
