namespace DecSm.Atom.Params;

[PublicAPI]
[Flags]
public enum ParamSource
{
    None = 0,
    Cache = 1,
    CommandLineArgs = 2,
    EnvironmentVariables = 4,
    Configuration = 8,
    Vault = 32,
    All = Cache | CommandLineArgs | EnvironmentVariables | Configuration | Vault,
}
