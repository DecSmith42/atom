# Setup Build Info (`ISetupBuildInfo`)

The `ISetupBuildInfo` interface provides a standardized way to initialize and manage essential build metadata within
your Atom project. This includes critical information like the build's unique ID, its semantic version, and a consistent
timestamp. When your build definition implements `ISetupBuildInfo` (which `BuildDefinition` does by default), you gain
access to a dedicated target that ensures these values are established early in your build process.

## `ISetupBuildInfo` Interface

This interface defines a single target, `SetupBuildInfo`, which is responsible for resolving and caching core build
information.

### `SetupBuildInfo` Target

The `SetupBuildInfo` target is designed to be one of the first targets executed in your workflow. It resolves the
`BuildId`, `BuildVersion`, and `BuildTimestamp` parameters and makes them available for subsequent targets.

**What it does:**

* **Resolves `BuildId`**: Determines a unique identifier for the current build run. This can come from command-line
  arguments, environment variables (e.g., CI/CD run IDs), or a default provider.
* **Resolves `BuildVersion`**: Determines the semantic version of your build. This can be specified via parameters or
  resolved from your project's `Directory.Build.props` file.
* **Resolves `BuildTimestamp`**: Establishes a consistent timestamp for the build. This is crucial because directly
  accessing `DateTime.Now` or `DateTime.UtcNow` in different targets or processes might yield slightly different
  results. `SetupBuildInfo` ensures a single, stable timestamp is used throughout the entire build.
* **Caches Values**: Once resolved, these values are cached and made available to other targets.
* **Writes Variables**: It writes these resolved values as workflow variables, making them accessible across different
  jobs in a CI/CD pipeline.

**When to use it:**

* Always, as one of the very first targets in your build workflow.
* Whenever subsequent targets need a consistent `BuildId`, `BuildVersion`, or `BuildTimestamp`.
* Especially important in CI/CD environments where consistency across jobs is vital.

**How to use it:**
Simply include `Targets.SetupBuildInfo` in your workflow's target list. Since `BuildDefinition` already implements
`ISetupBuildInfo`, this target is readily available.

```csharp
// In your build definition (e.g., _atom/Build.cs)
[BuildDefinition]
public partial class MyBuild : BuildDefinition, IMyCustomTargets
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("CI")
        {
            Targets =
            [
                Targets.SetupBuildInfo, // Always include this early in your workflow
                Targets.MyCustomTarget,
                // ... other targets that might depend on BuildId, BuildVersion, BuildTimestamp
            ],
            // ...
        }
    ];

    // Example of a custom target consuming build info
    Target MyCustomTarget => t => t
        .DescribedAs("Uses build information.")
        .DependsOn(SetupBuildInfo) // Explicitly depend on SetupBuildInfo
        .ConsumesVariable(nameof(SetupBuildInfo), nameof(BuildId)) // Declare consumption of BuildId variable
        .ConsumesVariable(nameof(SetupBuildInfo), nameof(BuildVersion)) // Declare consumption of BuildVersion variable
        .ConsumesVariable(nameof(SetupBuildInfo), nameof(BuildTimestamp)) // Declare consumption of BuildTimestamp variable
        .Executes(() =>
        {
            Logger.LogInformation($"Build ID: {BuildId}");
            Logger.LogInformation($"Build Version: {BuildVersion}");
            Logger.LogInformation($"Build Timestamp: {BuildTimestamp}");
        });
}
```

By using `ISetupBuildInfo` and its `SetupBuildInfo` target, you ensure that your build has a consistent and reliable
foundation of metadata, which is crucial for traceability, versioning, and reporting.
