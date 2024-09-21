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
    IPackAzureKeyVaultExtension,
    IPackAzureStorageExtension,
    IPackDevopsWorkflowsExtension,
    IPackDotnetExtension,
    IPackGithubWorkflowsExtension,
    IPackGitVersionExtension,
    IPushToNuget,
    ITestAtom,
    ICleanupPrereleaseArtifacts
{
    public override IReadOnlyList<IWorkflowOption> DefaultWorkflowOptions => [UseAzureKeyVault.Enabled, new DevopsVariableGroup("Atom")];

    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("Validate")
        {
            Triggers = [Github.Triggers.Manual, Github.Triggers.PullIntoMain, Devops.Triggers.Manual],
            StepDefinitions =
            [
                Commands.Setup,
                Commands.PackAtom.WithSuppressedArtifactPublishing,
                Commands.PackAtomTool.WithSuppressedArtifactPublishing,
                Commands.PackAzureKeyVaultExtension.WithSuppressedArtifactPublishing,
                Commands.PackAzureStorageExtension.WithSuppressedArtifactPublishing,
                Commands.PackDevopsWorkflowsExtension.WithSuppressedArtifactPublishing,
                Commands.PackDotnetExtension.WithSuppressedArtifactPublishing,
                Commands.PackGithubWorkflowsExtension.WithSuppressedArtifactPublishing,
                Commands.PackGitVersionExtension.WithSuppressedArtifactPublishing,
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
            Triggers = [Github.Triggers.Manual, Github.Triggers.PushToMain, Devops.Triggers.Manual, Devops.Triggers.PushToMain],
            StepDefinitions =
            [
                Commands.Setup,
                Commands.PackAtom,
                Commands.PackAtomTool,
                Commands.PackAzureKeyVaultExtension,
                Commands.PackAzureStorageExtension,
                Commands.PackDevopsWorkflowsExtension,
                Commands.PackDotnetExtension,
                Commands.PackGithubWorkflowsExtension,
                Commands.PackGitVersionExtension,
                Commands.TestAtom.WithGithubRunnerMatrix(["windows-latest", "ubuntu-latest", "macos-latest"]),
                Commands.PushToNuget,
            ],
            WorkflowTypes = [Github.WorkflowType, Devops.WorkflowType],
        },
        new("BuildWithCustomArtifacts")
        {
            Triggers = [Github.Triggers.Manual, Github.Triggers.PushToMain, Devops.Triggers.PushToMain],
            StepDefinitions =
            [
                Commands.Setup,
                Commands.PackAtom,
                Commands.PackAtomTool,
                Commands.PackAzureKeyVaultExtension,
                Commands.PackAzureStorageExtension,
                Commands.PackDevopsWorkflowsExtension,
                Commands.PackDotnetExtension,
                Commands.PackGithubWorkflowsExtension,
                Commands.PackGitVersionExtension,
                Commands.TestAtom.WithGithubRunnerMatrix(["windows-latest", "ubuntu-latest", "macos-latest"]),
                Commands.PushToNuget,
            ],
            WorkflowTypes = [Github.WorkflowType, Devops.WorkflowType],
            Options = [UseCustomArtifactProvider.Enabled],
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
