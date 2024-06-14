namespace DecSm.Atom.GithubWorkflows.Generation.Options;

public sealed record DependabotOptions : IWorkflowOption
{
    public required IReadOnlyList<DependabotRegistry> Registries { get; init; }

    public required IReadOnlyList<DependabotUpdate> Updates { get; init; }
}

public sealed record DependabotRegistry(string Name, string Type, string Url, string? Token = null);

public sealed record DependabotUpdate(
    string Ecosystem,
    string TargetBranch = "main",
    string Directory = "/",
    int OpenPullRequestsLimit = 10,
    DependabotSchedule Schedule = DependabotSchedule.Weekly
)
{
    public required IReadOnlyCollection<string> Registries { get; init; } = [];

    public IReadOnlyCollection<DependabotUpdateGroup> Groups { get; init; } = [];
}

public sealed record DependabotUpdateGroup(string Name)
{
    public required IReadOnlyList<string>? Patterns { get; init; }
}

public enum DependabotSchedule
{
    Daily,
    Weekly,
    Monthly,
}

public static class DependabotValues
{
    public static string NugetType => "nuget-feed";

    public static string NugetEcosystem => "nuget";

    public static string NugetUrl => "https://api.nuget.org/v3/index.json";
}