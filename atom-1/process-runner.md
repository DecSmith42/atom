# Process Runner

The `IProcessRunner` interface provides a standardized and robust way to execute external processes (command-line tools, scripts, etc.) within your Atom build. It wraps the standard .NET `System.Diagnostics.Process` with enhanced functionality for logging, error handling, and result capture, making your build scripts more reliable and easier to debug.

### `IProcessRunner` Interface

This interface defines the contract for running external processes, offering both synchronous and asynchronous execution methods.

#### Key Methods:

* **`Run(ProcessRunOptions options)`**: Executes an external process synchronously. The calling thread will block until the process completes.
* **`RunAsync(ProcessRunOptions options, CancellationToken cancellationToken = default)`**: Executes an external process asynchronously. This is generally preferred for long-running operations to keep your build responsive and allow for cancellation.

Both methods return a `ProcessRunResult` object, which contains the process's exit code, captured standard output, and standard error.

#### Error Handling:

By default, if a process returns a non-zero exit code, `IProcessRunner` will throw a `StepFailedException`. You can override this behavior by setting `ProcessRunOptions.AllowFailedResult` to `true`, which allows you to inspect the `ExitCode` yourself and handle failures gracefully.

### `ProcessRunOptions` Record

The `ProcessRunOptions` record allows you to configure various aspects of how an external process is executed. It's an immutable record, ensuring thread safety and predictable behavior.

#### Key Properties:

* **`Name`**: The name or path of the executable to run (e.g., `"dotnet"`, `"git"`, `"npm"`).
* **`Args`**: The command-line arguments to pass to the executable. You can provide this as a single string or an array of strings (which `ProcessRunOptions` will join for you).
* **`WorkingDirectory`**: The directory where the process should be executed. If `null`, the current application's working directory is used.
* **`InvocationLogLevel`**: The `LogLevel` for messages indicating the process invocation (defaults to `Information`).
* **`OutputLogLevel`**: The `LogLevel` for standard output messages from the process (defaults to `Debug`).
* **`ErrorLogLevel`**: The `LogLevel` for standard error messages from the process (defaults to `Warning`).
* **`AllowFailedResult`**: If `true`, a non-zero exit code will not throw an exception (defaults to `false`).
* **`EnvironmentVariables`**: A dictionary of environment variables to set for the process.
* **`TransformOutput`**: An optional function to transform each line of standard output before logging and capturing. Useful for filtering or redacting.
* **`TransformError`**: An optional function to transform each line of standard error.

### `ProcessRunResult` Record

The `ProcessRunResult` record encapsulates the outcome of an external process execution.

#### Key Properties:

* **`RunOptions`**: The `ProcessRunOptions` used to run the process.
* **`ExitCode`**: The integer exit code returned by the process. A value of `0` typically indicates success.
* **`Output`**: The complete standard output stream captured from the process.
* **`Error`**: The complete standard error stream captured from the process.

### How to use `IProcessRunner`

You typically access `IProcessRunner` through the `ProcessRunner` property available via `IBuildAccessor` in your target definitions.

```csharp
// In IMyTargets.cs
[TargetDefinition]
public partial interface IMyTargets : IBuildAccessor
{
    [ParamDefinition("build-config", "The build configuration")]
    string BuildConfig => GetParam(() => BuildConfig, "Release");

    Target BuildProject => t => t
        .DescribedAs("Builds the main project.")
        .Executes(async () =>
        {
            var options = new ProcessRunOptions("dotnet", $"build --configuration {BuildConfig}")
            {
                WorkingDirectory = FileSystem.AtomRootDirectory / "src" / "MyProject",
                OutputLogLevel = LogLevel.Information // Show build output in info logs
            };
            var result = await ProcessRunner.RunAsync(options);

            if (result.ExitCode != 0)
            {
                Logger.LogError("dotnet build failed!");
                // Optionally, throw a StepFailedException if AllowFailedResult was true
                // throw new StepFailedException("Build command failed.");
            }
        });

    Target RunGitStatus => t => t
        .DescribedAs("Checks git status, allowing non-zero exit for dirty working directory.")
        .Executes(async () =>
        {
            var options = new ProcessRunOptions("git", "status --porcelain")
            {
                WorkingDirectory = FileSystem.AtomRootDirectory,
                AllowFailedResult = true // Don't throw if working directory is dirty
            };
            var result = await ProcessRunner.RunAsync(options);

            if (result.ExitCode != 0)
            {
                Logger.LogWarning("Git working directory is dirty. Exit code: {ExitCode}", result.ExitCode);
                Logger.LogWarning("Changes:\n{Output}", result.Output);
            }
            else
            {
                Logger.LogInformation("Git working directory is clean.");
            }
        });
}
```

By using `IProcessRunner`, you can execute external tools reliably, capture their output, and integrate their results into your build logic with consistent logging and error handling.
