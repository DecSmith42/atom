namespace DecSm.Atom.Process;

[PublicAPI]
public sealed record ProcessRunResult(ProcessRunOptions RunOptions, int ExitCode, string Output, string Error);
