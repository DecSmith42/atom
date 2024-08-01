namespace DecSm.Atom.Extensions.GitVersion;

public class GitVersionBuildVersionProvider(IDotnetToolHelper dotnetToolHelper, IProcessRunner processRunner) : IBuildVersionProvider
{
    private VersionInfo? _version;

    public VersionInfo Version
    {
        get
        {
            if (_version is not null)
                return _version;

            dotnetToolHelper.InstallTool("GitVersion.Tool");

            var gitVersionResult = processRunner.Run(new("dotnet", "gitversion /output json")
            {
                InvocationLogLevel = LogLevel.Debug,
            });

            var jsonOutput = JsonSerializer.Deserialize<JsonElement>(gitVersionResult.Output);

            var majorProp = jsonOutput
                .GetProperty("Major")
                .GetUInt32();

            var minorProp = jsonOutput
                .GetProperty("Minor")
                .GetUInt32();

            var patchProp = jsonOutput
                .GetProperty("Patch")
                .GetUInt32();

            var revisionProp = jsonOutput
                .GetProperty("PreReleaseNumber")
                .GetUInt32();

            var preReleaseTagProp = jsonOutput
                .GetProperty("PreReleaseTag")
                .GetString()!;

            var informationalVersionProp = jsonOutput
                .GetProperty("InformationalVersion")
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