namespace DecSm.Atom.Module.Dotnet.Cli;

public enum VerbosityOptions
{
    Quiet,
    Minimal,
    Normal,
    Detailed,
    Diagnostic,
}

public static class VerbosityOptionsExtensions
{
    extension(VerbosityOptions verbosityOptions)
    {
        public string ToString() =>
            verbosityOptions switch
            {
                VerbosityOptions.Quiet => "quiet",
                VerbosityOptions.Minimal => "minimal",
                VerbosityOptions.Normal => "normal",
                VerbosityOptions.Detailed => "detailed",
                VerbosityOptions.Diagnostic => "diagnostic",
                _ => throw new ArgumentOutOfRangeException(nameof(verbosityOptions), verbosityOptions, null),
            };
    }
}
