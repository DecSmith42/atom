namespace DecSm.Atom.Extensions.Dotnet;

public static class TransformProjectVersionScope
{
    public static ITransformFileScope Create(AbsolutePath file, SemVer version) =>
        ITransformFileScope.Create(file, text => MsBuildUtil.SetVersionInfo(text, version));
}
