﻿namespace DecSm.Atom.Util;

/// <summary>
///     Provides utility methods for string manipulation and operations.
/// </summary>
[PublicAPI]
public static class StringUtil
{
    /// <summary>
    ///     Calculates the Levenshtein distance between two strings.
    ///     The Levenshtein distance is the minimum number of single-character edits
    ///     (insertions, deletions, or substitutions) required to change one string into another.
    /// </summary>
    /// <param name="from">The source string to compare from. Can be null or empty.</param>
    /// <param name="to">The target string to compare to. Can be null or empty.</param>
    /// <returns>
    ///     The Levenshtein distance as an integer. Returns 0 if both strings are null or empty.
    ///     If one string is null/empty and the other is not, returns the length of the non-empty string.
    /// </returns>
    /// <example>
    ///     <code>
    /// int distance = "kitten".GetLevenshteinDistance("sitting"); // Returns 3
    /// int distance = "".GetLevenshteinDistance("abc"); // Returns 3
    /// int distance = "test".GetLevenshteinDistance("test"); // Returns 0
    /// </code>
    /// </example>
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

    /// <summary>
    ///     Sanitizes a string for safe logging by removing or replacing potentially problematic characters
    ///     and optionally truncating the string to a maximum length.
    /// </summary>
    /// <param name="value">The string value to sanitize. Can be null.</param>
    /// <param name="stripNewlines">
    ///     If <c>true</c>, replaces newline characters (Environment.NewLine, \r, \n) with spaces.
    ///     Default is <c>true</c>.
    /// </param>
    /// <param name="maxLength">
    ///     The maximum length of the returned string. If the sanitized string exceeds this length,
    ///     it will be truncated and "..." will be appended. Default is 4096 characters.
    /// </param>
    /// <returns>
    ///     The sanitized string, or null if the input was null. If the input was empty, returns the empty string.
    ///     If truncation occurs, the returned string will end with "...".
    /// </returns>
    /// <example>
    ///     <code>
    /// string sanitized = "Hello\nWorld".SanitizeForLogging(); // Returns "Hello World"
    /// string truncated = new string('A', 5000).SanitizeForLogging(maxLength: 100); // Returns 100 A's + "..."
    /// string safe = "Normal text".SanitizeForLogging(); // Returns "Normal text"
    /// </code>
    /// </example>
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
