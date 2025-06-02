namespace DecSm.Atom.BuildInfo;

internal sealed partial class DefaultBuildVersionProvider(IAtomFileSystem fileSystem) : IBuildVersionProvider
{
    public SemVer Version
    {
        get
        {
            var directoryBuildProps = fileSystem.AtomRootDirectory / "Directory.Build.props";

            if (!directoryBuildProps.FileExists)
                throw new InvalidOperationException(
                    $"File required for {nameof(DefaultBuildVersionProvider)} but not found: {directoryBuildProps}");

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
                                : throw new InvalidOperationException($"Unable to determine version from {directoryBuildProps}");
        }
    }

    [GeneratedRegex("<(Version|VersionPrefix|VersionSuffix|PackageVersion|InformationalVersion)>(?<value>.+)</\\1>")]
    private static partial Regex VersionTagRegex();
}
