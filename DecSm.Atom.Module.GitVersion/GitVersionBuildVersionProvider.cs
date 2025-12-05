namespace DecSm.Atom.Module.GitVersion;

/// <summary>
///     Provides the build version using GitVersion.
/// </summary>
/// <remarks>
///     This provider executes the GitVersion tool to extract major, minor, patch, and pre-release
///     tag information, which is then used to construct a semantic version.
///     It requires the GitVersion.Tool to be installed.
/// </remarks>
[PublicAPI]
internal sealed class GitVersionBuildVersionProvider(
    IDotnetToolInstallHelper dotnetToolInstallHelper,
    IProcessRunner processRunner
) : IBuildVersionProvider
{
    /// <summary>
    ///     Gets the semantic version of the build, derived from GitVersion's output.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the version information cannot be determined from GitVersion's output.
    /// </exception>
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

            var jsonOutput =
                JsonSerializer.Deserialize(gitVersionResult.Output, JsonElementContext.Default.JsonElement);

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
