namespace DecSm.Atom.Artifacts;

[TargetDefinition]
public interface IDownloadArtifact : IBuildDefinition
{
    [ParamDefinition("download-artifact-name", "The name of the artifact to download.")]
    string DownloadArtifactName => GetParam(() => DownloadArtifactName)!;
    
    Target DownloadArtifact =>
        d =>
        {
            var artifactProvider = Services.GetRequiredService<IArtifactProvider>();
            
            d.ConsumedVariables.Add(new(nameof(ISetup.Setup), nameof(ISetup.AtomBuildId)));
            
            d.RequiredParams.Add(nameof(DownloadArtifactName));
            d.RequiredParams.AddRange(artifactProvider.RequiredParams);
            
            return d.Executes(async () =>
            {
                var artifactNames = DownloadArtifactName.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                await artifactProvider.DownloadArtifacts(artifactNames);
            });
        };
}