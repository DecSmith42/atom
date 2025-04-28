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
    IPackPrivateTestLib,
    IPushToPrivateNuget,
    ITestPrivateNugetRestore,
    IPublishTester,
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
                Commands.SetupBuildInfo,
                Commands.PackAtom.WithSuppressedArtifactPublishing,
                Commands.PackAtomTool.WithSuppressedArtifactPublishing,
                Commands.PackAzureKeyVaultModule.WithSuppressedArtifactPublishing,
                Commands.PackAzureStorageModule.WithSuppressedArtifactPublishing,
                Commands.PackDevopsWorkflowsModule.WithSuppressedArtifactPublishing,
                Commands.PackDotnetModule.WithSuppressedArtifactPublishing,
                Commands.PackGithubWorkflowsModule.WithSuppressedArtifactPublishing,
                Commands.PackGitVersionModule.WithSuppressedArtifactPublishing,
                Commands.PublishTester.WithSuppressedArtifactPublishing,
                Commands
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
                Commands.SetupBuildInfo,
                Commands.PackAtom,
                Commands.PackAtomTool,
                Commands.PackAzureKeyVaultModule,
                Commands.PackAzureStorageModule,
                Commands.PackDevopsWorkflowsModule,
                Commands.PackDotnetModule,
                Commands.PackGithubWorkflowsModule,
                Commands.PackGitVersionModule,
                Commands.PackPrivateTestLib,
                Commands.TestAtom.WithGithubRunnerMatrix([
                    IJobRunsOn.WindowsLatestTag, IJobRunsOn.UbuntuLatestTag, IJobRunsOn.MacOsLatestTag,
                ]),
                Commands.PushToNuget,
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
            StepDefinitions = [Commands.TestManualParams],
            WorkflowTypes = [Github.WorkflowType, Devops.WorkflowType],
        },
        new("Test_ValidatePrivateNugetFeed")
        {
            Triggers = [GitPullRequestTrigger.IntoMain],
            StepDefinitions =
            [
                Commands.SetupBuildInfo,
                Commands.PackPrivateTestLib,
                Commands.TestPrivateNugetRestore.WithAddedOptions(AddNugetFeedsStep),
            ],
            WorkflowTypes = [Github.WorkflowType, Devops.WorkflowType],
            Options = [new SetupDotnetStep("8.0.x")],
        },
        new("Test_BuildPrivateNugetFeed")
        {
            Triggers = [GitPullRequestTrigger.IntoMain],
            StepDefinitions =
            [
                Commands.SetupBuildInfo,
                Commands.TestPrivateNugetRestore.WithAddedOptions(AddNugetFeedsStep),
                Commands.PushToPrivateNuget.WithAddedOptions(WorkflowSecretInjection.Create(Params.PrivateNugetApiKey)),
            ],
            WorkflowTypes = [Github.WorkflowType, Devops.WorkflowType],
            Options = [new WorkflowParamInjection(Params.NugetDryRun, "true"), new SetupDotnetStep("8.0.x")],
        },
        new("Test_Devops_Validate")
        {
            StepDefinitions =
            [
                Commands.SetupBuildInfo,
                Commands.PackAtom.WithSuppressedArtifactPublishing,
                Commands.PackAtomTool.WithSuppressedArtifactPublishing,
                Commands.PackAzureKeyVaultModule.WithSuppressedArtifactPublishing,
                Commands.PackAzureStorageModule.WithSuppressedArtifactPublishing,
                Commands.PackDevopsWorkflowsModule.WithSuppressedArtifactPublishing,
                Commands.PackDotnetModule.WithSuppressedArtifactPublishing,
                Commands.PackGithubWorkflowsModule.WithSuppressedArtifactPublishing,
                Commands.PackGitVersionModule.WithSuppressedArtifactPublishing,
                Commands
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
                Commands.SetupBuildInfo,
                Commands.PackAtom,
                Commands.PackAtomTool,
                Commands.PackAzureKeyVaultModule,
                Commands.PackAzureStorageModule,
                Commands.PackDevopsWorkflowsModule,
                Commands.PackDotnetModule,
                Commands.PackGithubWorkflowsModule,
                Commands.PackGitVersionModule,
                Commands.TestAtom.WithGithubRunnerMatrix([
                    IJobRunsOn.WindowsLatestTag, IJobRunsOn.UbuntuLatestTag, IJobRunsOn.MacOsLatestTag,
                ]),
                Commands.PushToNuget,
            ],
            WorkflowTypes = [Devops.WorkflowType],
            Options = [new WorkflowParamInjection(Params.NugetDryRun, "true")],
        },
        new("Test_BuildWithCustomArtifacts")
        {
            Triggers = [GitPullRequestTrigger.IntoMain],
            StepDefinitions =
            [
                Commands.SetupBuildInfo,
                Commands.PackAtom,
                Commands.PackAtomTool,
                Commands.PackAzureKeyVaultModule,
                Commands.PackAzureStorageModule,
                Commands.PackDevopsWorkflowsModule,
                Commands.PackDotnetModule,
                Commands.PackGithubWorkflowsModule,
                Commands.PackGitVersionModule,
                Commands.TestAtom.WithGithubRunnerMatrix([
                    IJobRunsOn.WindowsLatestTag, IJobRunsOn.UbuntuLatestTag, IJobRunsOn.MacOsLatestTag,
                ]),
                Commands.PushToNuget,
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
            StepDefinitions = [Commands.CleanupPrereleaseArtifacts],
            WorkflowTypes = [Github.WorkflowType],
        },
        Github.DependabotDefaultWorkflow(),
    ];
}
