namespace DecSm.Atom.Nuget;

/// <summary>
///     Custom step to add configured nuget feeds to the build agent nuget.config.
///     Attach to a job definition to run this first before the job.
/// </summary>
[PublicAPI]
public sealed record AddNugetFeedsStep : CustomStep
{
    public IReadOnlyList<NugetFeedOptions> FeedsToAdd { get; init; } = [];

    public static string GetEnvVarNameForFeed(string feedName) =>
        $"NUGET_TOKEN_{feedName.Replace(" ", "_").ToUpper()}";
}
