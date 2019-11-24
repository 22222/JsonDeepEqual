using System;
using System.Text.RegularExpressions;

namespace Two.JsonDeepEqual.Utilities
{
    /// <summary>
    /// Utility methods for glob patterns.
    /// </summary>
    internal static class GlobConvert
    {
        private static readonly char[] GlobChars = new char[] { '*', '?' };

        /// <summary>
        /// Returns true if the given pattern contains any glob characters (like *).
        /// </summary>
        /// <param name="pattern">The pattern to check, or null.</param>
        /// <returns>True if the pattern contains any characters with special handling in a glob pattern.</returns>
        public static bool IsGlobPattern(string? pattern)
        {
            return pattern != null && pattern.IndexOfAny(GlobChars) >= 0;
        }

        /// <summary>
        /// Returns true if the given pattern should be treated as an absolute path.
        /// </summary>
        /// <param name="pattern">The pattern to check, or null.</param>
        /// <returns>True if the pattern starts with a directory separator character.</returns>
        public static bool IsAbsolutePathPattern(string? pattern)
        {
            if (pattern == null || pattern.Length == 0)
            {
                return false;
            }
            return pattern[0] == '/' || pattern[0] == '\\';
        }

        /// <summary>
        /// Converts a glob pattern to a regular expression, or returns null if the global pattern is null or empty.
        /// </summary>
        /// <param name="globPattern">The glob pattern to convert to a regular expression.</param>
        /// <param name="ignoreCase">True if the regular expression should be case insensitive.</param>
        /// <returns>The <see cref="Regex"/> or null.</returns>
        public static Regex? CreatePathRegexOrNull(string? globPattern, bool ignoreCase)
        {
            if (string.IsNullOrEmpty(globPattern))
            {
                return null;
            }

            var escapedPattern = Regex.Escape(globPattern) + "$";
            if (IsAbsolutePathPattern(globPattern))
            {
                escapedPattern = "^" + escapedPattern;
            }
            else
            {
                escapedPattern = @"(?:^|/|\\)" + escapedPattern;
            }

#pragma warning disable CA1307 // Specify StringComparison
            var regexPattern = escapedPattern
                .Replace(@"\*\*", ".*")
                .Replace(@"\*", @"[^/\\]*")
                .Replace(@"\?", @"[^/\\]");
#pragma warning restore CA1307 // Specify StringComparison
            var regexOptions = RegexOptions.None;
            if (ignoreCase)
            {
                regexOptions |= RegexOptions.IgnoreCase;
            }
            return new Regex(regexPattern, regexOptions);
        }
    }
}
