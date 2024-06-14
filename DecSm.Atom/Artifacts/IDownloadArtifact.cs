namespace DecSm.Atom.Artifacts;

[TargetDefinition]
public interface IDownloadArtifact : IArtifactHelper
{
    Target DownloadArtifact =>
        targetDefinition =>
        {
            var artifactProvider = Services.GetRequiredService<IArtifactProvider>();

            targetDefinition.ConsumedVariables.Add(new(nameof(ISetup.Setup), nameof(ISetup.AtomBuildId)));

            targetDefinition.RequiredParams.Add(nameof(AtomArtifacts));
            targetDefinition.RequiredParams.AddRange(artifactProvider.RequiredParams);

            targetDefinition.Executes(() => artifactProvider.DownloadArtifacts(AtomArtifactNames));

            return targetDefinition;
        };
}