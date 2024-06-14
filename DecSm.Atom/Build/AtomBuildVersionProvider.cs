namespace DecSm.Atom.Build;

internal sealed class AtomBuildVersionProvider(IFileSystem fileSystem) : IBuildVersionProvider
{
    public VersionInfo Version
    {
        get
        {
            var solutionRoot = fileSystem.SolutionRoot();

            var directoryBuildProps = solutionRoot / "Directory.Build.props";

            if (!directoryBuildProps.FileExists)
                throw new($"File required for {nameof(AtomBuildVersionProvider)} but not found: {directoryBuildProps}");

            return MsBuildUtil.GetVersionInfo(directoryBuildProps);
        }
    }
}