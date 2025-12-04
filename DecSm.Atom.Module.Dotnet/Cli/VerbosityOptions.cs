namespace DecSm.Atom.Module.Dotnet.Cli;

[PublicAPI]
public enum VerbosityOptions
{
    Quiet,
    Minimal,
    Normal,
    Detailed,
    Diagnostic,
}

[PublicAPI]
public static class VerbosityOptionsExtensions
{
    extension(VerbosityOptions verbosityOptions)
    {
        [PublicAPI]
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
