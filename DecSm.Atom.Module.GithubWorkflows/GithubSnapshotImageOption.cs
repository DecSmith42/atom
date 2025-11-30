namespace DecSm.Atom.Module.GithubWorkflows;

[PublicAPI]
public sealed record GithubSnapshotImageOption : WorkflowOption<GithubSnapshotImageOption.GithubSnapshotImageValues,
    GithubSnapshotImageOption>
{
    public override bool AllowMultiple => false;

    public sealed record GithubSnapshotImageValues(string ImageName, string? Version);
}
