namespace DecSm.Atom.Artifacts;

/// <summary>
///     Defines a target for retrieving artifacts within the Atom framework.
/// </summary>
/// <remarks>
///     This interface, when implemented by a build definition, provides the <see cref="RetrieveArtifact" /> target.
///     This target is responsible for downloading specified artifacts using the configured <see cref="IArtifactProvider" />.
///     It typically consumes build information like <c>ISetupBuildInfo.BuildName</c> and <c>ISetupBuildInfo.BuildId</c> to identify the
///     correct artifacts.
///     The artifacts to be retrieved are specified via the <see cref="IAtomArtifactsParam.AtomArtifacts" /> parameter.
///     The target is hidden by default as it's primarily for internal use by the Atom framework or custom artifact provider workflows.
/// </remarks>
/// <example>
///     A workflow might use this target to download previously stored packages:
///     <code>
/// // In a GitHub Workflow YML file (e.g., .github/workflows/Test_BuildWithCustomArtifacts.yml)
/// // This step would invoke the RetrieveArtifact target.
/// - name: RetrieveArtifact
///   run: dotnet run --project _atom/_atom.csproj RetrieveArtifact --skip --headless
///   env:
///     # Environment variables for artifact provider configuration and artifact names
///     azure-vault-app-secret: ${{ secrets.AZURE_VAULT_APP_SECRET }}
///     azure-vault-address: ${{ vars.AZURE_VAULT_ADDRESS }}
///     azure-vault-tenant-id: ${{ vars.AZURE_VAULT_TENANT_ID }}
///     azure-vault-app-id: ${{ vars.AZURE_VAULT_APP_ID }}
///     atom-artifacts: DecSm.Atom,DecSm.Atom.Tool # Specifies which artifacts to retrieve
/// </code>
/// </example>
[TargetDefinition]
public partial interface IRetrieveArtifact : IAtomArtifactsParam, ISetupBuildInfo
{
    private IArtifactProvider ArtifactProvider => GetService<IArtifactProvider>();

    /// <summary>
    ///     Defines the target for retrieving artifacts.
    /// </summary>
    /// <remarks>
    ///     This target uses the configured <see cref="IArtifactProvider" /> to download artifacts specified
    ///     by the <see cref="IAtomArtifactsParam.AtomArtifacts" /> parameter.
    /// </remarks>
    Target RetrieveArtifact =>
        d => d
            .IsHidden()
            .DescribedAs("Retrieves artifacts.")
            .ConsumesVariable(nameof(SetupBuildInfo), nameof(BuildName))
            .ConsumesVariable(nameof(SetupBuildInfo), nameof(BuildId))
            .RequiresParam(nameof(AtomArtifacts))
            .RequiresParam(ArtifactProvider.RequiredParams)
            .Executes(async () =>
            {
                Logger.LogInformation("Using artifact provider: {Provider}",
                    ArtifactProvider.GetType()
                        .Name);

                await ArtifactProvider.RetrieveArtifacts(AtomArtifacts);
            });
}
