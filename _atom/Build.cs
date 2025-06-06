namespace Atom;

[BuildDefinition]
[GenerateEntryPoint]
internal partial class Build : DefaultBuildDefinition,
    IAzureKeyVault,
    IAzureArtifactStorage,
    IDevopsWorkflows,
    IGithubWorkflows,
    IGitVersion,
    IDeployTargets,
    ITestTargets,
    ICleanupPrereleaseArtifacts
{
    public override IReadOnlyList<IWorkflowOption> GlobalWorkflowOptions =>
    [
        UseAzureKeyVault.Enabled, UseGitVersionForBuildId.Enabled, new DevopsVariableGroup("Atom"), new SetupDotnetStep("9.0.x"),
    ];

    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        // Real workflows
        new("Validate")
        {
            Triggers = [GitPullRequestTrigger.IntoMain],
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
                Targets.TestAtom.WithGithubRunnerMatrix([
                    IJobRunsOn.WindowsLatestTag, IJobRunsOn.UbuntuLatestTag, IJobRunsOn.MacOsLatestTag,
                ]),
            ],
            WorkflowTypes = [Github.WorkflowType],
        },
        new("Build")
        {
            Triggers = [GitPushTrigger.ToMain],
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
                Targets.TestAtom.WithGithubRunnerMatrix([
                    IJobRunsOn.WindowsLatestTag, IJobRunsOn.UbuntuLatestTag, IJobRunsOn.MacOsLatestTag,
                ]),
                Targets.PushToNuget,
                Targets.PushToRelease.WithGithubTokenInjection(),
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
                        Values = [IJobRunsOn.WindowsLatestTag, IJobRunsOn.UbuntuLatestTag, IJobRunsOn.MacOsLatestTag],
                    })
                    .WithOptions(DevopsPool.SetByMatrix, GithubRunsOn.SetByMatrix),
            ],
            WorkflowTypes = [Devops.WorkflowType],
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
                Targets.TestAtom.WithDevopsPoolMatrix([
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
                    .WithMatrixDimensions(new MatrixDimension(nameof(IJobRunsOn.JobRunsOn))
                    {
                        Values = [IJobRunsOn.WindowsLatestTag, IJobRunsOn.UbuntuLatestTag, IJobRunsOn.MacOsLatestTag],
                    })
                    .WithOptions(DevopsPool.SetByMatrix, GithubRunsOn.SetByMatrix),
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
            Targets = [Targets.CleanupPrereleaseArtifacts],
            WorkflowTypes = [Github.WorkflowType],
        },
        Github.DependabotDefaultWorkflow(),
    ];
}
