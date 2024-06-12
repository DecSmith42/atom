namespace DecSm.Atom.Util;

public static class MsBuildUtil
{
    public static VersionInfo ParseVersionInfo(string msBuildFilePath)
    {
        var sdkVersion = GetDotnetSdkVersion();
        Console.WriteLine($"Using .NET SDK version: {sdkVersion}");
        var sdkLocations = GetDotnetSdkLocations();
        Console.WriteLine($"Found .NET SDK locations: {string.Join(", ", sdkLocations)}");
        var currentSdk = sdkLocations.FirstOrDefault(s => s.Contains(sdkVersion));
        Console.WriteLine($"Using .NET SDK location: {currentSdk}");

        var currentSdkPath = currentSdk
            ?.Split('[', ']')[1];

        Console.WriteLine($"Using .NET SDK path: {currentSdkPath}");
        Environment.SetEnvironmentVariable("MSBuildSDKsPath", currentSdkPath);
        Console.WriteLine($"Set MSBuildSDKsPath to: {Environment.GetEnvironmentVariable("MSBuildSDKsPath")}");

        return ParseVersionInfoInternal(msBuildFilePath);
    }

    private static VersionInfo ParseVersionInfoInternal(string msBuildFilePath)
    {
        var project = Project.FromFile(msBuildFilePath, new());

        var versionPrefix = ParseVersionSem(project.GetPropertyValue("VersionPrefix"));
        var versionSuffix = ParseVersionFreetext(project.GetPropertyValue("VersionSuffix"));
        var version = ParseVersionWithSuffix(project.GetPropertyValue("Version"));
        var packageVersion = ParseVersionWithSuffix(project.GetPropertyValue("PackageVersion"));
        var assemblyVersion = ParseVersionWithRevision(project.GetPropertyValue("AssemblyVersion"));
        var fileVersion = ParseVersionWithRevision(project.GetPropertyValue("FileVersion"));
        var informationalVersion = ParseVersionFreetext(project.GetPropertyValue("InformationalVersion"));

        return new(versionPrefix, versionSuffix, version, packageVersion, assemblyVersion, fileVersion, informationalVersion);
    }


    private static string? GetDotnetSdkVersion()
    {
        var sdkVersion = string.Empty;

        try
        {
            using var process = new Process();

            process.StartInfo.FileName = "dotnet";
            process.StartInfo.Arguments = "--version";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();

            sdkVersion = process.StandardOutput.ReadLine();
            process.WaitForExit();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving .NET SDK version: {ex.Message}");
        }

        return sdkVersion;
    }

    private static string[] GetDotnetSdkLocations()
    {
        var sdkLocations = new List<string>();

        try
        {
            using var process = new Process();

            process.StartInfo.FileName = "dotnet";
            process.StartInfo.Arguments = "--list-sdks";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();

            while (process.StandardOutput.ReadLine() is { } line)
                sdkLocations.Add(line);

            process.WaitForExit();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving .NET SDK locations: {ex.Message}");
        }

        return sdkLocations.ToArray();
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

public sealed record VersionInfo
{
    private readonly VersionSem _prefix;
    private readonly VersionFreetext? _suffix;
    private readonly VersionWithSuffix? _version;
    private readonly VersionWithSuffix? _packageVersion;
    private readonly VersionWithRevision? _assemblyVersion;
    private readonly VersionWithRevision? _fileVersion;
    private readonly VersionFreetext? _informationalVersion;

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