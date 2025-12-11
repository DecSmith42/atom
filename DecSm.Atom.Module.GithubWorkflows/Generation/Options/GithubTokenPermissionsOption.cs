namespace DecSm.Atom.Module.GithubWorkflows.Generation.Options;

[PublicAPI]
public sealed record GithubTokenPermissionsOption : IWorkflowOption
{
    public GithubTokenPermission? Actions { get; init; }

    public GithubTokenPermission? Attestations { get; init; }

    public GithubTokenPermission? Checks { get; init; }

    public GithubTokenPermission? Contents { get; init; }

    public GithubTokenPermission? Deployments { get; init; }

    public GithubTokenPermission? IdToken { get; init; }

    public GithubTokenPermission? Issues { get; init; }

    public GithubTokenPermission? Discussions { get; init; }

    public GithubTokenPermission? Packages { get; init; }

    public GithubTokenPermission? Pages { get; init; }

    public GithubTokenPermission? PullRequests { get; init; }

    public GithubTokenPermission? SecurityEvents { get; init; }

    public GithubTokenPermission? Statuses { get; init; }

    public static GithubTokenPermissionsOption NoneAll { get; } = new()
    {
        Actions = GithubTokenPermission.None,
        Attestations = GithubTokenPermission.None,
        Checks = GithubTokenPermission.None,
        Contents = GithubTokenPermission.None,
        Deployments = GithubTokenPermission.None,
        IdToken = GithubTokenPermission.None,
        Issues = GithubTokenPermission.None,
        Discussions = GithubTokenPermission.None,
        Packages = GithubTokenPermission.None,
        Pages = GithubTokenPermission.None,
        PullRequests = GithubTokenPermission.None,
        SecurityEvents = GithubTokenPermission.None,
        Statuses = GithubTokenPermission.None,
    };

    public static GithubTokenPermissionsOption ReadAll { get; } = new()
    {
        Actions = GithubTokenPermission.Read,
        Attestations = GithubTokenPermission.Read,
        Checks = GithubTokenPermission.Read,
        Contents = GithubTokenPermission.Read,
        Deployments = GithubTokenPermission.Read,
        IdToken = GithubTokenPermission.Read,
        Issues = GithubTokenPermission.Read,
        Discussions = GithubTokenPermission.Read,
        Packages = GithubTokenPermission.Read,
        Pages = GithubTokenPermission.Read,
        PullRequests = GithubTokenPermission.Read,
        SecurityEvents = GithubTokenPermission.Read,
        Statuses = GithubTokenPermission.Read,
    };

    public static GithubTokenPermissionsOption WriteAll { get; } = new()
    {
        Actions = GithubTokenPermission.Write,
        Attestations = GithubTokenPermission.Write,
        Checks = GithubTokenPermission.Write,
        Contents = GithubTokenPermission.Write,
        Deployments = GithubTokenPermission.Write,
        IdToken = GithubTokenPermission.Write,
        Issues = GithubTokenPermission.Write,
        Discussions = GithubTokenPermission.Write,
        Packages = GithubTokenPermission.Write,
        Pages = GithubTokenPermission.Write,
        PullRequests = GithubTokenPermission.Write,
        SecurityEvents = GithubTokenPermission.Write,
        Statuses = GithubTokenPermission.Write,
    };

    public List<(string, string)> GetStrings =>
        new List<(string, string?)>
            {
                ("actions", GetTokenPermissionString(Actions)),
                ("attestations", GetTokenPermissionString(Attestations)),
                ("checks", GetTokenPermissionString(Checks)),
                ("contents", GetTokenPermissionString(Contents)),
                ("deployments", GetTokenPermissionString(Deployments)),
                ("id-token", GetTokenPermissionString(IdToken)),
                ("issues", GetTokenPermissionString(Issues)),
                ("discussions", GetTokenPermissionString(Discussions)),
                ("packages", GetTokenPermissionString(Packages)),
                ("pages", GetTokenPermissionString(Pages)),
                ("pull-requests", GetTokenPermissionString(PullRequests)),
                ("security-events", GetTokenPermissionString(SecurityEvents)),
                ("statuses", GetTokenPermissionString(Statuses)),
            }
            .Where(x => x.Item2 is not null)
            .Select(x => (x.Item1, x.Item2!))
            .ToList();

    private string? GetTokenPermissionString(GithubTokenPermission? permission) =>
        permission switch
        {
            GithubTokenPermission.None => "none",
            GithubTokenPermission.Read => "read",
            GithubTokenPermission.Write => "write",
            null => null,
            _ => throw new ArgumentOutOfRangeException(nameof(permission), permission, null),
        };
}

[PublicAPI]
public enum GithubTokenPermission
{
    None,
    Read,
    Write,
}
