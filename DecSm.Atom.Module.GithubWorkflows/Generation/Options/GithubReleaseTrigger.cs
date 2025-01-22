namespace DecSm.Atom.Module.GithubWorkflows.Generation.Options;

[PublicAPI]
public sealed record GithubReleaseTrigger : IWorkflowTrigger
{
    public IReadOnlyList<string> Types { get; init; } = [];

    public static GithubReleaseTrigger OnReleased { get; } = new()
    {
        Types = ["released"],
    };

    public static GithubReleaseTrigger OnPublished { get; } = new()
    {
        Types = ["published"],
    };
}
