namespace Atom.Targets;

[TargetDefinition]
internal partial interface ITestGithubReleaseAssetUpload : IGithubReleaseHelper, IPackAtom
{
    Target TestGithubReleaseAssetUpload =>
        d => d
            .RequiresParam(Params.GithubToken)
            .ConsumesArtifact(Commands.PackAtom, AtomProjectName)
            .Executes(async () => await UploadAssetToRelease("test1", FileSystem.AtomArtifactsDirectory / AtomProjectName));
}
