namespace DecSm.Atom.Module.GithubWorkflows.Generation.Options;

public sealed record GithubRunsOn : IWorkflowOption
{
    public IReadOnlyList<string> Labels { get; init; } = [];

    public string? Group { get; init; }

    public static GithubRunsOn WindowsLatest { get; } = new()
    {
        Labels = [IJobRunsOn.WindowsLatestTag],
    };

    public static GithubRunsOn UbuntuLatest { get; } = new()
    {
        Labels = [IJobRunsOn.UbuntuLatestTag],
    };

    public static GithubRunsOn MacOsLatest { get; } = new()
    {
        Labels = [IJobRunsOn.MacOsLatestTag],
    };

    public static GithubRunsOn SetByMatrix { get; } = new()
    {
        Labels = ["${{ matrix.job-runs-on }}"],
    };
}
