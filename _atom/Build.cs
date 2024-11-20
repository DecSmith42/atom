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
    IPushToPrivateNuget
{
    public override IReadOnlyList<IWorkflowOption> DefaultWorkflowOptions =>
    [
        UseAzureKeyVault.Enabled,
        ProvideGitVersionAsWorkflowId.Enabled,
        new DevopsVariableGroup("Atom"),
        new AddNugetFeedsStep
        {
            Name = "Update NuGet Feeds",
            FeedsToAdd =
            [
                new()
                {
                    FeedName = "DecSm",
                    FeedUrl = "https://nuget.pkg.github.com/DecSm/index.json",
                    SecretName = "PRIVATE_NUGET_API_KEY",
                },
            ],
        },
    ];

    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("Validate")
        {
            Triggers = [Github.Triggers.Manual, Github.Triggers.PullIntoMain],
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
                Commands.PackPrivateTestLib,
                Commands
                    .TestAtom
                    .WithAddedMatrixDimensions(new MatrixDimension(nameof(IJobRunsOn.JobRunsOn),
                        ["windows-latest", "ubuntu-latest", "macos-latest"]))
                    .WithAddedOptions(DevopsPool.MatrixDefined, GithubRunsOn.MatrixDefined),
            ],
            WorkflowTypes = [Github.WorkflowType, Devops.WorkflowType],
        },
        new("Build")
        {
            Triggers = [Github.Triggers.Manual, Github.Triggers.PushToMain],
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
                Commands.TestAtom.WithGithubRunnerMatrix(["windows-latest", "ubuntu-latest", "macos-latest"]),
                Commands.PushToNuget,
                Commands.PushToPrivateNuget.WithAddedOptions(new WorkflowSecretInjection(Params.PrivateNugetApiKey)),
            ],
            WorkflowTypes = [Github.WorkflowType],
        },
        new("Test_Devops_Validate")
        {
            Triggers = [Github.Triggers.Manual, Github.Triggers.PullIntoMain, Devops.Triggers.Manual],
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
                        ["windows-latest", "ubuntu-latest", "macos-latest"]))
                    .WithAddedOptions(DevopsPool.MatrixDefined, GithubRunsOn.MatrixDefined),
            ],
            WorkflowTypes = [Github.WorkflowType, Devops.WorkflowType],
        },
        new("Test_Devops_Build")
        {
            Triggers = [Github.Triggers.Manual, Github.Triggers.PushToMain, Devops.Triggers.Manual, Devops.Triggers.PushToMain],
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
                Commands.TestAtom.WithGithubRunnerMatrix(["windows-latest", "ubuntu-latest", "macos-latest"]),
                Commands.PushToNuget,
            ],
            WorkflowTypes = [Github.WorkflowType, Devops.WorkflowType],
            Options = [new WorkflowParamInjection(Params.NugetDryRun, "true")],
        },
        new("Test_BuildWithCustomArtifacts")
        {
            Triggers = [Github.Triggers.Manual, Github.Triggers.PushToMain, Devops.Triggers.PushToMain],
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
                Commands.TestAtom.WithGithubRunnerMatrix(["windows-latest", "ubuntu-latest", "macos-latest"]),
                Commands.PushToNuget,
            ],
            WorkflowTypes = [Github.WorkflowType, Devops.WorkflowType],
            Options = [UseCustomArtifactProvider.Enabled, new WorkflowParamInjection(Params.NugetDryRun, "true")],
        },
        new("Cleanup")
        {
            Triggers =
            [
                Github.Triggers.Manual,
                new GithubPushTrigger
                {
                    IncludedTags = ["v[0-9]+.[0-9]+.[0-9]+"],
                },
                new DevopsPushTrigger
                {
                    IncludedTags = ["v[0-9]+.[0-9]+.[0-9]+"],
                },
            ],
            StepDefinitions = [Commands.CleanupPrereleaseArtifacts],
            WorkflowTypes = [Github.WorkflowType, Devops.WorkflowType],
        },
        Github.DependabotDefaultWorkflow(),
    ];
}
