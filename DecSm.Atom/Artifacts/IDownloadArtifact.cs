namespace DecSm.Atom.Artifacts;

[TargetDefinition]
public partial interface IDownloadArtifact : IArtifactHelper
{
    Target DownloadArtifact =>
        targetDefinition =>
        {
            var artifactProvider = GetService<IArtifactProvider>();

            targetDefinition.ConsumesVariable(nameof(ISetup.Setup), nameof(ISetup.AtomBuildId));

            targetDefinition.RequiredParams.Add(nameof(AtomArtifacts));
            targetDefinition.RequiredParams.AddRange(artifactProvider.RequiredParams);

            targetDefinition.Executes(() => artifactProvider.DownloadArtifacts(AtomArtifactNames));

            return targetDefinition;
        };
}