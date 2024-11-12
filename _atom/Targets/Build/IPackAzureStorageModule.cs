namespace Atom.Targets.Build;

[TargetDefinition]
internal partial interface IPackAzureStorageModule : IDotnetPackHelper
{
    public const string AzureStorageModuleProjectName = "DecSm.Atom.Module.AzureStorage";

    Target PackAzureStorageModule =>
        d => d
            .WithDescription("Builds the AzureStorage extension project into a nuget package")
            .ProducesArtifact(AzureStorageModuleProjectName)
            .Executes(() => DotnetPackProject(new(AzureStorageModuleProjectName)));
}
