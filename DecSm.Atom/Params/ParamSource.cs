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
    Variables = CommandLineArgs | EnvironmentVariables | Configuration,
    UserSecrets = 16,
    Vault = 32,
    Secrets = UserSecrets | Vault,
    NoCache = Variables | Secrets,
    All = Cache | CommandLineArgs | EnvironmentVariables | Configuration | UserSecrets | Vault,
}
