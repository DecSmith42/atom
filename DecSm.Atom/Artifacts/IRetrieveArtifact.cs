namespace DecSm.Atom.Artifacts;

/// <summary>
///     Includes the target definition required for retrieving artifacts.
///     Must be included in the build to use custom providers for retrieving artifacts.
/// </summary>
/// <remarks>
///     When creating a custom provider for storing artifacts, add this interface to the build.
/// </remarks>
[TargetDefinition]
public partial interface IRetrieveArtifact : IArtifactHelper
{
    Target RetrieveArtifact =>
        targetDefinition =>
        {
            var artifactProvider = GetService<IArtifactProvider>();

            targetDefinition.IsHidden();
            targetDefinition.WithDescription("Retrieves artifacts.");

            targetDefinition.ConsumesVariable(nameof(ISetupBuildInfo.SetupBuildInfo), nameof(ISetupBuildInfo.AtomBuildId));

            targetDefinition.RequiredParams.Add(nameof(AtomArtifacts));
            targetDefinition.RequiredParams.AddRange(artifactProvider.RequiredParams);

            targetDefinition.Executes(async () =>
            {
                Logger.LogInformation("Using artifact provider: {Provider}",
                    artifactProvider.GetType()
                        .Name);

                await artifactProvider.RetrieveArtifacts(AtomArtifacts);
            });

            return targetDefinition;
        };
}
