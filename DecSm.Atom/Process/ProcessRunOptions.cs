namespace DecSm.Atom.Process;

/// <summary>
///     Configuration options for executing external processes through the <see cref="ProcessRunner" /> service.
/// </summary>
/// <remarks>
///     This record provides a comprehensive set of configuration options for controlling how external processes
///     are executed, including logging behavior, error handling, and execution context. It supports both
///     string-based and array-based argument specification for flexibility in different scenarios.
///     <para>
///         The class is designed as an immutable record with init-only properties, ensuring thread safety
///         and preventing accidental modification during process execution.
///     </para>
///     <para>
///         Default values are configured for common build automation scenarios:
///         <list type="bullet">
///             <item>
///                 <description>Process invocation logged at Information level</description>
///             </item>
///             <item>
///                 <description>Standard output logged at Debug level</description>
///             </item>
///             <item>
///                 <description>Error output logged at Warning level</description>
///             </item>
///             <item>
///                 <description>Failed processes throw exceptions by default</description>
///             </item>
///         </list>
///     </para>
/// </remarks>
/// <param name="Name">The name or path of the executable to run (e.g., "dotnet", "git", "npm").</param>
/// <param name="Args">The command-line arguments to pass to the executable as a single string.</param>
/// <example>
///     <code>
/// // Basic usage with string arguments
/// var options1 = new ProcessRunOptions("dotnet", "build --configuration Release");
/// // Using array arguments constructor
/// var options2 = new ProcessRunOptions("git", ["status", "--porcelain"]);
/// // Advanced configuration
/// var options3 = new ProcessRunOptions("npm", "install")
/// {
///     WorkingDirectory = "./frontend",
///     InvocationLogLevel = LogLevel.Debug,
///     OutputLogLevel = LogLevel.Information,
///     ErrorLogLevel = LogLevel.Error,
///     AllowFailedResult = true
/// };
/// </code>
/// </example>
[PublicAPI]
public sealed record ProcessRunOptions(string Name, string Args)
{
    /// <summary>
    ///     Initializes a new instance of <see cref="ProcessRunOptions" /> with the specified executable name and argument array.
    /// </summary>
    /// <param name="Name">The name or path of the executable to run.</param>
    /// <param name="Args">An array of command-line arguments that will be joined with spaces.</param>
    /// <remarks>
    ///     This constructor provides a convenient way to specify arguments as an array, which is automatically
    ///     joined into a single string using space separators. This is particularly useful when working with
    ///     programmatically constructed argument lists or when arguments contain special characters.
    /// </remarks>
    /// <example>
    ///     <code>
    /// var options = new ProcessRunOptions("docker", ["run", "-it", "--rm", "ubuntu:latest", "/bin/bash"]);
    /// // Equivalent to: new ProcessRunOptions("docker", "run -it --rm ubuntu:latest /bin/bash")
    /// </code>
    /// </example>
    public ProcessRunOptions(string Name, string[] Args) : this(Name, string.Join(" ", Args)) { }

    /// <summary>
    ///     Gets or sets the working directory for the process execution.
    /// </summary>
    /// <value>
    ///     The path to the directory where the process should be executed, or <c>null</c> to use
    ///     the current application's working directory. Relative paths are resolved relative to
    ///     the current working directory.
    /// </value>
    /// <remarks>
    ///     When specified, this directory becomes the current working directory for the child process,
    ///     affecting relative path resolution and file operations within the executed process.
    ///     The directory must exist when the process is started, or the execution will fail.
    /// </remarks>
    /// <example>
    ///     <code>
    /// var options = new ProcessRunOptions("dotnet", "restore")
    /// {
    ///     WorkingDirectory = "./src/MyProject"
    /// };
    /// </code>
    /// </example>
    public string? WorkingDirectory { get; init; }

    /// <summary>
    ///     Gets or sets the log level for process invocation messages.
    /// </summary>
    /// <value>
    ///     The <see cref="LogLevel" /> used when logging the process start information,
    ///     including the executable name, arguments, and working directory. Defaults to <see cref="LogLevel.Information" />.
    /// </value>
    /// <remarks>
    ///     This property controls the visibility of process invocation logging, which includes the command
    ///     being executed and its working directory. Setting this to a higher level (e.g., <see cref="LogLevel.Debug" />)
    ///     reduces log verbosity in production scenarios, while lower levels (e.g., <see cref="LogLevel.Trace" />)
    ///     provide more detailed execution tracking.
    /// </remarks>
    /// <example>
    ///     <code>
    /// var options = new ProcessRunOptions("sensitive-tool", "process-data")
    /// {
    ///     InvocationLogLevel = LogLevel.Debug // Less visible in production logs
    /// };
    /// </code>
    /// </example>
    public LogLevel InvocationLogLevel { get; init; } = LogLevel.Information;

    /// <summary>
    ///     Gets or sets the log level for standard output messages from the executed process.
    /// </summary>
    /// <value>
    ///     The <see cref="LogLevel" /> used when logging each line of standard output from the process.
    ///     Defaults to <see cref="LogLevel.Debug" />.
    /// </value>
    /// <remarks>
    ///     This property controls how standard output from the child process is logged. Debug level is
    ///     the default as it provides detailed execution information without overwhelming production logs.
    ///     For processes that produce important output that should always be visible, consider using
    ///     <see cref="LogLevel.Information" /> or higher.
    /// </remarks>
    /// <example>
    ///     <code>
    /// var options = new ProcessRunOptions("git", "log --oneline -10")
    /// {
    ///     OutputLogLevel = LogLevel.Information // Make git log visible in normal logs
    /// };
    /// </code>
    /// </example>
    public LogLevel OutputLogLevel { get; init; } = LogLevel.Debug;

    /// <summary>
    ///     Gets or sets the log level for standard error messages from the executed process.
    /// </summary>
    /// <value>
    ///     The <see cref="LogLevel" /> used when logging each line of standard error from the process.
    ///     Defaults to <see cref="LogLevel.Warning" />.
    /// </value>
    /// <remarks>
    ///     This property controls how error output from the child process is logged. Warning level is
    ///     the default as error output typically indicates issues that should be visible in production logs.
    ///     For processes that use stderr for non-error information (like progress updates), consider
    ///     using <see cref="LogLevel.Debug" /> or <see cref="LogLevel.Information" />.
    /// </remarks>
    /// <example>
    ///     <code>
    /// var options = new ProcessRunOptions("wget", "https://example.com/file.zip")
    /// {
    ///     ErrorLogLevel = LogLevel.Debug // wget uses stderr for progress, not errors
    /// };
    /// </code>
    /// </example>
    public LogLevel ErrorLogLevel { get; init; } = LogLevel.Warning;

    /// <summary>
    ///     Gets or sets a value indicating whether the process execution should tolerate non-zero exit codes.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the process execution should return normally even when the exit code is non-zero;
    ///     <c>false</c> if a <see cref="StepFailedException" /> should be thrown for non-zero exit codes.
    ///     Defaults to <c>false</c>.
    /// </value>
    /// <remarks>
    ///     <para>
    ///         When set to <c>false</c> (the default), any non-zero exit code from the executed process
    ///         will cause the <see cref="ProcessRunner" /> to throw a <see cref="StepFailedException" />,
    ///         which is appropriate for build automation scenarios where process failures should halt execution.
    ///     </para>
    ///     <para>
    ///         When set to <c>true</c>, the process execution completes normally regardless of the exit code,
    ///         allowing the caller to inspect the <see cref="ProcessRunResult.ExitCode" /> and handle failures
    ///         appropriately. This is useful for optional operations or when the exit code carries semantic
    ///         meaning beyond success/failure.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    /// // Strict mode - throw on any failure (default)
    /// var strictOptions = new ProcessRunOptions("dotnet", "test");
    /// // Tolerant mode - handle failures manually
    /// var tolerantOptions = new ProcessRunOptions("git", "status --porcelain")
    /// {
    ///     AllowFailedResult = true
    /// };
    /// var result = await runner.RunAsync(tolerantOptions);
    /// if (result.ExitCode != 0)
    /// {
    ///     // Handle the failure case appropriately
    ///     logger.LogWarning("Git status check failed: {Error}", result.Error);
    /// }
    /// </code>
    /// </example>
    public bool AllowFailedResult { get; init; }
}
