using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Two.JsonDeepEqual
{
    /// <summary>
    /// Finds the differences between two objects based on their JSON representation.
    /// </summary>
    public static class JsonDeepEqualDiff
    {
        /// <summary>
        /// Finds the differences between two objects, using a JSON serialization comparer.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The value to be compared against.</param>
        /// <returns>The differences, or an empty enumerable if the two objects are equal.</returns>
        public static IEnumerable<JsonDiffNode> EnumerateDifferences(object? expected, object? actual)
            => EnumerateDifferences(expected, actual, options: null);

        /// <summary>
        /// Finds the differences between two objects, using a JSON serialization comparer.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The value to be compared against.</param>
        /// <param name="options">Options that control the comparison, or null to use default options.</param>
        /// <returns>The differences, or an empty enumerable if the two objects are equal.</returns>
        public static IEnumerable<JsonDiffNode> EnumerateDifferences(object? expected, object? actual, JsonDeepEqualDiffOptions? options)
        {
            if (options == null)
            {
                options = new JsonDeepEqualDiffOptions();
            }

            var jsonSerializer = options.ToJsonSerializer();
            var expectedJToken = expected != null ? JToken.FromObject(expected, jsonSerializer) : null;
            var actualJToken = actual != null ? JToken.FromObject(actual, jsonSerializer) : null;

            var results = JsonDiff.EnumerateDifferences(expectedJToken, actualJToken, options);
            return results;
        }
    }
}
