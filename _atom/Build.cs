namespace Atom;

[BuildDefinition]
[GenerateEntryPoint]
[GenerateSolutionModel]
internal partial class Build : BuildDefinition,
    IAzureKeyVault,
    IDevopsWorkflows,
    IGithubWorkflows,
    IGitVersion,
    IBuildTargets,
    ITestTargets,
    IDeployTargets,
    IApproveDependabotPr
{
    public static readonly string[] PlatformNames =
    [
        IJobRunsOn.WindowsLatestTag,
        "windows-11-arm",
        IJobRunsOn.UbuntuLatestTag,
        "ubuntu-24.04-arm",
        "macos-15-intel",
        IJobRunsOn.MacOsLatestTag,
    ];

    public static readonly string[] DevopsPlatformNames =
    [
        IJobRunsOn.WindowsLatestTag, IJobRunsOn.UbuntuLatestTag, IJobRunsOn.MacOsLatestTag,
    ];

    public static readonly string[] FrameworkNames = ["net8.0", "net9.0", "net10.0"];

    private static readonly MatrixDimension TestFrameworkMatrix = new(nameof(ITestTargets.TestFramework))
    {
        Values = FrameworkNames,
    };

    public override IReadOnlyList<IWorkflowOption> GlobalWorkflowOptions =>
    [
        UseAzureKeyVault.Enabled, UseGitVersionForBuildId.Enabled, new SetupDotnetStep("10.0.x"),
    ];

    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        // Real workflows
        new("Validate")
        {
            Triggers = [ManualTrigger.Empty, GitPullRequestTrigger.IntoMain],
            Targets =
            [
                WorkflowTargets.SetupBuildInfo,
                WorkflowTargets.PackProjects.WithSuppressedArtifactPublishing,
                WorkflowTargets.PackTool.WithSuppressedArtifactPublishing.WithGithubRunnerMatrix(PlatformNames),
                WorkflowTargets
                    .TestProjects
                    .WithGithubRunnerMatrix(PlatformNames)
                    .WithMatrixDimensions(TestFrameworkMatrix)
                    .WithOptions(new SetupDotnetStep("8.0.x"), new SetupDotnetStep("9.0.x")),
            ],
            WorkflowTypes = [Github.WorkflowType],
            Options = [GithubTokenPermissionsOption.NoneAll],
        },
        new("Build")
        {
            Triggers =
            [
                ManualTrigger.Empty,
                new GitPushTrigger
                {
                    IncludedBranches = ["main", "feature/**", "patch/**"],
                },
                GithubReleaseTrigger.OnReleased,
            ],
            Targets =
            [
                WorkflowTargets.SetupBuildInfo,
                WorkflowTargets.PackProjects,
                WorkflowTargets.PackTool.WithGithubRunnerMatrix(PlatformNames),
                WorkflowTargets
                    .TestProjects
                    .WithGithubRunnerMatrix(PlatformNames)
                    .WithMatrixDimensions(TestFrameworkMatrix)
                    .WithOptions(new SetupDotnetStep("8.0.x"), new SetupDotnetStep("9.0.x")),
                WorkflowTargets.PushToNuget,
                WorkflowTargets
                    .PushToRelease
                    .WithGithubTokenInjection(new()
                    {
                        Contents = GithubTokenPermission.Write,
                    })
                    .WithOptions(GithubIf.Create(new ConsumedVariableExpression(nameof(WorkflowTargets.SetupBuildInfo),
                            ParamDefinitions[nameof(ISetupBuildInfo.BuildVersion)].ArgName)
                        .Contains(new StringExpression("-"))
                        .EqualTo("false"))),
            ],
            WorkflowTypes = [Github.WorkflowType],
            Options = [GithubTokenPermissionsOption.NoneAll],
        },
        new("Dependabot Enable auto-merge")
        {
            Triggers = [GitPullRequestTrigger.IntoMain],
            Targets =
            [
                WorkflowTargets.ApproveDependabotPr.WithGithubTokenInjection(new()
                {
                    Contents = GithubTokenPermission.Read,
                    IdToken = GithubTokenPermission.Write,
                    PullRequests = GithubTokenPermission.Write,
                }),
            ],
            WorkflowTypes = [Github.WorkflowType],
            Options =
            [
                GithubTokenPermissionsOption.NoneAll,
                GithubIf.Create(new EqualExpression("github.actor", new StringExpression("dependabot[bot]"))),
                new WorkflowParamInjection(nameof(IApproveDependabotPr.PullRequestNumber),
                    new LiteralExpression("github.event.number").Expression),
            ],
        },

        // Test workflows
        new("Test_Devops_Build")
        {
            Triggers = [ManualTrigger.Empty, GitPullRequestTrigger.IntoMain, GitPushTrigger.ToMain],
            Targets =
            [
                WorkflowTargets.SetupBuildInfo,
                WorkflowTargets.PackProjects,
                WorkflowTargets.PackTool.WithDevopsPoolMatrix(DevopsPlatformNames),
                WorkflowTargets
                    .TestProjects
                    .WithDevopsPoolMatrix(DevopsPlatformNames)
                    .WithMatrixDimensions(TestFrameworkMatrix)
                    .WithOptions(new SetupDotnetStep("8.0.x"), new SetupDotnetStep("9.0.x")),
                WorkflowTargets.PushToNugetDevops,
            ],
            WorkflowTypes = [Devops.WorkflowType],
            Options = [new WorkflowParamInjection(Params.NugetDryRun, "true"), new DevopsVariableGroup("Atom")],
        },
        Github.DependabotDefaultWorkflow(),
    ];
}
