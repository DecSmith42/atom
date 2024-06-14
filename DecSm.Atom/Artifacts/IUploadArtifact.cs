namespace DecSm.Atom.Artifacts;

[TargetDefinition]
public interface IUploadArtifact : IBuildDefinition
{
    [ParamDefinition("upload-artifact-name", "The name of the artifact to upload.")]
    string UploadArtifactName => GetParam(() => UploadArtifactName)!;

    Target UploadArtifact =>
        d =>
        {
            var artifactProvider = Services.GetRequiredService<IArtifactProvider>();

            d.ConsumedVariables.Add(new(nameof(ISetup.Setup), nameof(ISetup.AtomBuildId)));

            d.RequiredParams.Add(nameof(UploadArtifactName));
            d.RequiredParams.AddRange(artifactProvider.RequiredParams);

            return d.Executes(async () =>
            {
                var artifactNames = UploadArtifactName.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                await artifactProvider.UploadArtifacts(artifactNames);
            });
        };
}