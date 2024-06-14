namespace DecSm.Atom.Util;

public static class MsBuildUtil
{
    public static VersionInfo GetVersionInfo(AbsolutePath msBuildFile, bool includeDirectoryBuildProps = true)
    {
        var properties = GetProperties(msBuildFile, includeDirectoryBuildProps);

        var versionPrefix = ParseVersionSem(properties.GetValueOrDefault("VersionPrefix"));
        var versionSuffix = ParseVersionFreetext(properties.GetValueOrDefault("VersionSuffix"));
        var version = ParseVersionWithSuffix(properties.GetValueOrDefault("Version"));
        var packageVersion = ParseVersionWithSuffix(properties.GetValueOrDefault("PackageVersion"));
        var assemblyVersion = ParseVersionWithRevision(properties.GetValueOrDefault("AssemblyVersion"));
        var fileVersion = ParseVersionWithRevision(properties.GetValueOrDefault("FileVersion"));
        var informationalVersion = ParseVersionFreetext(properties.GetValueOrDefault("InformationalVersion"));

        return new(versionPrefix, versionSuffix, version, packageVersion, assemblyVersion, fileVersion, informationalVersion);
    }

    private static Dictionary<string, string> GetProperties(AbsolutePath msBuildFile, bool includeDirectoryBuildProps = true)
    {
        var msBuildFiles = ReadMsBuildFiles(msBuildFile, includeDirectoryBuildProps);
        var unresolvedProperties = ParsePropertiesFromFiles(msBuildFiles);

        PopulateMissingVersionProperties(unresolvedProperties);

        return ResolveProperties(unresolvedProperties);
    }

    private static List<AbsolutePath> ReadMsBuildFiles(AbsolutePath msBuildFile, bool includeDirectoryBuildProps)
    {
        List<AbsolutePath> msBuildFiles = [msBuildFile];

        if (includeDirectoryBuildProps)
        {
            var currentDirectory = msBuildFile.Parent;

            while (currentDirectory?.Exists is true)
            {
                var directoryBuildProps = currentDirectory / "Directory.Build.props";

                if (directoryBuildProps.FileExists)
                    msBuildFiles.Add(directoryBuildProps);

                if (string.Equals(currentDirectory, msBuildFile.FileSystem.SolutionRoot(), StringComparison.OrdinalIgnoreCase))
                    break;

                currentDirectory = currentDirectory.Parent;
            }
        }

        msBuildFiles.Reverse();

        return msBuildFiles;
    }

    private static Dictionary<string, string> ParsePropertiesFromFiles(List<AbsolutePath> msBuildFiles)
    {
        var unresolvedProperties = new Dictionary<string, string>();

        foreach (var fileContents in msBuildFiles.Select(file => file.FileSystem.File.ReadAllText(file)))
        {
            if (fileContents is not { Length: > 0 })
                continue;

            var msBuildFileXml = XDocument.Parse(fileContents);

            // Get all properties
            var props = msBuildFileXml
                .Root
                ?.Elements("PropertyGroup")
                .SelectMany(pg => pg.Elements())
                .Select(x => new MsBuildProperty(x.Name.LocalName)
                {
                    Value = x.Value,
                });

            foreach (var prop in props ?? [])
                unresolvedProperties[prop.Name] = prop.Value;
        }

        return unresolvedProperties;
    }

    private static void PopulateMissingVersionProperties(Dictionary<string, string> unresolvedProperties)
    {
        unresolvedProperties.TryAdd("VersionPrefix", "1.0.0");
        unresolvedProperties.TryAdd("VersionSuffix", string.Empty);

        unresolvedProperties.TryAdd("Version",
            unresolvedProperties["VersionPrefix"] is { Length: > 0 }
                ? $"{unresolvedProperties["VersionPrefix"]}-{unresolvedProperties["VersionSuffix"]}"
                : unresolvedProperties["VersionPrefix"]);

        unresolvedProperties.TryAdd("PackageVersion", unresolvedProperties["Version"]);
        unresolvedProperties.TryAdd("AssemblyVersion", $"{unresolvedProperties["VersionPrefix"]}.0");
        unresolvedProperties.TryAdd("FileVersion", $"{unresolvedProperties["VersionPrefix"]}.0");
    }

    private static Dictionary<string, string> ResolveProperties(Dictionary<string, string> unresolvedProperties)
    {
        var properties = unresolvedProperties
            .Select(x => new MsBuildProperty(x.Key)
            {
                Value = x.Value,
            })
            .ToList();

        foreach (var property in properties)
            property.Resolved = properties.All(x => !property.Value.Contains(x.Name, StringComparison.OrdinalIgnoreCase));

        foreach (var property in properties)
            ResolveProperty(properties, property, 0);

        return properties
            .Where(x => x.Value is { Length: > 0 })
            .ToDictionary(x => x.Name, x => x.Value);
    }

    // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local - used to prevent infinite recursion
    private static void ResolveProperty(List<MsBuildProperty> properties, MsBuildProperty msBuildProperty, int depth)
    {
        if (depth > properties.Count * 2)
            throw new InvalidOperationException("Circular reference detected");

        if (msBuildProperty.Resolved)
            return;

        var value = msBuildProperty.Value;

        foreach (var otherProperty in properties.Where(otherProperty => value.Contains(otherProperty.Name)))
        {
            if (!otherProperty.Resolved)
                ResolveProperty(properties, otherProperty, depth + 1);

            value = value.Replace($"$({otherProperty.Name})", otherProperty.Value);
        }

        msBuildProperty.Value = value;
    }

    private static VersionSem? ParseVersionSem(string? text)
    {
        if (text is null or "")
            return null;

        var parts = text.Split('.');

        if (!uint.TryParse(parts[0], out var major))
            throw new InvalidOperationException("Major version must be a number");

        if (parts.Length == 1)
            return new(major, 0, 0);

        if (!uint.TryParse(parts[1], out var minor))
            throw new InvalidOperationException("Minor version must be a number");

        if (parts.Length == 2)
            return new(major, minor, 0);

        if (!uint.TryParse(parts[2], out var patch))
            throw new InvalidOperationException("Patch version must be a number");

        if (parts.Length > 3)
            throw new InvalidOperationException("Version must have at most 3 parts");

        return new(major, minor, patch);
    }

    private static VersionFreetext? ParseVersionFreetext(string? text) =>
        text is null or ""
            ? null
            : new(text);

    private static VersionWithSuffix? ParseVersionWithSuffix(string? text)
    {
        if (text is null or "")
            return null;

        var parts = text.Split('-');

        if (parts.Length == 0)
            return null;

        if (parts.Length > 2)
            parts = [parts[0], string.Join('-', parts.Skip(1))];

        var prefix = ParseVersionSem(parts[0]);

        if (prefix is null)
            return null;

        var suffix = parts.Length == 1
            ? null
            : parts[1];

        if (suffix is null or "")
            return new(prefix, null);

        return new(prefix, new(suffix));
    }

    private static VersionWithRevision? ParseVersionWithRevision(string? text)
    {
        if (text is null or "")
            return null;

        var parts = text.Split('.', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
            return null;

        if (!uint.TryParse(parts[0], out var major))
            throw new InvalidOperationException("Major version must be a number");

        if (parts.Length == 1)
            return new(new(major, 0, 0), 0);

        if (!uint.TryParse(parts[1], out var minor))
            throw new InvalidOperationException("Minor version must be a number");

        if (parts.Length == 2)
            return new(new(major, minor, 0), 0);

        if (!uint.TryParse(parts[2], out var patch))
            throw new InvalidOperationException("Patch version must be a number");

        if (parts.Length == 3)
            return new(new(major, minor, patch), 0);

        if (!uint.TryParse(parts[3], out var revision))
            throw new InvalidOperationException("Revision version must be a number");

        return new(new(major, minor, patch), revision);
    }
}

public sealed record MsBuildProperty(string Name)
{
    public required string Value { get; set; }

    public bool Resolved { get; set; }
}

public sealed record VersionInfo
{
    private readonly VersionWithRevision? _assemblyVersion;
    private readonly VersionWithRevision? _fileVersion;
    private readonly VersionFreetext? _informationalVersion;
    private readonly VersionWithSuffix? _packageVersion;
    private readonly VersionSem _prefix;
    private readonly VersionFreetext? _suffix;
    private readonly VersionWithSuffix? _version;

    public VersionInfo(
        VersionSem? prefix,
        VersionFreetext? suffix,
        VersionWithSuffix? version,
        VersionWithSuffix? packageVersion,
        VersionWithRevision? assemblyVersion,
        VersionWithRevision? fileVersion,
        VersionFreetext? informationalVersion)
    {
        if (prefix is null)
        {
            if (version is null)
                throw new InvalidOperationException("Version must be specified if prefix is not");

            prefix = version.Prefix;
        }

        _prefix = prefix;
        _suffix = suffix;
        _version = version;
        _packageVersion = packageVersion;
        _assemblyVersion = assemblyVersion;
        _fileVersion = fileVersion;
        _informationalVersion = informationalVersion;
    }

    public VersionWithSuffix Version =>
        (_prefix, _suffix, _version) switch
        {
            (_, null, null) => new(_prefix, null),
            (_, _, null) => new(_prefix, _suffix),
            (_, _, _) => _version,
        };

    public VersionWithSuffix PackageVersion => _packageVersion ?? Version;

    public VersionWithRevision AssemblyVersion => _assemblyVersion ?? new(_prefix, 0);

    public VersionWithRevision FileVersion => _fileVersion ?? new(_prefix, 0);

    public VersionFreetext InformationalVersion => _informationalVersion ?? new(_prefix.ToString());
}

public sealed record VersionSem(uint Major, uint Minor, uint Patch)
{
    public override string ToString() =>
        $"{Major}.{Minor}.{Patch}";
}

public sealed record VersionFreetext(string Value)
{
    public override string ToString() =>
        Value;
}

public sealed record VersionWithSuffix(VersionSem Prefix, VersionFreetext? Suffix)
{
    public override string ToString() =>
        Suffix is null
            ? Prefix.ToString()
            : $"{Prefix}-{Suffix}";
}

public sealed record VersionWithRevision(VersionSem Version, uint Revision)
{
    public override string ToString() =>
        $"{Version}.{Revision}";
}