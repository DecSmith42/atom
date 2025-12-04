namespace DecSm.Atom.BuildInfo;

/// <summary>
///     Default implementation of <see cref="IBuildVersionProvider" /> that provides semantic versions for build processes.
/// </summary>
/// <remarks>
///     This provider generates a build version based on the current assembly version or a fallback default version.
///     The version follows semantic versioning (SemVer) standards and is used to identify build outputs.
///     This is the fallback provider used when no custom version provider is configured.
/// </remarks>
internal sealed partial class DefaultBuildVersionProvider(IAtomFileSystem fileSystem) : IBuildVersionProvider
{
    /// <summary>
    ///     Gets the build version as a semantic version.
    /// </summary>
    /// <value>
    ///     A <see cref="SemVer" /> representing the current build version.
    ///     The version is typically derived from assembly metadata or falls back to a default version (1.0.0).
    /// </value>
    public SemVer Version
    {
        get
        {
            var directoryBuildProps = fileSystem.AtomRootDirectory / "Directory.Build.props";

            if (!directoryBuildProps.FileExists)
                return SemVer.One;

            var contents = fileSystem.File.ReadAllText(directoryBuildProps);

            var matches = VersionTagRegex()
                .Matches(contents);

            string? version = null;
            string? versionPrefix = null;
            string? versionSuffix = null;
            string? packageVersion = null;
            string? informationalVersion = null;

            foreach (Match match in matches)
            {
                var value = match.Groups["value"].Value;

                if (string.IsNullOrWhiteSpace(value) || value.Contains("$("))
                    continue;

                switch (match.Groups[1].Value)
                {
                    case "Version":
                        version = value;

                        break;
                    case "VersionPrefix":
                        versionPrefix = value;

                        break;
                    case "VersionSuffix":
                        versionSuffix = value;

                        break;
                    case "PackageVersion":
                        packageVersion = value;

                        break;
                    case "InformationalVersion":
                        informationalVersion = value;

                        break;
                }
            }

            return SemVer.TryParse(informationalVersion, out var informationalSemVer)
                ? informationalSemVer
                : SemVer.TryParse(packageVersion, out var packageSemVer)
                    ? packageSemVer
                    : SemVer.TryParse(version, out var versionSemVer)
                        ? versionSemVer
                        : SemVer.TryParse($"{versionPrefix}-{versionSuffix}", out var combinedSemVer)
                            ? combinedSemVer
                            : SemVer.TryParse(versionPrefix, out var prefixSemVer)
                                ? prefixSemVer
                                : SemVer.One;
        }
    }

    [GeneratedRegex("<(Version|VersionPrefix|VersionSuffix|PackageVersion|InformationalVersion)>(?<value>.+)</\\1>")]
    private static partial Regex VersionTagRegex();
}
