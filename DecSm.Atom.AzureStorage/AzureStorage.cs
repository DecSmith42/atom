namespace DecSm.Atom.AzureStorage;

public static class AzureStorage
{
    public static IAtomConfiguration AddAzureArtifacts(this IAtomConfiguration configuration)
    {
        configuration.Builder.Services.AddSingleton<IArtifactProvider, AzureBlobArtifactProvider>();

        return configuration;
    }
}