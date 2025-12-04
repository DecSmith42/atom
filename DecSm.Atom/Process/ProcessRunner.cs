namespace DecSm.Atom.Process;

public interface IProcessRunner
{
    /// <summary>
    ///     Executes an external process synchronously with the specified options.
    /// </summary>
    /// <param name="options">
    ///     The configuration options for process execution, including the executable name,
    ///     arguments, working directory, and logging levels.
    /// </param>
    /// <returns>
    ///     A <see cref="ProcessRunResult" /> containing the process exit code, captured output, and error streams.
    /// </returns>
    /// <exception cref="StepFailedException">
    ///     Thrown when the process fails (non-zero exit code) and <see cref="ProcessRunOptions.AllowFailedResult" />
    ///     is <c>false</c>.
    /// </exception>
    /// <remarks>
    ///     This method blocks the calling thread until the process completes. For non-blocking execution,
    ///     use <see cref="ProcessRunner.RunAsync" />.
    ///     The method captures both stdout and stderr streams in real-time, logging each line according to
    ///     the configured log levels in the options. If the process fails and failure is not allowed,
    ///     the captured output and error streams are logged at Information and Warning levels respectively
    ///     before throwing a <see cref="StepFailedException" />.
    /// </remarks>
    ProcessRunResult Run(ProcessRunOptions options);

    /// <summary>
    ///     Executes an external process asynchronously with the specified options and optional cancellation support.
    /// </summary>
    /// <param name="options">
    ///     The configuration options for process execution, including the executable name,
    ///     arguments, working directory, and logging levels.
    /// </param>
    /// <param name="cancellationToken">
    ///     A cancellation token that can be used to cancel the operation.
    ///     Defaults to <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a <see cref="ProcessRunResult" />
    ///     with the process exit code, captured output, and error streams.
    /// </returns>
    /// <exception cref="StepFailedException">
    ///     Thrown when the process fails (non-zero exit code) and <see cref="ProcessRunOptions.AllowFailedResult" />
    ///     is <c>false</c>.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is cancelled via the <paramref name="cancellationToken" />.
    /// </exception>
    /// <remarks>
    ///     This method allows non-blocking execution of external processes, making it suitable for use in
    ///     async build targets and workflows. The method captures both stdout and stderr streams in real-time,
    ///     logging each line according to the configured log levels in the options.
    ///     If the process fails and failure is not allowed, the captured output and error streams are logged
    ///     at Information and Warning levels respectively before throwing a <see cref="StepFailedException" />.
    ///     The cancellation token allows for graceful termination of long-running processes when needed.
    /// </remarks>
    /// <example>
    ///     <code>
    /// var options = new ProcessRunOptions("git", ["clone", "https://github.com/user/repo.git"])
    /// {
    ///     WorkingDirectory = "./temp",
    ///     AllowFailedResult = true
    /// };
    /// using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
    /// var result = await processRunner.RunAsync(options, cts.Token);
    /// if (result.ExitCode != 0)
    /// {
    ///     logger.LogWarning("Git clone failed: {Error}", result.Error);
    /// }
    /// </code>
    /// </example>
    Task<ProcessRunResult> RunAsync(ProcessRunOptions options, CancellationToken cancellationToken = default);
}

/// <summary>
///     Provides a standardized service for executing external processes with comprehensive logging,
///     error handling, and result capture within the Atom build system.
/// </summary>
/// <remarks>
///     This class serves as a wrapper around <see cref="System.Diagnostics.Process" /> with enhanced
///     functionality specifically designed for build automation scenarios. It provides:
///     <list type="bullet">
///         <item>
///             <description>Configurable logging at multiple levels (invocation, output, error)</description>
///         </item>
///         <item>
///             <description>Real-time stream capture and logging</description>
///         </item>
///         <item>
///             <description>Flexible error handling with optional failure tolerance</description>
///         </item>
///         <item>
///             <description>Both synchronous and asynchronous execution methods</description>
///         </item>
///         <item>
///             <description>Working directory configuration</description>
///         </item>
///         <item>
///             <description>Cancellation support for long-running processes</description>
///         </item>
///     </list>
///     The service is typically injected into build targets and helper services to execute
///     external tools such as the .NET CLI, Git, package managers, and other command-line utilities.
/// </remarks>
/// <param name="logger">The logger instance for capturing process execution information.</param>
/// <example>
///     <code>
/// var options = new ProcessRunOptions("dotnet", "build")
/// {
///     WorkingDirectory = "./src",
///     InvocationLogLevel = LogLevel.Information,
///     OutputLogLevel = LogLevel.Debug,
///     ErrorLogLevel = LogLevel.Warning,
///     AllowFailedResult = false
/// };
/// var result = await processRunner.RunAsync(options);
/// if (result.ExitCode == 0)
/// {
///     // Process succeeded
///     Console.WriteLine(result.Output);
/// }
/// </code>
/// </example>
[PublicAPI]
public sealed class ProcessRunner(ILogger<ProcessRunner> logger) : IProcessRunner
{
    /// <summary>
    ///     Executes an external process synchronously with the specified options.
    /// </summary>
    /// <param name="options">
    ///     The configuration options for process execution, including the executable name,
    ///     arguments, working directory, and logging levels.
    /// </param>
    /// <returns>
    ///     A <see cref="ProcessRunResult" /> containing the process exit code, captured output, and error streams.
    /// </returns>
    /// <exception cref="StepFailedException">
    ///     Thrown when the process fails (non-zero exit code) and <see cref="ProcessRunOptions.AllowFailedResult" />
    ///     is <c>false</c>.
    /// </exception>
    /// <remarks>
    ///     This method blocks the calling thread until the process completes. For non-blocking execution,
    ///     use <see cref="RunAsync(ProcessRunOptions, CancellationToken)" />.
    ///     The method captures both stdout and stderr streams in real-time, logging each line according to
    ///     the configured log levels in the options. If the process fails and failure is not allowed,
    ///     the captured output and error streams are logged at Information and Warning levels respectively
    ///     before throwing a <see cref="StepFailedException" />.
    /// </remarks>
    public ProcessRunResult Run(ProcessRunOptions options)
    {
        switch (options)
        {
            case { WorkingDirectory.Length: > 0, EnvironmentVariables.Count: > 0 }:
                logger.Log(options.InvocationLogLevel,
                    "Run: {Name} {Args} in {WorkingDirectory} with env {EnvironmentVariables}",
                    options.Name,
                    options.Args,
                    options.WorkingDirectory,
                    string.Join(", ", options.EnvironmentVariables.Select(kv => $"{kv.Key}={kv.Value}")));

                break;
            case { WorkingDirectory.Length: > 0 }:
                logger.Log(options.InvocationLogLevel,
                    "Run: {Name} {Args} in {WorkingDirectory}",
                    options.Name,
                    options.Args,
                    options.WorkingDirectory);

                break;
            case { EnvironmentVariables.Count: > 0 }:
                logger.Log(options.InvocationLogLevel,
                    "Run: {Name} {Args} with env {EnvironmentVariables}",
                    options.Name,
                    options.Args,
                    string.Join(", ", options.EnvironmentVariables.Select(kv => $"{kv.Key}={kv.Value}")));

                break;
            default:
                logger.Log(options.InvocationLogLevel, "Run: {Name} {Args}", options.Name, options.Args);

                break;
        }

        using var process = new System.Diagnostics.Process();

        process.StartInfo = new()
        {
            FileName = options.Name,
            Arguments = options.Args,
            WorkingDirectory = options.WorkingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        foreach (var environmentVariable in options.EnvironmentVariables)
            process.StartInfo.Environment[environmentVariable.Key] = environmentVariable.Value;

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += OnProcessOnOutputDataReceived;
        process.ErrorDataReceived += OnProcessOnErrorDataReceived;

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }
        finally
        {
            process.OutputDataReceived -= OnProcessOnOutputDataReceived;
            process.ErrorDataReceived -= OnProcessOnErrorDataReceived;
        }

        var output = outputBuilder.ToString();
        var error = errorBuilder.ToString();

        var result = new ProcessRunResult(options, process.ExitCode, output, error);

        if (result.ExitCode is 0)
            return result;

        if (options.OutputLogLevel < LogLevel.Information)
            logger.LogInformation("{Output}", output);

        if (options.ErrorLogLevel < LogLevel.Information)
            logger.LogWarning("{Error}", error);

        if (options.AllowFailedResult)
        {
            logger.Log(options.InvocationLogLevel, "Process finished with exit code {ExitCode}", result.ExitCode);

            return result;
        }

        throw new StepFailedException($"Process {options.Name} {options.Args} failed with exit code {result.ExitCode}");

        void OnProcessOnErrorDataReceived(object _, DataReceivedEventArgs e)
        {
            if (e.Data is null)
                return;

            var text = options.TransformError is not null
                ? options.TransformError(e.Data)
                : e.Data;

            if (text is null)
                return;

            errorBuilder.AppendLine(text);
            logger.Log(options.ErrorLogLevel, "{Error}", text);
        }

        void OnProcessOnOutputDataReceived(object _, DataReceivedEventArgs e)
        {
            if (e.Data is null)
                return;

            var text = options.TransformOutput is not null
                ? options.TransformOutput(e.Data)
                : e.Data;

            if (text is null)
                return;

            outputBuilder.AppendLine(text);
            logger.Log(options.OutputLogLevel, "{Output}", text);
        }
    }

    /// <summary>
    ///     Executes an external process asynchronously with the specified options and optional cancellation support.
    /// </summary>
    /// <param name="options">
    ///     The configuration options for process execution, including the executable name,
    ///     arguments, working directory, and logging levels.
    /// </param>
    /// <param name="cancellationToken">
    ///     A cancellation token that can be used to cancel the operation.
    ///     Defaults to <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a <see cref="ProcessRunResult" />
    ///     with the process exit code, captured output, and error streams.
    /// </returns>
    /// <exception cref="StepFailedException">
    ///     Thrown when the process fails (non-zero exit code) and <see cref="ProcessRunOptions.AllowFailedResult" />
    ///     is <c>false</c>.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is cancelled via the <paramref name="cancellationToken" />.
    /// </exception>
    /// <remarks>
    ///     This method allows non-blocking execution of external processes, making it suitable for use in
    ///     async build targets and workflows. The method captures both stdout and stderr streams in real-time,
    ///     logging each line according to the configured log levels in the options.
    ///     If the process fails and failure is not allowed, the captured output and error streams are logged
    ///     at Information and Warning levels respectively before throwing a <see cref="StepFailedException" />.
    ///     The cancellation token allows for graceful termination of long-running processes when needed.
    /// </remarks>
    /// <example>
    ///     <code>
    /// var options = new ProcessRunOptions("git", ["clone", "https://github.com/user/repo.git"])
    /// {
    ///     WorkingDirectory = "./temp",
    ///     AllowFailedResult = true
    /// };
    /// using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
    /// var result = await processRunner.RunAsync(options, cts.Token);
    /// if (result.ExitCode != 0)
    /// {
    ///     logger.LogWarning("Git clone failed: {Error}", result.Error);
    /// }
    /// </code>
    /// </example>
    public async Task<ProcessRunResult> RunAsync(
        ProcessRunOptions options,
        CancellationToken cancellationToken = default)
    {
        switch (options)
        {
            case { WorkingDirectory.Length: > 0, EnvironmentVariables.Count: > 0 }:
                logger.Log(options.InvocationLogLevel,
                    "Run: {Name} {Args} in {WorkingDirectory} with env {EnvironmentVariables}",
                    options.Name,
                    options.Args,
                    options.WorkingDirectory,
                    string.Join(", ", options.EnvironmentVariables.Select(kv => $"{kv.Key}={kv.Value}")));

                break;
            case { WorkingDirectory.Length: > 0 }:
                logger.Log(options.InvocationLogLevel,
                    "Run: {Name} {Args} in {WorkingDirectory}",
                    options.Name,
                    options.Args,
                    options.WorkingDirectory);

                break;
            case { EnvironmentVariables.Count: > 0 }:
                logger.Log(options.InvocationLogLevel,
                    "Run: {Name} {Args} with env {EnvironmentVariables}",
                    options.Name,
                    options.Args,
                    string.Join(", ", options.EnvironmentVariables.Select(kv => $"{kv.Key}={kv.Value}")));

                break;
            default:
                logger.Log(options.InvocationLogLevel, "Run: {Name} {Args}", options.Name, options.Args);

                break;
        }

        using var process = new System.Diagnostics.Process();

        process.StartInfo = new()
        {
            FileName = options.Name,
            Arguments = options.Args,
            WorkingDirectory = options.WorkingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        foreach (var environmentVariable in options.EnvironmentVariables)
            process.StartInfo.Environment[environmentVariable.Key] = environmentVariable.Value;

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += OnProcessOnOutputDataReceived;
        process.ErrorDataReceived += OnProcessOnErrorDataReceived;

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync(cancellationToken);
        }
        finally
        {
            process.OutputDataReceived -= OnProcessOnOutputDataReceived;
            process.ErrorDataReceived -= OnProcessOnErrorDataReceived;
        }

        var output = outputBuilder.ToString();
        var error = errorBuilder.ToString();

        var result = new ProcessRunResult(options, process.ExitCode, output, error);

        if (result.ExitCode is 0)
            return result;

        if (options.OutputLogLevel < LogLevel.Information)
            logger.LogInformation("{Output}", output);

        if (options.ErrorLogLevel < LogLevel.Information)
            logger.LogWarning("{Error}", error);

        if (options.AllowFailedResult)
        {
            logger.Log(options.InvocationLogLevel, "Process finished with exit code {ExitCode}", result.ExitCode);

            return result;
        }

        throw new StepFailedException($"Process {options.Name} {options.Args} failed with exit code {result.ExitCode}");

        void OnProcessOnErrorDataReceived(object _, DataReceivedEventArgs e)
        {
            if (e.Data is null)
                return;

            var text = options.TransformError is not null
                ? options.TransformError(e.Data)
                : e.Data;

            if (text is null)
                return;

            errorBuilder.AppendLine(text);
            logger.Log(options.ErrorLogLevel, "{Error}", text);
        }

        void OnProcessOnOutputDataReceived(object _, DataReceivedEventArgs e)
        {
            if (e.Data is null)
                return;

            var text = options.TransformOutput is not null
                ? options.TransformOutput(e.Data)
                : e.Data;

            if (text is null)
                return;

            outputBuilder.AppendLine(text);
            logger.Log(options.OutputLogLevel, "{Output}", text);
        }
    }
}
