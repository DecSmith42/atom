namespace Atom.Helpers;

[TargetDefinition]
public partial interface IDotnetVersionHelper
{
    string GetProjectPackageVersion(AbsolutePath project) =>
        MsBuildUtil
            .ParseVersionInfo(project)
            .PackageVersion
            .ToString();

    string GetProjectVersion(AbsolutePath project) =>
        MsBuildUtil
            .ParseVersionInfo(project)
            .Version
            .ToString();

    string GetProjectAssemblyVersion(AbsolutePath project) =>
        MsBuildUtil
            .ParseVersionInfo(project)
            .AssemblyVersion
            .ToString();

    string GetProjectFileVersion(AbsolutePath project) =>
        MsBuildUtil
            .ParseVersionInfo(project)
            .FileVersion
            .ToString();

    string GetProjectInformationalVersion(AbsolutePath project) =>
        MsBuildUtil
            .ParseVersionInfo(project)
            .InformationalVersion
            .ToString();
}