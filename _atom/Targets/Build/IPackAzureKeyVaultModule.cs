namespace Atom.Targets.Build;

[TargetDefinition]
internal partial interface IPackAzureKeyVaultModule : IDotnetPackHelper
{
    public const string AzureKeyVaultModuleProjectName = "DecSm.Atom.Module.AzureKeyVault";

    Target PackAzureKeyVaultModule =>
        d => d
            .WithDescription("Builds the AzureKeyVault extension project into a nuget package")
            .ProducesArtifact(AzureKeyVaultModuleProjectName)
            .Executes(() => DotnetPackProject(new(AzureKeyVaultModuleProjectName)));
}
