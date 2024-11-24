namespace DecSm.Atom.Module.Dotnet;

public static class TransformProjectVersionScope
{
    public static ITransformFileScope Create(AbsolutePath file, SemVer version) =>
        ITransformFileScope.Create(file, text => MsBuildUtil.SetVersionInfo(text, version));

    public static ITransformMultiFileScope Create(IEnumerable<AbsolutePath> files, SemVer version) =>
        ITransformMultiFileScope.Create(files, text => MsBuildUtil.SetVersionInfo(text, version));
}
