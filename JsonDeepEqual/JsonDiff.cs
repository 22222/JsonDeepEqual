using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Two.JsonDeepEqual
{
    /// <summary>
    /// Finds the differences between two JSON documents.
    /// </summary>
    public static class JsonDiff
    {
        /// <summary>
        /// Finds the differences between two JSON strings.
        /// </summary>
        /// <param name="expectedJson">The expected JSON string.</param>
        /// <param name="actualJson">The JSON string to be compared against.</param>
        /// <returns>The differences, or an empty enumerable if the two JSON strings are equal.</returns>
        public static IEnumerable<JsonDiffNode> EnumerateDifferences(string? expectedJson, string? actualJson)
            => EnumerateDifferences(expectedJson, actualJson, options: null);

        /// <summary>
        /// Finds the differences between two JSON strings.
        /// </summary>
        /// <param name="expectedJson">The expected JSON string.</param>
        /// <param name="actualJson">The JSON string to be compared against.</param>
        /// <param name="options">Options that control the comparison, or null to use default options.</param>
        /// <returns>The differences, or an empty enumerable if the two JSON strings are equal.</returns>
        public static IEnumerable<JsonDiffNode> EnumerateDifferences(string? expectedJson, string? actualJson, JsonDiffOptions? options)
            => new JsonDiffer(options).EnumerateJsonDifferences(expectedJson, actualJson);

        /// <summary>
        /// Finds the differences between two <see cref="JToken"/> values.
        /// </summary>
        /// <param name="expectedJToken">The expected <see cref="JToken"/>.</param>
        /// <param name="actualJToken">The <see cref="JToken"/> to be compared against.</param>
        /// <returns>The differences, or an empty enumerable if the <see cref="JToken"/> values are equal.</returns>
        public static IEnumerable<JsonDiffNode> EnumerateDifferences(JToken? expectedJToken, JToken? actualJToken)
            => EnumerateDifferences(expectedJToken, actualJToken, options: null);

        /// <summary>
        /// Finds the differences between two <see cref="JToken"/> values.
        /// </summary>
        /// <param name="expectedJToken">The expected <see cref="JToken"/>.</param>
        /// <param name="actualJToken">The <see cref="JToken"/> to be compared against.</param>
        /// <param name="options">Options that control the comparison, or null to use default options.</param>
        /// <returns>The differences, or an empty enumerable if the <see cref="JToken"/> values are equal.</returns>
        public static IEnumerable<JsonDiffNode> EnumerateDifferences(JToken? expectedJToken, JToken? actualJToken, JsonDiffOptions? options)
            => new JsonDiffer(options).EnumerateJTokenDifferences(expectedJToken, actualJToken);

        private sealed class JsonDiffer
        {
            private readonly JsonDiffOptions options;
            private readonly Func<IEnumerable<string>, IEnumerable<string>>? jsonPropertyPathFilterOrNull;

            public JsonDiffer(JsonDiffOptions? options)
            {
                this.options = options ?? new JsonDiffOptions();
                this.jsonPropertyPathFilterOrNull = options?.ToJsonPropertyPathFilterOrNull();
            }

            public IEnumerable<JsonDiffNode> EnumerateJsonDifferences(string? expectedJson, string? actualJson)
            {
                if (expectedJson != null && actualJson != null)
                {
                    if (options.IgnoreLineEndingDifferences)
                    {
                        expectedJson = Regex.Replace(expectedJson, "\r\n?", "\n");
                        actualJson = Regex.Replace(expectedJson, "\r\n?", "\n");
                    }
                    if (options.IgnoreWhiteSpaceDifferences)
                    {
                        expectedJson = Regex.Replace(expectedJson, @"\s+", " ");
                        actualJson = Regex.Replace(expectedJson, @"\s+", " ");
                    }
                    if (options.IgnoreCase)
                    {
#pragma warning disable CA1308 // Normalize strings to uppercase
                        expectedJson = expectedJson.ToLowerInvariant();
                        actualJson = actualJson.ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase
                    }
                }

                var expectedJToken = expectedJson != null ? JToken.Parse(expectedJson) : null;
                var actualJToken = actualJson != null ? JToken.Parse(actualJson) : null;
                return EnumerateJTokenDifferencesRecursive(expectedJToken, actualJToken, path: string.Empty);
            }

            public IEnumerable<JsonDiffNode> EnumerateJTokenDifferences(JToken? expectedJToken, JToken? actualJToken)
            {
                bool isStringManipulationOption = options.IgnoreLineEndingDifferences || options.IgnoreWhiteSpaceDifferences || options.IgnoreCase;
                if (isStringManipulationOption && expectedJToken != null && actualJToken != null)
                {
                    var expectedJson = expectedJToken.ToString(Newtonsoft.Json.Formatting.None);
                    var actualJson = actualJToken.ToString(Newtonsoft.Json.Formatting.None);
                    return EnumerateJsonDifferences(expectedJson, actualJson);
                }

                return EnumerateJTokenDifferencesRecursive(expectedJToken, actualJToken, path: string.Empty);
            }

            private IEnumerable<JsonDiffNode> EnumerateJTokenDifferencesRecursive(JToken? expected, JToken? actual, string path)
            {
                if (expected == null)
                {
                    expected = JValue.CreateNull();
                }
                if (actual == null)
                {
                    actual = JValue.CreateNull();
                }

                IEnumerable<JsonDiffNode> results;
                if (expected.Type == JTokenType.Object && actual.Type == JTokenType.Object)
                {
                    results = EnumerateJObjectDifferencesRecursiveUnfiltered((JObject)expected, (JObject)actual, path);
                }
                else if (expected.Type == JTokenType.Array && actual.Type == JTokenType.Array)
                {
                    results = EnumerateJArrayDifferencesRecursiveUnfiltered((JArray)expected, (JArray)actual, path);
                }
                else if (JToken.DeepEquals(expected, actual))
                {
                    results = Enumerable.Empty<JsonDiffNode>();
                }
                else
                {
                    var valueDifference = new JsonDiffNode(path, expected, actual);
                    results = new[] { valueDifference };
                }

                if (jsonPropertyPathFilterOrNull != null)
                {
                    var filteredPaths = new HashSet<string>(jsonPropertyPathFilterOrNull(results.Select(x => x.Path)));
                    results = results.Where(result => filteredPaths.Contains(result.Path));
                }

                return results;
            }

            private IEnumerable<JsonDiffNode> EnumerateJObjectDifferencesRecursiveUnfiltered(JObject expected, JObject actual, string path)
            {
                var expectedProperties = expected.Properties();
                int propertyMatchCount = 0;
                foreach (var expectedProperty in expectedProperties)
                {
                    JToken expectedValue = expectedProperty.Value;
                    JToken? actualValue;

                    var actualPropertyOrNull = actual.Property(expectedProperty.Name, StringComparison.Ordinal);
                    if (actualPropertyOrNull != null)
                    {
                        actualValue = actualPropertyOrNull.Value;
                        propertyMatchCount++;
                    }
                    else
                    {
                        actualValue = null;
                    }

                    var differences = EnumerateJTokenDifferencesRecursive(expectedValue, actualValue, path + "/" + expectedProperty.Name);
                    foreach (var difference in differences)
                    {
                        yield return difference;
                    }
                }

                var actualProperties = actual.Properties();
                if (propertyMatchCount != actualProperties.Count())
                {
                    foreach (var actualProperty in actualProperties)
                    {
                        var expectedPropertyOrNull = expected.Property(actualProperty.Name, StringComparison.Ordinal);
                        if (expectedPropertyOrNull != null)
                        {
                            continue;
                        }

                        JToken actualValue = actualProperty.Value;
                        JToken? expectedValue = null;

                        var differences = EnumerateJTokenDifferencesRecursive(expectedValue, actualValue, path + "/" + actualProperty.Name);
                        foreach (var difference in differences)
                        {
                            yield return difference;
                        }
                    }
                }
            }

            private IEnumerable<JsonDiffNode> EnumerateJArrayDifferencesRecursiveUnfiltered(JArray expected, JArray actual, string path)
            {
                var expectedCount = expected.Count;
                var actualCount = actual.Count;

                if (expectedCount == 0 && actualCount == 0)
                {
                    yield break;
                }

                bool ignoreArrayElementOrder = options.IgnoreArrayElementOrder;
                if (!ignoreArrayElementOrder)
                {
                    var minCount = Math.Min(expectedCount, actualCount);
                    for (var i = 0; i < minCount; i++)
                    {
                        var expectedElement = expected[i];
                        var actualElement = actual[i];
                        var elementDifferences = EnumerateJTokenDifferencesRecursive(expectedElement, actualElement, path + "/" + i);
                        foreach (var elementDifference in elementDifferences)
                        {
                            yield return elementDifference;
                        }
                    }
                    if (expectedCount > minCount)
                    {
                        for (var i = minCount; i < expectedCount; i++)
                        {
                            var expectedElement = expected[i];
                            var elementDifferences = EnumerateJTokenDifferencesRecursive(expectedElement, null, path + "/" + i);
                            foreach (var elementDifference in elementDifferences)
                            {
                                yield return elementDifference;
                            }
                        }
                    }
                    if (actualCount > minCount)
                    {
                        for (var i = minCount; i < actualCount; i++)
                        {
                            var actualElement = actual[i];
                            var elementDifferences = EnumerateJTokenDifferencesRecursive(null, actualElement, path + "/" + i);
                            foreach (var elementDifference in elementDifferences)
                            {
                                yield return elementDifference;
                            }
                        }
                    }
                }
                else
                {
                    var unmatchedExpected = expected.ToList();
                    var unmatchedActual = actual.ToList();
                    foreach (var expectedElement in expected)
                    {
                        JToken? matchedActualElement = null;
                        foreach (var actualElement in unmatchedActual)
                        {
                            var differences = EnumerateJTokenDifferencesRecursive(expectedElement, actualElement, path + "/*");
                            if (!differences.Any())
                            {
                                matchedActualElement = actualElement;
                                break;
                            }
                        }
                        if (matchedActualElement != null)
                        {
                            unmatchedExpected.Remove(expectedElement);
                            unmatchedActual.Remove(matchedActualElement);
                        }
                        if (!unmatchedActual.Any())
                        {
                            break;
                        }
                    }

                    // An unordered array difference doesn't really fit into the JSON pointer system.
                    // The "*" wildcard path thing is non-standard and kind of questionable, but we'll go with it for now.
                    if (unmatchedExpected.Any() || unmatchedActual.Any())
                    {
                        var arrayDifference = new JsonDiffNode(path + "/*", JArray.FromObject(unmatchedExpected), JArray.FromObject(unmatchedActual));
                        yield return arrayDifference;
                    }
                    if (expectedCount != actualCount)
                    {
                        var countDifference = new JsonDiffNode(path + "/length", JToken.FromObject(expectedCount), JToken.FromObject(actualCount));
                        yield return countDifference;
                    }
                }
            }
        }
    }
}
