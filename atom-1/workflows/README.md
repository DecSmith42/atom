# Workflows

Atom's workflow system allows you to define how your build targets are orchestrated, especially for Continuous Integration/Continuous Delivery (CI/CD) platforms like GitHub Actions or Azure DevOps. You define your workflows in C# code, and Atom's `WorkflowGenerator` translates them into platform-specific YAML files.

### `WorkflowDefinition`

A `WorkflowDefinition` is the blueprint for a single CI/CD workflow. It encapsulates all the necessary information to define how and when your build targets should run on a specific platform.

**What it is:** A record that defines a complete CI/CD workflow, including:

* **`Name`**: The name of the workflow (e.g., "CI", "Release"). This often becomes the filename for the generated workflow file.
* **`Triggers`**: A collection of `IWorkflowTrigger` instances that specify when the workflow should start (e.g., `GitPushTrigger`, `GitPullRequestTrigger`, `ManualTrigger`).
* **`Options`**: A collection of `IWorkflowOption` instances that configure various aspects of the workflow's behavior or environment (e.g., `SetupDotnetStep`, `UseCustomArtifactProvider`).
* **`Targets`**: A collection of `WorkflowTargetDefinition` instances that specify which build targets should be executed and in what order.
* **`WorkflowTypes`**: A collection of `IWorkflowType` instances that indicate which CI/CD platforms this workflow definition applies to (e.g., `Github.WorkflowType`, `Devops.WorkflowType`).

**When to use it:**

* To define any CI/CD pipeline for your project.
* To specify how your Atom build targets should run on platforms like GitHub Actions or Azure DevOps.

**How to use it:** You define your workflows by overriding the `Workflows` property in your main build definition class.

```csharp
// In _atom/Build.cs
[BuildDefinition]
public partial class MyBuild : BuildDefinition, IMyCustomTargets
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("ContinuousIntegration") // Name of the workflow
        {
            Triggers =
            [
                GitPushTrigger.ToMain, // Trigger on pushes to 'main' branch
                GitPullRequestTrigger.IntoMain // Trigger on PRs into 'main' branch
            ],
            Options =
            [
                SetupDotnetStep.FromGlobalJson(), // Set up .NET SDK based on global.json
                UseCustomArtifactProvider.Enabled // Use Atom's custom artifact provider
            ],
            Targets =
            [
                WorkflowTargets.SetupBuildInfo, // Common setup target
                WorkflowTargets.Compile,        // Your custom compile target
                WorkflowTargets.RunTests        // Your custom test target
            ],
            WorkflowTypes =
            [
                Github.WorkflowType,    // Generate for GitHub Actions
                Devops.WorkflowType     // Generate for Azure DevOps
            ]
        },
        new("Release")
        {
            Triggers = [new ManualTrigger()], // Manual trigger
            Targets =
            [
                Targets.SetupBuildInfo,
                Targets.PackProjects,
                Targets.PushToNuget
            ],
            WorkflowTypes = [Github.WorkflowType]
        }
    ];
}
```

### `WorkflowTargetDefinition`

A `WorkflowTargetDefinition` specifies how a particular build target should be included and configured within a workflow. It allows you to customize target-specific settings for the CI/CD environment.

**What it is:** A record that wraps a build target and allows for workflow-specific configurations:

* **`Name`**: The name of the build target being included.
* **`MatrixDimensions`**: Defines dimensions for a build matrix, allowing the target to run multiple times with different configurations (e.g., different OS, .NET versions).
* **`Options`**: Target-specific `IWorkflowOption` instances that apply only to this target within the workflow.
* **`SuppressArtifactPublishing`**: A boolean flag to prevent artifacts produced by this target from being published.

**When to use it:**

* To include a specific build target in a workflow.
* To apply matrix builds to a target.
* To override artifact publishing behavior for a target within a workflow.

**How to use it:** You typically create `WorkflowTargetDefinition` instances from your `Target` properties (which are exposed via the generated `Targets` static class) and then use fluent methods to configure them.

```csharp
// In your WorkflowDefinition's Targets list
WorkflowTargets.Compile.WithMatrixDimensions(
    new MatrixDimension("os") { Values = [IJobRunsOn.WindowsLatestTag, IJobRunsOn.UbuntuLatestTag] },
    new MatrixDimension("dotnet-version") { Values = ["6.0.x", "8.0.x"] }
);

WorkflowTargets.PackProjects.WithSuppressedArtifactPublishing; // Don't publish artifacts for this specific target in this workflow
```

### `WorkflowGenerator`

The `WorkflowGenerator` is the service responsible for taking your `WorkflowDefinition`s and translating them into actual platform-specific workflow files (e.g., `.github/workflows/*.yml` for GitHub Actions).

**What it is:** A service that orchestrates the generation of workflow files. It uses registered `IWorkflowWriter` implementations to write the files for each `IWorkflowType` specified in your `WorkflowDefinition`.

**What it does:**

* **`GenerateWorkflows(CancellationToken cancellationToken = default)`**: Iterates through all `WorkflowDefinition`s in your build, resolves them into `WorkflowModel`s, and then uses the appropriate `IWorkflowWriter` to generate the corresponding workflow files.
* **`WorkflowsDirty(CancellationToken cancellationToken = default)`**: Checks if any of the generated workflow files are out of sync with their `WorkflowDefinition`s. This is useful for CI/CD systems to determine if a regeneration is needed.

**When to use it:**

* You typically don't call `WorkflowGenerator` directly. Atom's `GenArg` (triggered by `atom --gen`) uses this service to generate your workflow files.
* In CI/CD environments, you might have a step that calls `atom --gen` to ensure your workflow files are always up-to-date.

**How it works:** When you run `atom --gen`, Atom:

1. Resolves all your `WorkflowDefinition`s.
2. For each `WorkflowDefinition`, it finds the `IWorkflowWriter` that matches its `IWorkflowType`.
3. It then calls the `Generate` method on that writer, which produces the platform-specific workflow file.

By defining your workflows in C# with `WorkflowDefinition` and `WorkflowTargetDefinition`, you gain type safety, reusability, and a single source of truth for your CI/CD pipelines, all managed by Atom's `WorkflowGenerator`.
