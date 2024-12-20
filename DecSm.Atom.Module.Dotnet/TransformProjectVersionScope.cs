﻿namespace DecSm.Atom.Module.Dotnet;

[PublicAPI]
public static class TransformProjectVersionScope
{
    public static TransformFileScope Create(RootedPath file, SemVer version) =>
        TransformFileScope.Create(file, text => MsBuildUtil.SetVersionInfo(text, version));

    public static TransformMultiFileScope Create(IEnumerable<RootedPath> files, SemVer version) =>
        TransformMultiFileScope.Create(files, text => MsBuildUtil.SetVersionInfo(text, version));
}
