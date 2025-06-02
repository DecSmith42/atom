namespace DecSm.Atom.Process;

/// <summary>
///     Represents the result of an external process execution, encapsulating all information about
///     the process run including its configuration, exit status, and captured output streams.
/// </summary>
/// <param name="RunOptions">The options used to run the process.</param>
/// <param name="ExitCode">The exit code returned by the process.</param>
/// <param name="Output">The standard output captured from the process.</param>
/// <param name="Error">The standard error output captured from the process.</param>
[PublicAPI]
public sealed record ProcessRunResult(ProcessRunOptions RunOptions, int ExitCode, string Output, string Error);
