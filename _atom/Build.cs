namespace Atom;

[BuildDefinition]
[GenerateEntryPoint]
[GenerateSolutionModel]
internal partial class Build : DefaultBuildDefinition,
    IAzureKeyVault,
    IDevopsWorkflows,
    IGithubWorkflows,
    IGitVersion,
    IDeployTargets,
    ITestTargets
{
    private Target Sandbox =>
        t => t.Executes(() =>
        {
            var atomProject = FileSystem.GetPath<Projects._atom>();
        });

    public override IReadOnlyList<IWorkflowOption> GlobalWorkflowOptions =>
    [
        UseAzureKeyVault.Enabled, UseGitVersionForBuildId.Enabled, new SetupDotnetStep("10.0.x", SetupDotnetStep.DotnetQuality.Preview),
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
                Targets.PackAtom.WithSuppressedArtifactPublishing,
                Targets.PackAtomTool.WithSuppressedArtifactPublishing,
                Targets.PackAzureKeyVaultModule.WithSuppressedArtifactPublishing,
                Targets.PackAzureStorageModule.WithSuppressedArtifactPublishing,
                Targets.PackDevopsWorkflowsModule.WithSuppressedArtifactPublishing,
                Targets.PackDotnetModule.WithSuppressedArtifactPublishing,
                Targets.PackGithubWorkflowsModule.WithSuppressedArtifactPublishing,
                Targets.PackGitVersionModule.WithSuppressedArtifactPublishing,
                Targets
                    .TestAtom
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
                Targets.PackAtom,
                Targets.PackAtomTool.WithGithubRunnerMatrix(IBuildTargets.BuildPlatformNames),
                Targets.PackAzureKeyVaultModule,
                Targets.PackAzureStorageModule,
                Targets.PackDevopsWorkflowsModule,
                Targets.PackDotnetModule,
                Targets.PackGithubWorkflowsModule,
                Targets.PackGitVersionModule,
                Targets
                    .TestAtom
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
                Targets.PackAtom.WithSuppressedArtifactPublishing,
                Targets.PackAtomTool.WithSuppressedArtifactPublishing,
                Targets.PackAzureKeyVaultModule.WithSuppressedArtifactPublishing,
                Targets.PackAzureStorageModule.WithSuppressedArtifactPublishing,
                Targets.PackDevopsWorkflowsModule.WithSuppressedArtifactPublishing,
                Targets.PackDotnetModule.WithSuppressedArtifactPublishing,
                Targets.PackGithubWorkflowsModule.WithSuppressedArtifactPublishing,
                Targets.PackGitVersionModule.WithSuppressedArtifactPublishing,
                Targets
                    .TestAtom
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
                Targets.PackAtom,
                Targets.PackAtomTool,
                Targets.PackAzureKeyVaultModule,
                Targets.PackAzureStorageModule,
                Targets.PackDevopsWorkflowsModule,
                Targets.PackDotnetModule,
                Targets.PackGithubWorkflowsModule,
                Targets.PackGitVersionModule,
                Targets
                    .TestAtom
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
