namespace Atom;

[DefaultBuildDefinition]
internal partial class Build : DefaultBuildDefinition,
    IAzureKeyVault,
    IDevopsWorkflows,
    IGithubWorkflows,
    IGitVersion,
    IBuildTargets,
    ITestTargets,
    IDeployTargets
{
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
                Targets.SetupBuildInfo,
                Targets.PackProjects.WithSuppressedArtifactPublishing,
                Targets.PackTool.WithSuppressedArtifactPublishing,
                Targets
                    .TestProjects
                    .WithGithubRunnerMatrix(IBuildTargets.BuildPlatformNames)
                    .WithOptions(new SetupDotnetStep("8.0.x"), new SetupDotnetStep("9.0.x")),
            ],
            WorkflowTypes = [Github.WorkflowType],
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
                Targets.SetupBuildInfo,
                Targets.PackProjects,
                Targets.PackTool.WithGithubRunnerMatrix(IBuildTargets.BuildPlatformNames),
                Targets
                    .TestProjects
                    .WithGithubRunnerMatrix(IBuildTargets.BuildPlatformNames)
                    .WithOptions(new SetupDotnetStep("8.0.x"), new SetupDotnetStep("9.0.x")),
                Targets.PushToNuget,
                Targets
                    .PushToRelease
                    .WithGithubTokenInjection()
                    .WithOptions(GithubIf.Create(new ConsumedVariableExpression(nameof(Targets.SetupBuildInfo),
                            ParamDefinitions[nameof(ISetupBuildInfo.BuildVersion)].ArgName)
                        .Contains(new StringExpression("-"))
                        .EqualTo("false"))),
            ],
            WorkflowTypes = [Github.WorkflowType],
        },

        // Test workflows
        new("Test_Devops_Validate")
        {
            Targets =
            [
                Targets.SetupBuildInfo,
                Targets.PackProjects.WithSuppressedArtifactPublishing,
                Targets.PackTool.WithSuppressedArtifactPublishing,
                Targets
                    .TestProjects
                    .WithMatrixDimensions(new MatrixDimension(nameof(IJobRunsOn.JobRunsOn))
                    {
                        Values = IBuildTargets.BuildPlatformNames,
                    })
                    .WithOptions(DevopsPool.SetByMatrix, GithubRunsOn.SetByMatrix)
                    .WithOptions(new SetupDotnetStep("8.0.x"), new SetupDotnetStep("9.0.x")),
            ],
            WorkflowTypes = [Devops.WorkflowType],
            Options = [new DevopsVariableGroup("Atom")],
        },
        new("Test_Devops_Build")
        {
            Targets =
            [
                Targets.SetupBuildInfo,
                Targets.PackProjects,
                Targets.PackTool,
                Targets
                    .TestProjects
                    .WithDevopsPoolMatrix(IBuildTargets.BuildPlatformNames)
                    .WithOptions(new SetupDotnetStep("8.0.x"), new SetupDotnetStep("9.0.x")),
                Targets.PushToNuget,
            ],
            WorkflowTypes = [Devops.WorkflowType],
            Options = [new WorkflowParamInjection(Params.NugetDryRun, "true"), new DevopsVariableGroup("Atom")],
        },
        Github.DependabotDefaultWorkflow(),
    ];
}
