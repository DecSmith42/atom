using DecSm.Atom.Process;

namespace DecSm.Atom.Extensions.GitVersioning;

public class GitVersioningBuildVersionProvider(IProcessRunner processRunner) : IBuildVersionProvider
{
    private VersionInfo? _version;

    public VersionInfo Version
    {
        get
        {
            if (_version is not null)
                return _version;

            processRunner.RunProcess("dotnet", "tool install --global nbgv");

            var output = processRunner.RunProcess("nbgv", "get-version -f json");
            var jsonOutput = JsonSerializer.Deserialize<JsonElement>(output);

            var majorProp = jsonOutput
                .GetProperty("VersionMajor")
                .GetUInt32();

            var minorProp = jsonOutput
                .GetProperty("VersionMinor")
                .GetUInt32();

            var patchProp = jsonOutput
                .GetProperty("BuildNumber")
                .GetUInt32();

            var revisionProp = jsonOutput
                .GetProperty("VersionHeight")
                .GetUInt32();

            var preReleaseTagProp = jsonOutput
                .GetProperty("PrereleaseVersionNoLeadingHyphen")
                .GetString()!;

            var informationalVersionProp = jsonOutput
                .GetProperty("AssemblyInformationalVersion")
                .GetString()!;

            var prefix = new VersionSem(majorProp, minorProp, patchProp);
            var suffix = new VersionFreetext(preReleaseTagProp);
            var version = new VersionWithSuffix(prefix, suffix);
            var packageVersion = new VersionWithSuffix(prefix, suffix);
            var assemblyVersion = new VersionWithRevision(prefix, 0);
            var fileInfoVersion = new VersionWithRevision(prefix, revisionProp);
            var informationalVersion = new VersionFreetext(informationalVersionProp);

            return _version = new(prefix, suffix, version, packageVersion, assemblyVersion, fileInfoVersion, informationalVersion);
        }
    }
}