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
    IDeployTargets
{
    public static readonly string[] PlatformNames =
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
                    .WithGithubTokenInjection()
                    .WithOptions(GithubIf.Create(new ConsumedVariableExpression(nameof(WorkflowTargets.SetupBuildInfo),
                            ParamDefinitions[nameof(ISetupBuildInfo.BuildVersion)].ArgName)
                        .Contains(new StringExpression("-"))
                        .EqualTo("false"))),
            ],
            WorkflowTypes = [Github.WorkflowType],
        },

        // Test workflows
        new("Test_Devops_Build")
        {
            Triggers = [ManualTrigger.Empty, GitPullRequestTrigger.IntoMain],
            Targets =
            [
                WorkflowTargets.SetupBuildInfo,
                WorkflowTargets.PackProjects,
                WorkflowTargets.PackTool.WithGithubRunnerMatrix(PlatformNames),
                WorkflowTargets
                    .TestProjects
                    .WithDevopsPoolMatrix(PlatformNames)
                    .WithMatrixDimensions(TestFrameworkMatrix)
                    .WithOptions(new SetupDotnetStep("8.0.x"), new SetupDotnetStep("9.0.x")),
                WorkflowTargets.PushToNuget,
            ],
            WorkflowTypes = [Devops.WorkflowType],
            Options = [new WorkflowParamInjection(Params.NugetDryRun, "true"), new DevopsVariableGroup("Atom")],
        },
        Github.DependabotDefaultWorkflow(),
    ];
}
