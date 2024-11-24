namespace Atom;

[BuildDefinition]
[GenerateEntryPoint]
internal partial class Build : BuildDefinition,
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
    ITestPrivateNugetRestore
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
        UseAzureKeyVault.Enabled, ProvideGitVersionAsWorkflowId.Enabled, new DevopsVariableGroup("Atom"),
    ];

    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        // Real workflows
        new("Validate")
        {
            Triggers = [Github.Triggers.PullIntoMain],
            StepDefinitions =
            [
                Commands.Setup,
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
            WorkflowTypes = [Github.WorkflowType],
        },
        new("Build")
        {
            Triggers = [Github.Triggers.PushToMain],
            StepDefinitions =
            [
                Commands.Setup,
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
        new("Test_ValidatePrivateNugetFeed")
        {
            Triggers = [Github.Triggers.PullIntoMain],
            StepDefinitions =
            [
                Commands.Setup, Commands.PackPrivateTestLib, Commands.TestPrivateNugetRestore.WithAddedOptions(AddNugetFeedsStep),
            ],
            WorkflowTypes = [Github.WorkflowType, Devops.WorkflowType],
        },
        new("Test_BuildPrivateNugetFeed")
        {
            Triggers = [Github.Triggers.PullIntoMain],
            StepDefinitions =
            [
                Commands.Setup,
                Commands.TestPrivateNugetRestore.WithAddedOptions(AddNugetFeedsStep),
                Commands.PushToPrivateNuget.WithAddedOptions(new WorkflowSecretInjection(Params.PrivateNugetApiKey)),
            ],
            WorkflowTypes = [Github.WorkflowType, Devops.WorkflowType],
            Options = [new WorkflowParamInjection(Params.NugetDryRun, "true")],
        },
        new("Test_Devops_Validate")
        {
            StepDefinitions =
            [
                Commands.Setup,
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
                Commands.Setup,
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
            Triggers = [Github.Triggers.PullIntoMain],
            StepDefinitions =
            [
                Commands.Setup,
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
                Github.Triggers.Manual,
                new GithubPushTrigger
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
