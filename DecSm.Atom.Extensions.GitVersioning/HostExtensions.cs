namespace DecSm.Atom.Extensions.GitVersioning;

public static class HostExtensions
{
    public static IAtomConfiguration AddGitVersioning(this IAtomConfiguration configuration)
    {
        configuration.Builder.Services.AddSingleton<IBuildIdProvider, GitVersioningBuildIdProvider>();
        configuration.Builder.Services.AddSingleton<IBuildVersionProvider, GitVersioningBuildVersionProvider>();

        return configuration;
    }
}