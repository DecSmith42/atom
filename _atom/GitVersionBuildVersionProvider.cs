using DecSm.Atom.Build;

namespace Atom;

public class GitVersionBuildVersionProvider(IProcessRunner processRunner) : IBuildVersionProvider
{
    private VersionInfo? _version;

    public VersionInfo Version
    {
        get
        {
            if (_version is not null)
                return _version;

            processRunner.RunProcess("dotnet", "tool install --global GitVersion.Tool");

            var output = processRunner.RunProcess("dotnet", "gitversion");
            var jsonOutput = JsonSerializer.Deserialize<JsonElement>(output);

            var majorProp = jsonOutput
                .GetProperty("Major")
                .GetUInt32()!;

            var minorProp = jsonOutput
                .GetProperty("Minor")
                .GetUInt32()!;

            var patchProp = jsonOutput
                .GetProperty("Patch")
                .GetUInt32()!;

            var preReleaseTagProp = jsonOutput
                .GetProperty("PreReleaseTag")
                .GetString()!;

            var informationalVersionProp = jsonOutput
                .GetProperty("InformationalVersion")
                .GetString()!;

            var prefix = new VersionSem(majorProp, minorProp, patchProp);
            var suffix = new VersionFreetext(preReleaseTagProp);
            var informationalVersion = new VersionFreetext(informationalVersionProp);

            return _version = new(prefix, suffix, null, null, null, null, informationalVersion);
        }
    }
}