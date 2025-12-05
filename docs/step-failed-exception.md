# Step Failed Exception (`StepFailedException`)

In any automation process, it's crucial to clearly indicate when a step or operation has failed. Atom provides a
specific exception, `StepFailedException`, to signal such failures within your build targets. This allows for consistent
error handling and reporting across your build pipeline.

## `StepFailedException` Class

The `StepFailedException` is a custom exception designed to be thrown when a specific build step or check fails. It
inherits from `Exception` and can optionally carry additional report data.

**What it is:**
A specialized exception that indicates a failure in a build step.

**When to use it:**

* When a condition that is critical for the build's success is not met.
* When an external process returns a non-zero exit code (and `ProcessRunOptions.AllowFailedResult` is `false`).
* To explicitly fail a target based on custom validation logic.

**Key Properties:**

* **`ReportData`**: An optional `IReportData` instance that can be attached to the exception. This allows you to provide
  structured information about the failure (e.g., a table of failed tests, a detailed error message) that will be
  included in the final build report.

**How to use it:**
Throw `StepFailedException` within your target's `Executes` block whenever a failure condition is met.

```csharp
// In IMyTargets.cs
[TargetDefinition]
public partial interface IMyTargets : IBuildAccessor
{
    Target ValidateConfiguration => t => t
        .DescribedAs("Validates critical build configuration settings.")
        .Executes(() =>
        {
            var configValue = Environment.GetEnvironmentVariable("CRITICAL_SETTING");
            if (string.IsNullOrEmpty(configValue))
            {
                throw new StepFailedException("CRITICAL_SETTING environment variable is not set. Build cannot proceed.");
            }
            Logger.LogInformation("CRITICAL_SETTING is configured.");
        });

    Target RunTestsAndReportFailures => t => t
        .DescribedAs("Runs tests and reports detailed failures.")
        .Executes(async () =>
        {
            var testResult = await ProcessRunner.RunAsync(new ProcessRunOptions("dotnet", "test --no-build"));

            if (testResult.ExitCode != 0)
            {
                // Create custom report data for the failure
                var failureReport = new TextReportData($"Test run failed with exit code {testResult.ExitCode}.\n\n{testResult.Output}\n{testResult.Error}")
                {
                    Title = "Detailed Test Failure Report"
                };

                throw new StepFailedException("One or more tests failed.", failureReport);
            }
            Logger.LogInformation("All tests passed successfully.");
        });
}
```

When `StepFailedException` is thrown, Atom's `BuildExecutor` catches it, logs the failure, updates the target's status
to `Failed`, and includes any `ReportData` in the final build report. This ensures that failures are clearly
communicated and detailed information is available for debugging.
