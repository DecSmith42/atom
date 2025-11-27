namespace DecSm.Atom.Module.GitVersion;

internal sealed class GitVersionBuildVersionProvider(
    IDotnetToolInstallHelper dotnetToolInstallHelper,
    IProcessRunner processRunner
) : IBuildVersionProvider
{
    [field: AllowNull]
    [field: MaybeNull]
    public SemVer Version
    {
        get
        {
            if (field is not null)
                return field;

            dotnetToolInstallHelper.InstallTool("GitVersion.Tool");

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

            var preReleaseTagProp = jsonOutput
                .GetProperty("PreReleaseTag")
                .GetString()!;

            return field = preReleaseTagProp is { Length: > 0 }
                ? SemVer.Parse($"{majorProp}.{minorProp}.{patchProp}-{preReleaseTagProp}")
                : SemVer.Parse($"{majorProp}.{minorProp}.{patchProp}");
        }
    }
}
