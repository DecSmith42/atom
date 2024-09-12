namespace Atom.Targets.Build;

[TargetDefinition]
internal partial interface IPackAzureStorageExtension : IDotnetPackHelper
{
    public const string AzureStorageExtensionProjectName = "DecSm.Atom.Extensions.AzureStorage";

    Target PackAzureStorageExtension =>
        d => d
            .WithDescription("Builds the AzureStorage extension project into a nuget package")
            .ProducesArtifact(AzureStorageExtensionProjectName)
            .Executes(() => DotnetPackProject(new(AzureStorageExtensionProjectName)));
}
