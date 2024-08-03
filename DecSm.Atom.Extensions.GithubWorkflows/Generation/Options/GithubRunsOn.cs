namespace DecSm.Atom.Extensions.GithubWorkflows.Generation.Options;

public sealed record GithubRunsOn(string[] Labels) : IWorkflowOption
{
    public string? Group { get; set; }

    public static GithubRunsOn WindowsLatest { get; } = new(["windows-latest"]);

    public static GithubRunsOn UbuntuLatest { get; } = new(["ubuntu-latest"]);

    public static GithubRunsOn MacOsLatest { get; } = new(["macos-latest"]);

    public static GithubRunsOn MatrixDefined { get; } = new(["${{ matrix.github-runs-on }}"]);
}