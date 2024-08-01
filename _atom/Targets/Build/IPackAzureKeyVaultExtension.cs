namespace Atom.Targets.Build;

[TargetDefinition]
internal partial interface IPackAzureKeyVaultExtension : IDotnetPackHelper
{
    public const string AzureKeyVaultExtensionProjectName = "DecSm.Atom.Extensions.AzureKeyVault";

    Target PackAzureKeyVaultExtension =>
        d => d
            .WithDescription("Builds the AzureKeyVault extension project into a nuget package")
            .ProducesArtifact(AzureKeyVaultExtensionProjectName)
            .Executes(() => DotnetPackProject(new(AzureKeyVaultExtensionProjectName)));
}