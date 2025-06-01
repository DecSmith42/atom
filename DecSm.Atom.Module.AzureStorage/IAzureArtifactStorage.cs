namespace DecSm.Atom.Module.AzureStorage;

[ConfigureBuilder]
[TargetDefinition]
public partial interface IAzureArtifactStorage : IStoreArtifact, IRetrieveArtifact
{
    [SecretDefinition("azurestorage-artifact-connectionstring", "Connection string for Azure storage container")]
    string AzureArtifactStorageConnectionString => GetParam(() => AzureArtifactStorageConnectionString)!;

    [ParamDefinition("azurestorage-artifact-container", "Azure storage container")]
    string AzureArtifactStorageContainer => GetParam(() => AzureArtifactStorageContainer)!;

    protected static partial void ConfigureBuilder(IHostApplicationBuilder builder) =>
        builder.Services.AddSingleton<IArtifactProvider, AzureBlobArtifactProvider>();
}
