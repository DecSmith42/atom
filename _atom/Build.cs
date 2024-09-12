namespace Atom;

[BuildDefinition]
[GenerateEntryPoint]
internal partial class Build : BuildDefinition,
    IAzureKeyVault,
    IAzureArtifactStorage,
    IGithubWorkflows,
    IGitVersion,
    IPackAtom,
    IPackAtomTool,
    IPackAzureKeyVaultExtension,
    IPackAzureStorageExtension,
    IPackDotnetExtension,
    IPackGithubWorkflowsExtension,
    IPackGitVersionExtension,
    IPushToNuget,
    ITestAtom,
    ICleanupPrereleaseArtifacts
{
    public override IReadOnlyList<IWorkflowOption> DefaultWorkflowOptions => [UseAzureKeyVault.Enabled];

    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("Validate")
        {
            Triggers = [Github.Triggers.Manual, Github.Triggers.PullIntoMain],
            StepDefinitions =
            [
                Commands.PackAtom.WithSuppressedArtifactPublishing,
                Commands.PackAtomTool.WithSuppressedArtifactPublishing,
                Commands.PackAzureKeyVaultExtension.WithSuppressedArtifactPublishing,
                Commands.PackAzureStorageExtension.WithSuppressedArtifactPublishing,
                Commands.PackDotnetExtension.WithSuppressedArtifactPublishing,
                Commands.PackGithubWorkflowsExtension.WithSuppressedArtifactPublishing,
                Commands.PackGitVersionExtension.WithSuppressedArtifactPublishing,
                Commands.TestAtom.WithGithubRunnerMatrix(["windows-latest", "ubuntu-latest", "macos-latest"]),
            ],
            WorkflowTypes = [Github.WorkflowType],
        },
        new("Build")
        {
            Triggers = [Github.Triggers.Manual, Github.Triggers.PushToMain],
            StepDefinitions =
            [
                Commands.PackAtom,
                Commands.PackAtomTool,
                Commands.PackAzureKeyVaultExtension,
                Commands.PackAzureStorageExtension,
                Commands.PackDotnetExtension,
                Commands.PackGithubWorkflowsExtension,
                Commands.PackGitVersionExtension,
                Commands.TestAtom.WithGithubRunnerMatrix(["windows-latest", "ubuntu-latest", "macos-latest"]),
                Commands.PushToNuget,
            ],
            WorkflowTypes = [Github.WorkflowType],
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
            ],
            StepDefinitions = [Commands.CleanupPrereleaseArtifacts],
            WorkflowTypes = [Github.WorkflowType],
        },
        Github.DependabotWorkflow(new()
        {
            Registries = [new("nuget", DependabotValues.NugetType, DependabotValues.NugetUrl)],
            Updates =
            [
                new(DependabotValues.NugetEcosystem)
                {
                    Registries = ["nuget"],
                    Groups =
                    [
                        new("nuget-deps")
                        {
                            Patterns = ["*"],
                        },
                    ],
                },
            ],
        }),
    ];
}
