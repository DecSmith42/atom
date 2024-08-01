namespace DecSm.Atom.Artifacts;

[TargetDefinition]
public interface IUploadArtifact : IArtifactHelper
{
    Target UploadArtifact =>
        targetDefinition =>
        {
            var artifactProvider = GetService<IArtifactProvider>();

            targetDefinition.ConsumedVariables.Add(new(nameof(ISetup.Setup), nameof(ISetup.AtomBuildId)));

            targetDefinition.RequiredParams.Add(nameof(AtomArtifacts));
            targetDefinition.RequiredParams.AddRange(artifactProvider.RequiredParams);

            targetDefinition.Executes(() => artifactProvider.UploadArtifacts(AtomArtifactNames));

            return targetDefinition;
        };
}