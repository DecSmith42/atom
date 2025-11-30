namespace DecSm.Atom.Module.GithubWorkflows;

[PublicAPI]
public sealed record
    GithubCheckoutOption : WorkflowOption<GithubCheckoutOption.GithubCheckoutOptionValues, GithubCheckoutOption>
{
    [PublicAPI]
    public sealed record GithubCheckoutOptionValues(
        string Version = "v4",
        bool Lfs = false,
        string? Submodules = null,
        string? Token = null
    );

    public override bool AllowMultiple => false;
}
