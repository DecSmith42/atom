namespace DecSm.Atom.Artifacts;

[TargetDefinition]
public partial interface IDownloadArtifact : IArtifactHelper
{
    Target DownloadArtifact =>
        targetDefinition =>
        {
            var artifactProvider = GetService<IArtifactProvider>();
            var buildIdProvider = GetService<IBuildIdProvider>();

            targetDefinition.IsHidden();

            targetDefinition.ConsumesVariable(nameof(ISetup.Setup), nameof(ISetup.AtomBuildId));

            targetDefinition.RequiredParams.Add(nameof(AtomArtifacts));
            targetDefinition.RequiredParams.AddRange(artifactProvider.RequiredParams);

            targetDefinition.Executes(async () =>
            {
                Logger.LogInformation("Using artifact provider: {Provider}",
                    artifactProvider.GetType()
                        .Name);

                await artifactProvider.DownloadArtifacts(AtomArtifacts);
            });

            return targetDefinition;
        };
}
