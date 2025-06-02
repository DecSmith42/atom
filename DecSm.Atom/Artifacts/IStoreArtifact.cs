namespace DecSm.Atom.Artifacts;

/// <summary>
///     Includes the target definition required for storing artifacts.
///     Must be included in the build to use custom providers for storing artifacts.
/// </summary>
/// <remarks>
///     When creating a custom provider for storing artifacts, add this interface to the build.
/// </remarks>
[TargetDefinition]
public partial interface IStoreArtifact : IAtomArtifactsParam, ISetupBuildInfo
{
    private IArtifactProvider ArtifactProvider => GetService<IArtifactProvider>();

    Target StoreArtifact =>
        d => d
            .IsHidden()
            .DescribedAs("Stores artifacts.")
            .ConsumesVariable(nameof(SetupBuildInfo), nameof(BuildName))
            .ConsumesVariable(nameof(SetupBuildInfo), nameof(BuildId))
            .RequiresParam(nameof(AtomArtifacts))
            .RequiresParam(ArtifactProvider.RequiredParams)
            .Executes(async () =>
            {
                Logger.LogInformation("Using artifact provider: {Provider}",
                    ArtifactProvider.GetType()
                        .Name);

                await ArtifactProvider.StoreArtifacts(AtomArtifacts);
            });
}
