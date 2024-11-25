namespace DecSm.Atom.Util;

[PublicAPI]
public static class StringUtil
{
    public static int GetLevenshteinDistance(this string? from, string? to)
    {
        if (string.IsNullOrEmpty(from))
            return string.IsNullOrEmpty(to)
                ? 0
                : to.Length;

        if (string.IsNullOrEmpty(to))
            return from.Length;

        var fromLength = from.Length;
        var toLength = to.Length;
        var distance = new Memory<int>(new int[(fromLength + 1) * (toLength + 1)]);

        for (var i = 0; i <= fromLength; i++)
            distance.Span[i * (toLength + 1)] = i;

        for (var j = 0; j <= toLength; j++)
            distance.Span[j] = j;

        for (var i = 1; i <= fromLength; i++)
        for (var j = 1; j <= toLength; j++)
        {
            var cost = to[j - 1] == from[i - 1]
                ? 0
                : 1;

            distance.Span[i * (toLength + 1) + j] =
                Math.Min(Math.Min(distance.Span[(i - 1) * (toLength + 1) + j] + 1, distance.Span[i * (toLength + 1) + (j - 1)] + 1),
                    distance.Span[(i - 1) * (toLength + 1) + (j - 1)] + cost);
        }

        return distance.Span[fromLength * (toLength + 1) + toLength];
    }

    [return: NotNullIfNotNull(nameof(value))]
    public static string? SanitizeForLogging(this string? value, bool stripNewlines = true, int maxLength = 4096)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        if (stripNewlines)
            value = value
                .Replace(Environment.NewLine, " ")
                .Replace("\r", " ")
                .Replace("\n", " ");

        if (value.Length <= maxLength)
            return value;

        return value[..maxLength] + "...";
    }
}
