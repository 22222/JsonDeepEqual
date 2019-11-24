using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text;

namespace Two.JsonDeepEqual
{
    /// <summary>
    /// A difference at a path between two JSON documents.
    /// </summary>
    public class JsonDiffNode
    {
        /// <summary>
        /// Constructs a <see cref="JsonDiffNode"/>.
        /// </summary>
        /// <param name="path">The path to this difference in the JSON document as a [JSON pointer](https://tools.ietf.org/html/rfc6901).</param>
        /// <param name="expectedValue">The expected value at the <paramref name="path"/>.</param>
        /// <param name="actualValue">The actual value at the at the <paramref name="path"/>.</param>
        public JsonDiffNode(string path, JToken? expectedValue, JToken? actualValue)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            ExpectedValue = expectedValue ?? JValue.CreateNull();
            ActualValue = actualValue ?? JValue.CreateNull();
        }

        /// <summary>
        /// The path to this difference in the JSON document as a [JSON pointer](https://tools.ietf.org/html/rfc6901).
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// The <see cref="JToken"/> value from the expected document at the <see cref="Path"/>.
        /// </summary>
        public JToken ExpectedValue { get; }

        /// <summary>
        /// The <see cref="JToken"/> value from the actual at the <see cref="Path"/>.
        /// </summary>
        public JToken ActualValue { get; }

        #region Computed

        /// <summary>
        /// The character index of the first difference in the serialized values, or null if the diff index is not known or applicable.
        /// </summary>
        public virtual int? DiffIndex => GetOrCreateJsonDifferenceDisplay().DiffIndex;

        /// <summary>
        /// The display value for <see cref="ExpectedValue"/>.
        /// This may be truncated for values with a long string representation.
        /// </summary>
        public virtual string ExpectedValueDisplay => GetOrCreateJsonDifferenceDisplay().ExpectedValueDisplay;

        /// <summary>
        /// The character index in <see cref="ExpectedValueDisplay"/> of the first difference, or null if there is no known difference index.
        /// This may differ from <see cref="DiffIndex"/> if the <see cref="ExpectedValueDisplay"/> is truncated.
        /// </summary>
        public virtual int? ExpectedValueDisplayDiffIndex => GetOrCreateJsonDifferenceDisplay().ExpectedValueDisplayDiffIndex;

        /// <summary>
        /// The display value for <see cref="ActualValue"/>.
        /// This may be truncated for values with a long string representation.
        /// </summary>
        public virtual string ActualValueDisplay => GetOrCreateJsonDifferenceDisplay().ActualValueDisplay;

        /// <summary>
        /// The character index in <see cref="ActualValueDisplay"/> of the first difference, or null if there is no known difference index.
        /// This may differ from <see cref="DiffIndex"/> if the <see cref="ActualValueDisplay"/> is truncated.
        /// </summary>
        public virtual int? ActualValueDisplayDiffIndex => GetOrCreateJsonDifferenceDisplay().ActualValueDisplayDiffIndex;

        /// <summary>
        /// Returns a message that describes this difference.
        /// </summary>
        /// <returns>The full description of this difference.</returns>
        public override string ToString() => GetOrCreateJsonDifferenceDisplay().ToStringValue;

        private JsonDiffNodeDisplay GetOrCreateJsonDifferenceDisplay()
        {
            if (jsonDiffNodeDisplayOrNull != null)
            {
                return jsonDiffNodeDisplayOrNull;
            }

            string expectedValue = ExpectedValue.ToString(Formatting.None);
            string actualValue = ActualValue.ToString(Formatting.None);
            var diffIndex = FindDiffIndex(expectedValue, actualValue);
            var (expectedValueDisplay, expectedValueDisplayDiffIndex) = GenerateDisplayValue(expectedValue, diffIndex);
            var (actualValueDisplay, actualValueDisplayDiffIndex) = GenerateDisplayValue(actualValue, diffIndex);

            bool showDiffIndexPointers = diffIndex.HasValue
                && expectedValueDisplayDiffIndex.HasValue
                && actualValueDisplayDiffIndex.HasValue
                && ExpectedValue != null
                && ActualValue != null
                && ExpectedValue.Type == ActualValue.Type
                && new[] { JTokenType.Object, JTokenType.Array, JTokenType.String, JTokenType.Bytes, JTokenType.Raw }.Contains(ExpectedValue.Type);

            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(Path))
            {
                sb.Append(Path).Append(":");
                sb.AppendLine();
            }
            if (showDiffIndexPointers && expectedValueDisplayDiffIndex.HasValue)
            {
                sb.Append("              ").Append(' ', expectedValueDisplayDiffIndex.Value).Append('↓').Append($" (pos {diffIndex})");
                sb.AppendLine();
            }
            sb.Append("    Expected: ").Append(expectedValueDisplay);
            sb.AppendLine();
            sb.Append("    Actual:   ").Append(actualValueDisplay);
            if (showDiffIndexPointers && actualValueDisplayDiffIndex.HasValue)
            {
                sb.AppendLine();
                sb.Append("              ").Append(' ', actualValueDisplayDiffIndex.Value).Append('↑').Append($" (pos {diffIndex})");
            }
            var toStringValue = sb.ToString();

            jsonDiffNodeDisplayOrNull = new JsonDiffNodeDisplay(
                expectedValueDisplay: expectedValueDisplay,
                expectedValueDisplayDiffIndex: expectedValueDisplayDiffIndex,
                actualValueDisplay: actualValueDisplay,
                actualValueDisplayDiffIndex: actualValueDisplayDiffIndex,
                diffIndex: diffIndex,
                toStringValue: toStringValue
            );
            return jsonDiffNodeDisplayOrNull;
        }

        private JsonDiffNodeDisplay? jsonDiffNodeDisplayOrNull;

        private class JsonDiffNodeDisplay
        {
            public JsonDiffNodeDisplay(string expectedValueDisplay, int? expectedValueDisplayDiffIndex, string actualValueDisplay, int? actualValueDisplayDiffIndex, int? diffIndex, string toStringValue)
            {
                ExpectedValueDisplay = expectedValueDisplay ?? throw new ArgumentNullException(nameof(expectedValueDisplay));
                ExpectedValueDisplayDiffIndex = expectedValueDisplayDiffIndex;
                ActualValueDisplay = actualValueDisplay ?? throw new ArgumentNullException(nameof(actualValueDisplay));
                ActualValueDisplayDiffIndex = actualValueDisplayDiffIndex;
                DiffIndex = diffIndex;
                ToStringValue = toStringValue ?? throw new ArgumentNullException(nameof(toStringValue));
            }

            public string ExpectedValueDisplay { get; }

            public int? ExpectedValueDisplayDiffIndex { get; }

            public string ActualValueDisplay { get; }

            public int? ActualValueDisplayDiffIndex { get; }

            public int? DiffIndex { get; }

            public string ToStringValue { get; }
        }

        private static int? FindDiffIndex(string expected, string actual)
        {
            int? differenceIndex = null;

            var expectedLength = expected.Length;
            var actualLength = actual.Length;
            var minLength = Math.Min(expectedLength, actualLength);
            for (var i = 0; i < minLength; i++)
            {
                if (expected[i] != actual[i])
                {
                    differenceIndex = i;
                    break;
                }
            }
            if (!differenceIndex.HasValue && expectedLength != actualLength)
            {
                differenceIndex = minLength;
            }
            return differenceIndex;
        }

        private static Tuple<string, int?> GenerateDisplayValue(string value, int? diffIndexOrNull)
        {
            const int beforeDiffLength = 20;
            const int afterDiffLength = 40;
            const int maxLength = beforeDiffLength + afterDiffLength + 1;
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            {
                return Tuple.Create(value ?? string.Empty, diffIndexOrNull);
            }

            string valueDisplay;
            int? displayDiffIndex;
            if (!diffIndexOrNull.HasValue)
            {
                valueDisplay = value.Substring(0, maxLength) + "…";
                displayDiffIndex = null;
            }
            else
            {
                int diffIndex = diffIndexOrNull.Value;
                int startIndex = Math.Max(diffIndex - beforeDiffLength, 0);
                int endIndex = Math.Min(diffIndex + afterDiffLength + 1, value.Length);
                valueDisplay = value.Substring(startIndex, endIndex - startIndex);

                int displayDiffIndexValue = diffIndex;
                if (startIndex > 0)
                {
                    displayDiffIndexValue = diffIndex + 1 - startIndex;
                    valueDisplay = "…" + valueDisplay;
                }
                if (endIndex < value.Length)
                {
                    valueDisplay += "…";
                }
                displayDiffIndex = displayDiffIndexValue;
            }
            return Tuple.Create(valueDisplay, displayDiffIndex);
        }

        #endregion
    }
}
