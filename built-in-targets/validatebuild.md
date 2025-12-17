# ValidateBuild

The `IValidateBuild` interface provides a standardized target for performing general validation checks within your Atom build process. This target is designed to encapsulate various checks that ensure the integrity, correctness, or readiness of your build artifacts, environment, or configuration.

### `IValidateBuild` Interface

This interface defines a single target, `ValidateBuild`, which serves as a placeholder for any validation logic you need to execute.

#### `ValidateBuild` Target

The `ValidateBuild` target is a flexible point in your build pipeline where you can insert custom validation steps. It's often used to ensure that prerequisites are met, outputs are as expected, or certain conditions are satisfied before proceeding to further stages like deployment.

**What it does:**

* **Encapsulates Validation Logic**: You can add any number of `Executes` blocks to this target to perform various checks.
* **Fails Build on Error**: If any of the tasks within `ValidateBuild` throw an exception (especially a `StepFailedException`), the build will fail, preventing subsequent dependent targets from running.

**When to use it:**

* To perform checks on generated artifacts (e.g., file existence, content validation).
* To verify environment configurations.
* To run static analysis tools or code quality checks.
* To ensure that certain conditions are met before a deployment.

**How to use it:** Implement `IValidateBuild` in your build definition (it's included by default in `BuildDefinition`). Then, add your validation logic to the `ValidateBuild` target. You can also make other targets depend on `ValidateBuild` to ensure validation runs before them.

```csharp
// In your build definition (e.g., _atom/Build.cs)
[BuildDefinition]
public partial class MyBuild : BuildDefinition, IMyValidationTargets
{
    // ...
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("CI")
        {
            Targets =
            [
                Targets.SetupBuildInfo,
                Targets.ValidateBuild, // Include the ValidateBuild target
                Targets.MyCustomTarget,
            ],
            // ...
        }
    ];
}

// In IMyValidationTargets.cs (or directly in MyBuild.cs if you prefer)
[TargetDefinition]
public partial interface IMyValidationTargets : IBuildAccessor, IValidateBuild
{
    // Extend the ValidateBuild target with custom checks
    Target ValidateBuild => t => t
        .Extends(ValidateBuild) // Extend the base ValidateBuild target
        .Executes(() =>
        {
            Logger.LogInformation("Performing custom validation checks...");

            // Example: Check if a critical file exists
            var requiredFile = FileSystem.AtomPublishDirectory / "my-critical-artifact.txt";
            if (!requiredFile.FileExists)
            {
                throw new StepFailedException($"Required artifact '{requiredFile}' not found!");
            }

            // Example: Check a parameter value
            if (BuildConfig == "Debug")
            {
                Logger.LogWarning("Building in Debug configuration. Ensure this is intended for release builds.");
            }
        });

    // Assuming BuildConfig is defined elsewhere, e.g., in IBuildInfo
    [ParamDefinition("build-config", "The build configuration")]
    string BuildConfig => GetParam(() => BuildConfig, "Release");
}
```

By using `IValidateBuild`, you can centralize and standardize your build validation logic, making your pipelines more robust and reliable.
