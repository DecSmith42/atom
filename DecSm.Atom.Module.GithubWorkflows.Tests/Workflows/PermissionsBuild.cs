namespace DecSm.Atom.Module.GithubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class PermissionsBuild : MinimalBuildDefinition, IGithubWorkflows, IPermissionsTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("permissions-workflow")
        {
            Triggers =
            [
                new GitPullRequestTrigger
                {
                    IncludedBranches = ["main"],
                },
            ],
            Targets =
            [
                WorkflowTargets.PermissionsTarget.WithOptions(new GithubTokenPermissionsOption
                {
                    Actions = GithubTokenPermission.Write,
                }),
            ],
            WorkflowTypes = [Github.WorkflowType],
            Options = [GithubTokenPermissionsOption.ReadAll],
        },
    ];
}

[TargetDefinition]
public partial interface IPermissionsTarget
{
    Target PermissionsTarget => t => t;
}
