namespace DecSm.Atom.Artifacts;

[TargetDefinition]
public partial interface IUploadArtifact : IArtifactHelper
{
    Target UploadArtifact =>
        targetDefinition =>
        {
            var artifactProvider = GetService<IArtifactProvider>();

            targetDefinition.IsHidden();

            targetDefinition.ConsumedVariables.Add(new(nameof(ISetup.Setup), nameof(ISetup.AtomBuildId)));

            targetDefinition.RequiredParams.Add(nameof(AtomArtifacts));
            targetDefinition.RequiredParams.AddRange(artifactProvider.RequiredParams);

            targetDefinition.Executes(async () =>
            {
                Logger.LogInformation("Using artifact provider: {Provider}",
                    artifactProvider.GetType()
                        .Name);

                await artifactProvider.UploadArtifacts(AtomArtifacts);
            });

            return targetDefinition;
        };
}
