namespace DecSm.Atom.Artifacts;

/// <summary>
///     Defines a target for storing artifacts within the Atom framework.
/// </summary>
/// <remarks>
///     This interface, when implemented by a build definition, provides the <see cref="StoreArtifact" /> target.
///     This target is responsible for uploading specified artifacts using the configured <see cref="IArtifactProvider" />.
///     It typically consumes build information like <c>ISetupBuildInfo.BuildName</c> and <c>ISetupBuildInfo.BuildId</c> to
///     correctly tag or
///     categorize the artifacts.
///     The artifacts to be stored are specified via the <see cref="IAtomArtifactsParam.AtomArtifacts" /> parameter.
///     Artifacts are expected to be located in the directory specified by
///     <see cref="IAtomFileSystem.AtomPublishDirectory" /> before this
///     target is run.
///     The target is hidden by default as it's primarily for internal use by the Atom framework or custom artifact
///     provider workflows.
/// </remarks>
/// <example>
///     A workflow might use this target to upload build outputs:
///     <code>
/// // In a GitHub Workflow YML file (e.g., .github/workflows/Test_BuildWithCustomArtifacts.yml)
/// // This step would invoke the StoreArtifact target.
/// - name: StoreArtifact
///   run: dotnet run --project _atom/_atom.csproj StoreArtifact --skip --headless
///   env:
///     # Environment variables for artifact provider configuration and artifact names
///     azure-vault-app-secret: ${{ secrets.AZURE_VAULT_APP_SECRET }}
///     azure-vault-address: ${{ vars.AZURE_VAULT_ADDRESS }}
///     azure-vault-tenant-id: ${{ vars.AZURE_VAULT_TENANT_ID }}
///     azure-vault-app-id: ${{ vars.AZURE_VAULT_APP_ID }}
///     atom-artifacts: DecSm.Atom,DecSm.Atom.Tool # Specifies which artifacts to store
///     nuget-dry-run: true # Example of another parameter that might be used by a target
/// </code>
/// </example>
[TargetDefinition]
public partial interface IStoreArtifact : IAtomArtifactsParam, ISetupBuildInfo
{
    private IArtifactProvider ArtifactProvider => GetService<IArtifactProvider>();

    /// <summary>
    ///     Defines the target for storing artifacts.
    /// </summary>
    /// <remarks>
    ///     This target uses the configured <see cref="IArtifactProvider" /> to upload artifacts specified
    ///     by the <see cref="IAtomArtifactsParam.AtomArtifacts" /> parameter. The artifacts are taken from the
    ///     <see cref="IAtomFileSystem.AtomPublishDirectory" />.
    /// </remarks>
    Target StoreArtifact =>
        t => t
            .IsHidden()
            .DescribedAs("Stores artifacts.")
            .ConsumesVariable(nameof(SetupBuildInfo), nameof(BuildName))
            .ConsumesVariable(nameof(SetupBuildInfo), nameof(BuildId))
            .RequiresParam(nameof(AtomArtifacts))
            .RequiresParam(ArtifactProvider.RequiredParams.ToArray())
            .Executes(async cancellationToken =>
            {
                Logger.LogInformation("Using artifact provider: {Provider}",
                    ArtifactProvider.GetType()
                        .Name);

                await ArtifactProvider.StoreArtifacts(AtomArtifacts, cancellationToken: cancellationToken);
            });
}
