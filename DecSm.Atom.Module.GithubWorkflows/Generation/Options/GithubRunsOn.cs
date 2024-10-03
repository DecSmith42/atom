namespace DecSm.Atom.Module.GithubWorkflows.Generation.Options;

public sealed record GithubRunsOn : IWorkflowOption
{
    public IReadOnlyList<string> Labels { get; init; } = [];

    public string? Group { get; init; }

    public static GithubRunsOn WindowsLatest { get; } = new()
    {
        Labels = ["windows-latest"],
    };

    public static GithubRunsOn UbuntuLatest { get; } = new()
    {
        Labels = ["ubuntu-latest"],
    };

    public static GithubRunsOn MacOsLatest { get; } = new()
    {
        Labels = ["macos-latest"],
    };

    public static GithubRunsOn MatrixDefined { get; } = new()
    {
        Labels = ["${{ matrix.job-runs-on }}"],
    };
}
