﻿using DecSm.Atom.BuildInfo;

namespace DecSm.Atom.Module.GitVersion;

internal sealed class GitVersionBuildVersionProvider(IDotnetToolInstallHelper dotnetToolInstallHelper, IProcessRunner processRunner)
    : IBuildVersionProvider
{
    private SemVer? _version;

    public SemVer Version
    {
        get
        {
            if (_version is not null)
                return _version;

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

            return _version = preReleaseTagProp is { Length: > 0 }
                ? SemVer.Parse($"{majorProp}.{minorProp}.{patchProp}-{preReleaseTagProp}")
                : SemVer.Parse($"{majorProp}.{minorProp}.{patchProp}");
        }
    }
}
