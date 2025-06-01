namespace Atom;

[BuildDefinition]
[GenerateEntryPoint]
internal partial class Build : DefaultBuildDefinition,
    IAzureKeyVault,
    IAzureArtifactStorage,
    IDevopsWorkflows,
    IGithubWorkflows,
    IGitVersion,
    IPackAtom,
    IPackAtomTool,
    IPackAzureKeyVaultModule,
    IPackAzureStorageModule,
    IPackDevopsWorkflowsModule,
    IPackDotnetModule,
    IPackGithubWorkflowsModule,
    IPackGitVersionModule,
    IPushToNuget,
    ITestAtom,
    ICleanupPrereleaseArtifacts,
    ITestManualParams,
    ITestSecretProvider
{
    private static AddNugetFeedsStep AddNugetFeedsStep =>
        new()
        {
            Name = "Update NuGet Feeds",
            FeedsToAdd =
            [
                new()
                {
                    FeedName = "DecSm",
                    FeedUrl = "https://nuget.pkg.github.com/DecSmith42/index.json",
                    SecretName = "PRIVATE_NUGET_API_KEY",
                },
            ],
        };

    public override IReadOnlyList<IWorkflowOption> DefaultWorkflowOptions =>
    [
        UseAzureKeyVault.Enabled, UseGitVersionForBuildId.Enabled, new DevopsVariableGroup("Atom"), new SetupDotnetStep("9.0.x"),
    ];

    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        // Real workflows
        new("Validate")
        {
            Triggers = [GitPullRequestTrigger.IntoMain],
            StepDefinitions =
            [
                Targets.SetupBuildInfo,
                Targets.PackAtom.WithSuppressedArtifactPublishing,
                Targets.PackAtomTool.WithSuppressedArtifactPublishing,
                Targets.PackAzureKeyVaultModule.WithSuppressedArtifactPublishing,
                Targets.PackAzureStorageModule.WithSuppressedArtifactPublishing,
                Targets.PackDevopsWorkflowsModule.WithSuppressedArtifactPublishing,
                Targets.PackDotnetModule.WithSuppressedArtifactPublishing,
                Targets.PackGithubWorkflowsModule.WithSuppressedArtifactPublishing,
                Targets
                    .TestAtom
                    .WithAddedMatrixDimensions(new MatrixDimension(nameof(IJobRunsOn.JobRunsOn),
                        [IJobRunsOn.WindowsLatestTag, IJobRunsOn.UbuntuLatestTag, IJobRunsOn.MacOsLatestTag]))
                    .WithAddedOptions(DevopsPool.SetByMatrix, GithubRunsOn.SetByMatrix),
            ],
            WorkflowTypes = [Github.WorkflowType],
        },
        new("Build")
        {
            Triggers = [GitPushTrigger.ToMain],
            StepDefinitions =
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
                Targets.TestAtom.WithGithubRunnerMatrix([
                    IJobRunsOn.WindowsLatestTag, IJobRunsOn.UbuntuLatestTag, IJobRunsOn.MacOsLatestTag,
                ]),
                Targets.PushToNuget,
            ],
            WorkflowTypes = [Github.WorkflowType],
        },

        // Test workflows
        new("Test_ManualParams")
        {
            Triggers =
            [
                new ManualTrigger
                {
                    Inputs = [ManualStringInput.ForParam(ParamDefinitions[Params.TestStringParam])],
                },
            ],
            StepDefinitions = [Targets.TestManualParams],
            WorkflowTypes = [Github.WorkflowType, Devops.WorkflowType],
        },
        new("Test_Devops_Validate")
        {
            StepDefinitions =
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
                    .WithAddedMatrixDimensions(new MatrixDimension(nameof(IJobRunsOn.JobRunsOn),
                        [IJobRunsOn.WindowsLatestTag, IJobRunsOn.UbuntuLatestTag, IJobRunsOn.MacOsLatestTag]))
                    .WithAddedOptions(DevopsPool.SetByMatrix, GithubRunsOn.SetByMatrix),
            ],
            WorkflowTypes = [Devops.WorkflowType],
        },
        new("Test_Devops_Build")
        {
            StepDefinitions =
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
                Targets.TestAtom.WithGithubRunnerMatrix([
                    IJobRunsOn.WindowsLatestTag, IJobRunsOn.UbuntuLatestTag, IJobRunsOn.MacOsLatestTag,
                ]),
                Targets.PushToNuget,
            ],
            WorkflowTypes = [Devops.WorkflowType],
            Options = [new WorkflowParamInjection(Params.NugetDryRun, "true")],
        },
        new("Test_BuildWithCustomArtifacts")
        {
            Triggers = [GitPullRequestTrigger.IntoMain],
            StepDefinitions =
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
                Targets.TestAtom.WithGithubRunnerMatrix([
                    IJobRunsOn.WindowsLatestTag, IJobRunsOn.UbuntuLatestTag, IJobRunsOn.MacOsLatestTag,
                ]),
                Targets.PushToNuget,
            ],
            WorkflowTypes = [Github.WorkflowType, Devops.WorkflowType],
            Options = [UseCustomArtifactProvider.Enabled, new WorkflowParamInjection(Params.NugetDryRun, "true")],
        },
        new("CleanupBuilds")
        {
            Triggers =
            [
                ManualTrigger.Empty,
                new GitPushTrigger
                {
                    IncludedTags = ["v[0-9]+.[0-9]+.[0-9]+"],
                },
            ],
            StepDefinitions = [Targets.CleanupPrereleaseArtifacts],
            WorkflowTypes = [Github.WorkflowType],
        },
        Github.DependabotDefaultWorkflow(),
    ];
}
