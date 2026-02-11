namespace DecSm.Atom.Module.GithubWorkflows.Generation.Options;

[PublicAPI]
public sealed record DependabotOptions : IWorkflowOption
{
    public required IReadOnlyList<DependabotRegistry> Registries { get; init; }

    public required IReadOnlyList<DependabotUpdate> Updates { get; init; }
}

[PublicAPI]
public sealed record DependabotRegistry(string Name, string Type, string Url)
{
    public IGithubExpression? Username { get; init; }

    public IGithubExpression? Password { get; init; }

    public IGithubExpression? Token { get; init; }
}

[PublicAPI]
public sealed record DependabotUpdate(
    string Ecosystem,
    string TargetBranch = "main",
    string Directory = "/",
    int OpenPullRequestsLimit = 10,
    DependabotSchedule Schedule = DependabotSchedule.Weekly
)
{
    public IReadOnlyCollection<string> Registries { get; init; } = [];

    public IReadOnlyCollection<DependabotUpdateGroup> Groups { get; init; } = [];

    public IReadOnlyCollection<string> ExcludePaths { get; init; } = [];

    public IReadOnlyCollection<DependabotDependency> Allow { get; init; } = [];

    public IReadOnlyCollection<DependabotDependency> Ignore { get; init; } = [];
}

[PublicAPI]
public sealed record DependabotUpdateGroup(string Name)
{
    public required IReadOnlyList<string>? Patterns { get; init; }
}

[PublicAPI]
public enum DependabotSchedule
{
    Daily,
    Weekly,
    Monthly,
}

[PublicAPI]
public sealed record DependabotDependency(string DependencyName)
{
    public IReadOnlyCollection<string> Versions { get; init; } = [];

    public IReadOnlyCollection<string> UpdateTypes { get; init; } = [];
}

[PublicAPI]
public static class DependabotValues
{
    public static string NugetType => "nuget-feed";

    public static string NugetEcosystem => "nuget";

    public static string NugetUrl => "https://api.nuget.org/v3/index.json";
}
