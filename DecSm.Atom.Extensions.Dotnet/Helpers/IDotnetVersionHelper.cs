namespace DecSm.Atom.Extensions.Dotnet.Helpers;

[TargetDefinition]
public partial interface IDotnetVersionHelper
{
    string GetProjectPackageVersion(AbsolutePath project) =>
        MsBuildUtil
            .GetVersionInfo(project)
            .PackageVersion
            .ToString();

    string GetProjectVersion(AbsolutePath project) =>
        MsBuildUtil
            .GetVersionInfo(project)
            .Version
            .ToString();

    string GetProjectAssemblyVersion(AbsolutePath project) =>
        MsBuildUtil
            .GetVersionInfo(project)
            .AssemblyVersion
            .ToString();

    string GetProjectFileVersion(AbsolutePath project) =>
        MsBuildUtil
            .GetVersionInfo(project)
            .FileVersion
            .ToString();

    string GetProjectInformationalVersion(AbsolutePath project) =>
        MsBuildUtil
            .GetVersionInfo(project)
            .InformationalVersion
            .ToString();
}