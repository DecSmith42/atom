namespace DecSm.Atom.Nuget;

[PublicAPI]
public sealed record NugetFeedOptions
{
    public required string FeedName { get; init; }

    public required string FeedUrl { get; init; }

    public required string SecretName { get; init; }
}
