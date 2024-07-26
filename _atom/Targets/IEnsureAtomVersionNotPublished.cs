namespace Atom.Targets;

[TargetDefinition]
internal partial interface IEnsureAtomVersionNotPublished : INugetHelper, INugetCredentials
{
    Target EnsureAtomVersionNotPublished =>
        d => d.Executes(async () =>
        {
            var projectFilePath = FileSystem.ProjectFilePath(IPackAtom.AtomProjectName);

            if (projectFilePath is null)
                throw new InvalidOperationException($"Could not find project file for {IPackAtom.AtomProjectName}");

            if (await IsNugetPackageVersionPublished(IPackAtom.AtomProjectName,
                    GetProjectPackageVersion(projectFilePath),
                    NugetFeed,
                    NugetApiKey))
                throw new CheckFailedException(
                    $"Version {GetProjectPackageVersion(projectFilePath)} of {IPackAtom.AtomProjectName} is already published.");

            Logger.LogInformation("Version {Version} of {ProjectName} is not published",
                GetProjectPackageVersion(projectFilePath),
                IPackAtom.AtomProjectName);
        });
}