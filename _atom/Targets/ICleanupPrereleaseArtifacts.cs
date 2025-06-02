using DecSm.Atom.BuildInfo;

namespace Atom.Targets;

[TargetDefinition]
internal partial interface ICleanupPrereleaseArtifacts : IBuildInfo
{
    Target CleanupPrereleaseArtifacts =>
        t => t
            .DescribedAs("Cleans up stored artifacts from prerelease builds up to the second-most recent stable build.")
            .Executes(async () =>
            {
                if (BuildVersion.IsPreRelease)
                {
                    Logger.LogInformation("Current version is prerelease, no cleanup will be performed");

                    return;
                }

                var knownBuildIds = await GetService<IArtifactProvider>()
                    .GetStoredRunIdentifiers();

                var knownVersions = knownBuildIds
                    .Select(static x => SemVer.TryParse(x, out var semVer)
                        ? semVer
                        : null)
                    .OfType<SemVer>()
                    .OrderByDescending(static x => x)
                    .ToArray();

                var stableVersions = knownVersions
                    .Where(x => !x.IsPreRelease)
                    .ToArray();

                if (stableVersions.Length < 2)
                {
                    Logger.LogInformation("Fewer than two stable versions found, no cleanup will be performed");

                    return;
                }

                var secondMostRecentStableVersion = stableVersions[1];

                Logger.LogInformation("Cleaning prerelease builds up to version: {Version}", secondMostRecentStableVersion);

                var versionsToCleanup = knownVersions
                    .Where(x => x.IsPreRelease && x < secondMostRecentStableVersion)
                    .ToArray();

                if (versionsToCleanup.Length == 0)
                {
                    Logger.LogInformation("No versions to cleanup, exiting");

                    return;
                }

                var versionStringsToCleanup = versionsToCleanup
                    .Select(static x => x.ToString())
                    .ToArray();

                Logger.LogInformation("Cleanup versions: [ {Versions} ]", string.Join(", ", versionStringsToCleanup));

                await GetService<IArtifactProvider>()
                    .Cleanup(versionStringsToCleanup);
            });
}
