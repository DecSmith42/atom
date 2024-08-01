namespace DecSm.Atom.Extensions.GitVersion;

public static class HostExtensions
{
    public static IAtomConfiguration AddGitVersion(this IAtomConfiguration configuration)
    {
        configuration.Builder.Services.AddSingleton<IBuildIdProvider, GitVersionBuildIdProvider>();
        configuration.Builder.Services.AddSingleton<IBuildVersionProvider, GitVersionBuildVersionProvider>();

        return configuration;
    }
}