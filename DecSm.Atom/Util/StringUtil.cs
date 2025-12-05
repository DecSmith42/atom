namespace DecSm.Atom.Util;

/// <summary>
///     Provides utility methods for string manipulation and operations.
/// </summary>
[PublicAPI]
public static class StringUtil
{
    extension(string? @string)
    {
        /// <summary>
        ///     Calculates the Levenshtein distance between two strings.
        /// </summary>
        /// <param name="compareTo">The target string to compare to.</param>
        /// <returns>
        ///     The Levenshtein distance as an integer. Returns 0 if both strings are null or empty.
        ///     If one string is null/empty and the other is not, returns the length of the non-empty string.
        /// </returns>
        /// <example>
        ///     <code>
        /// "kitten".GetLevenshteinDistance("sitting"); // Returns 3
        /// "".GetLevenshteinDistance("abc"); // Returns 3
        /// "test".GetLevenshteinDistance("test"); // Returns 0
        ///     </code>
        /// </example>
        public int GetLevenshteinDistance(string? compareTo)
        {
            if (string.IsNullOrEmpty(@string))
                return string.IsNullOrEmpty(compareTo)
                    ? 0
                    : compareTo.Length;

            if (string.IsNullOrEmpty(compareTo))
                return @string.Length;

            var fromLength = @string.Length;
            var toLength = compareTo.Length;
            var distance = new Memory<int>(new int[(fromLength + 1) * (toLength + 1)]);

            for (var i = 0; i <= fromLength; i++)
                distance.Span[i * (toLength + 1)] = i;

            for (var j = 0; j <= toLength; j++)
                distance.Span[j] = j;

            for (var i = 1; i <= fromLength; i++)
            for (var j = 1; j <= toLength; j++)
            {
                var cost = compareTo[j - 1] == @string[i - 1]
                    ? 0
                    : 1;

                distance.Span[i * (toLength + 1) + j] = Math.Min(
                    Math.Min(distance.Span[(i - 1) * (toLength + 1) + j] + 1,
                        distance.Span[i * (toLength + 1) + (j - 1)] + 1),
                    distance.Span[(i - 1) * (toLength + 1) + (j - 1)] + cost);
            }

            return distance.Span[fromLength * (toLength + 1) + toLength];
        }

        /// <summary>
        ///     Sanitizes a string for safe logging by removing or replacing problematic characters and optionally truncating it.
        /// </summary>
        /// <param name="stripNewlines">If <c>true</c>, replaces newline characters with spaces. Defaults to <c>true</c>.</param>
        /// <param name="maxLength">The maximum length of the returned string. Defaults to 4096 characters.</param>
        /// <returns>
        ///     The sanitized string, or <c>null</c> if the input was <c>null</c>. If truncated, the string ends with "...".
        /// </returns>
        /// <example>
        ///     <code>
        /// "Hello\nWorld".SanitizeForLogging(); // Returns "Hello World"
        /// new string('A', 5000).SanitizeForLogging(maxLength: 100); // Returns "AAAA... (97 more A's)"
        /// "Normal text".SanitizeForLogging(); // Returns "Normal text"
        ///     </code>
        /// </example>
        [return: NotNullIfNotNull(nameof(@string))]
        public string? SanitizeForLogging(bool stripNewlines = true, int maxLength = 4096)
        {
            if (string.IsNullOrEmpty(@string))
                return @string;

            if (stripNewlines)
                @string = @string
                    .Replace(Environment.NewLine, " ")
                    .Replace("\r", " ")
                    .Replace("\n", " ");

            if (@string.Length <= maxLength)
                return @string;

            return @string[..maxLength] + "...";
        }
    }
}
