namespace DecSm.Atom.Artifacts;

/// <summary>
///     Includes the target definition required for storing artifacts.
///     Must be included in the build to use custom providers for storing artifacts.
/// </summary>
/// <remarks>
///     When creating a custom provider for storing artifacts, add this interface to the build.
/// </remarks>
[TargetDefinition]
public partial interface IStoreArtifact : IArtifactHelper
{
    Target StoreArtifact =>
        targetDefinition =>
        {
            var artifactProvider = GetService<IArtifactProvider>();

            targetDefinition.IsHidden();
            targetDefinition.WithDescription("Stores artifacts.");

            targetDefinition.ConsumedVariables.Add(new(nameof(ISetupBuildInfo.SetupBuildInfo), nameof(BuildId)));

            targetDefinition.RequiredParams.Add(nameof(AtomArtifacts));
            targetDefinition.RequiredParams.AddRange(artifactProvider.RequiredParams);

            targetDefinition.Executes(async () =>
            {
                Logger.LogInformation("Using artifact provider: {Provider}",
                    artifactProvider.GetType()
                        .Name);

                await artifactProvider.StoreArtifacts(AtomArtifacts);
            });

            return targetDefinition;
        };
}
