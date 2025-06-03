namespace DecSm.Atom.Artifacts;

/// <summary>
///     Instructs the Atom build system to use a custom <see cref="IArtifactProvider" /> for artifact management
///     instead of relying on the default artifact handling mechanisms of the CI/CD platform (e.g., GitHub Actions, Azure DevOps).
/// </summary>
/// <remarks>
///     <para>
///         When <see cref="ToggleWorkflowOption{TSelf}.Enabled" />, this signals the workflow generation process to utilize
///         the <see cref="IStoreArtifact.StoreArtifact" /> and <see cref="IRetrieveArtifact.RetrieveArtifact" /> targets.
///         These targets then delegate artifact operations to the registered <see cref="IArtifactProvider" /> implementation.
///     </para>
///     <para>
///         When this option is not enabled (or explicitly <see cref="ToggleWorkflowOption{TSelf}.Disabled" />),
///         the Atom framework will typically use the native artifact storage capabilities of the underlying CI/CD platform.
///         For example, in GitHub Actions, it would use <c>actions/upload-artifact</c> and <c>actions/download-artifact</c>.
///         In Azure DevOps, it would use <c>PublishPipelineArtifact@1</c> and <c>DownloadPipelineArtifact@2</c>.
///     </para>
///     <para>
///         Enabling this option is necessary if you intend to use a specific artifact storage backend, such as Azure Blob Storage,
///         across different CI/CD platforms or for local builds, providing a consistent artifact management strategy.
///         The <see cref="WorkflowResolver" /> checks <c>UseCustomArtifactProvider.IsEnabled(workflowOptions)</c> to adjust job dependencies,
///         ensuring that artifact operations via <see cref="IArtifactProvider" /> are correctly sequenced.
///     </para>
/// </remarks>
/// <example>
///     How to enable the custom artifact provider in a workflow definition:
///     <code>
/// // In your Build.cs
/// public override IReadOnlyList&lt;WorkflowDefinition&gt; Workflows =>
/// [
///     new("MyWorkflowWithCustomArtifacts")
///     {
///         // ... other workflow configurations ...
///         Options = [UseCustomArtifactProvider.Enabled, /* other options */],
///         StepDefinitions =
///         [
///             Targets.SetupBuildInfo, // Important for providing BuildId to the artifact provider
///             Targets.PackMyProject,  // A target that produces an artifact
///             // StoreArtifact and RetrieveArtifact targets will be implicitly handled by the custom provider
///             // when UseCustomArtifactProvider.Enabled is present.
///         ],
///         WorkflowTypes = [Github.WorkflowType]
///     }
/// ];
/// </code>
///     In this example, the "Test_BuildWithCustomArtifacts" workflow has `UseCustomArtifactProvider.Enabled` in its options, meaning it will
///     use the configured `IArtifactProvider` (like `AzureBlobArtifactProvider`) for artifact handling.
/// </example>
/// <seealso cref="IArtifactProvider" />
/// <seealso cref="IStoreArtifact" />
/// <seealso cref="IRetrieveArtifact" />
[PublicAPI]
public sealed record UseCustomArtifactProvider : ToggleWorkflowOption<UseCustomArtifactProvider>;
