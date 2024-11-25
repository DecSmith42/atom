namespace DecSm.Atom.Nuget;

[PublicAPI]
public sealed record AddNugetFeedsStep : CustomStep
{
    public IReadOnlyList<NugetFeedOptions> FeedsToAdd { get; init; } = [];

    public static string EnvVar(string feedName) =>
        $"NUGET_TOKEN_{feedName.Replace(" ", "_").ToUpper()}";
}
