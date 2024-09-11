namespace DecSm.Atom;

public sealed partial class SemVer : ISpanParsable<SemVer>, IComparable<SemVer>, IComparisonOperators<SemVer, SemVer, bool>
{
    public int Major { get; private init; }

    public int Minor { get; private init; }

    public int Patch { get; private init; }

    public string Prefix => $"{Major}.{Minor}.{Patch}";

    public string? PreRelease { get; private init; }

    public bool IsPreRelease => PreRelease != null;

    public int BuildNumberFromPreRelease => ExtractBuildNumber(PreRelease);

    public string? Metadata { get; private init; }

    public int BuildNumberFromMetadata => ExtractBuildNumber(Metadata);

    public int CompareTo(SemVer? other)
    {
        if (other is null)
            return 1;

        var majorComparison = Major.CompareTo(other.Major);

        if (majorComparison != 0)
            return majorComparison;

        var minorComparison = Minor.CompareTo(other.Minor);

        if (minorComparison != 0)
            return minorComparison;

        var patchComparison = Patch.CompareTo(other.Patch);

        if (patchComparison != 0)
            return patchComparison;

        switch (PreRelease, other.PreRelease)
        {
            case (null, not null):
                return 1;
            case (not null, null):
                return -1;
            case (null, null):
                return string.Compare(Metadata, other.Metadata, StringComparison.Ordinal);
        }

        var preReleaseParts = PreRelease.Split('.');
        var otherPreReleaseParts = other.PreRelease.Split('.');

        for (var i = 0; i < Math.Min(preReleaseParts.Length, otherPreReleaseParts.Length); i++)
            if (int.TryParse(preReleaseParts[i], out var preReleasePart) &&
                int.TryParse(otherPreReleaseParts[i], out var otherPreReleasePart))
            {
                var preReleasePartComparison = preReleasePart.CompareTo(otherPreReleasePart);

                if (preReleasePartComparison != 0)
                    return preReleasePartComparison;
            }
            else
            {
                var preReleasePartComparison = string.Compare(preReleaseParts[i], otherPreReleaseParts[i], StringComparison.Ordinal);

                if (preReleasePartComparison != 0)
                    return preReleasePartComparison;
            }

        var preReleaseLengthComparison = preReleaseParts.Length.CompareTo(otherPreReleaseParts.Length);

        if (preReleaseLengthComparison != 0)
            return preReleaseLengthComparison;

        return string.Compare(Metadata, other.Metadata, StringComparison.Ordinal);
    }

    public static bool operator >(SemVer left, SemVer right) =>
        left.CompareTo(right) > 0;

    public static bool operator >=(SemVer left, SemVer right) =>
        left.CompareTo(right) >= 0;

    public static bool operator <(SemVer left, SemVer right) =>
        left.CompareTo(right) < 0;

    public static bool operator <=(SemVer left, SemVer right) =>
        left.CompareTo(right) <= 0;

    public static bool operator ==(SemVer? left, SemVer? right) =>
        (left is null && right is null) || (left is not null && left.Equals(right));

    public static bool operator !=(SemVer? left, SemVer? right) =>
        !(left == right);

    public static SemVer Parse(string s, IFormatProvider? provider)
    {
        var match = SemVerRegex()
            .Match(s);

        if (!match.Success)
            throw new ArgumentException($"Invalid version string '{s}'.", nameof(s));

        return new()
        {
            Major = int.Parse(match.Groups[1].Value),
            Minor = int.Parse(match.Groups[2].Value),
            Patch = int.Parse(match.Groups[3].Value),
            PreRelease = string.IsNullOrWhiteSpace(match.Groups[4].Value)
                ? null
                : match.Groups[4].Value,
            Metadata = string.IsNullOrWhiteSpace(match.Groups[5].Value)
                ? null
                : match.Groups[5].Value,
        };
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out SemVer result)
    {
        if (s is null)
        {
            result = null;

            return false;
        }

        var match = SemVerRegex()
            .Match(s);

        if (!match.Success ||
            !int.TryParse(match.Groups[1].Value, out var major) ||
            major < 0 ||
            !int.TryParse(match.Groups[2].Value, out var minor) ||
            minor < 0 ||
            !int.TryParse(match.Groups[3].Value, out var patch) ||
            patch < 0)
        {
            result = null;

            return false;
        }

        var preRelease = match.Groups[4].Value;
        var metadata = match.Groups[5].Value;

        result = new()
        {
            Major = major,
            Minor = minor,
            Patch = patch,
            PreRelease = string.IsNullOrWhiteSpace(preRelease)
                ? null
                : preRelease,
            Metadata = string.IsNullOrWhiteSpace(metadata)
                ? null
                : metadata,
        };

        return true;
    }

    public static SemVer Parse(ReadOnlySpan<char> s, IFormatProvider? provider) =>
        Parse(s.ToString(), provider);

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out SemVer result) =>
        TryParse(s.ToString(), provider, out result);

    public bool IsBetween(SemVer firstBound, SemVer secondBound) =>
        firstBound == secondBound
            ? Equals(firstBound)
            : firstBound < secondBound
                ? firstBound < this && this < secondBound
                : secondBound < this && this < firstBound;

    public Version ToSystemVersion(bool throwIfContainsPreRelease = false, bool throwIfContainsMetadata = false) =>
        throwIfContainsPreRelease && PreRelease is not null
            ? throw new ArgumentException("The SemVer contains a pre-release tag, which is not supported by System.Version.")
            : throwIfContainsMetadata && Metadata is not null
                ? throw new ArgumentException("The SemVer contains metadata, which is not supported by System.Version.")
                : new(Major, Minor, Patch);

    public static SemVer FromSystemVersion(Version version, bool throwIfContainsRevision = false) =>
        throwIfContainsRevision && version.Revision > 0
            ? throw new ArgumentException("The version contains a revision number, which is not supported by SemVer.")
            : new()
            {
                Major = version.Major,
                Minor = version.Minor,
                Patch = version.Build,
            };

    public static int ExtractBuildNumber(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return 0;

        var matches = NumberRegex()
            .Matches(input);

        return matches.Count is 1
            ? int.Parse(matches[0].Value)
            : 0;
    }

    public static SemVer Parse(string s) =>
        Parse(s, null);

    public static bool TryParse([NotNullWhen(true)] string? s, [MaybeNullWhen(false)] out SemVer result) =>
        TryParse(s, null, out result);

    public static bool TryParse(ReadOnlySpan<char> s, [MaybeNullWhen(false)] out SemVer result) =>
        TryParse(s.ToString(), out result);

    public bool Equals(SemVer? other) =>
        CompareTo(other) == 0;

    public override bool Equals(object? obj) =>
        obj is SemVer semVer && Equals(semVer);

    public override int GetHashCode() =>
        HashCode.Combine(Major, Minor, Patch, PreRelease, Metadata);

    public static implicit operator SemVer(string s) =>
        Parse(s);

    public static implicit operator string(SemVer semVer) =>
        semVer.ToString();

    public override string ToString() =>
        (PreRelease, Metadata) switch
        {
            (not null, not null) => $"{Major}.{Minor}.{Patch}-{PreRelease}+{Metadata}",
            (not null, null) => $"{Major}.{Minor}.{Patch}-{PreRelease}",
            (null, not null) => $"{Major}.{Minor}.{Patch}+{Metadata}",
            _ => $"{Major}.{Minor}.{Patch}",
        };

    [GeneratedRegex(
        @"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-((?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+([0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$")]
    private static partial Regex SemVerRegex();

    [GeneratedRegex(@"\d+")]
    private static partial Regex NumberRegex();
}
