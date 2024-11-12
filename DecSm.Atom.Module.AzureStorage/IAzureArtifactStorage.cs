namespace DecSm.Atom.Module.AzureStorage;

[TargetDefinition]
public partial interface IAzureArtifactStorage : IUploadArtifact, IDownloadArtifact
{
    [SecretDefinition("azurestorage-artifact-connectionstring", "Connection string for Azure storage container")]
    string AzureArtifactStorageConnectionString => GetParam(() => AzureArtifactStorageConnectionString)!;

    [ParamDefinition("azurestorage-artifact-container", "Azure storage container")]
    string AzureArtifactStorageContainer => GetParam(() => AzureArtifactStorageContainer)!;

    static void IBuildDefinition.Register(IServiceCollection services) =>
        services.AddSingleton<IArtifactProvider, AzureBlobArtifactProvider>();
}
