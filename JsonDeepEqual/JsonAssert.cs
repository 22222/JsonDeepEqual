using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Two.JsonDeepEqual.Exceptions;

namespace Two.JsonDeepEqual
{
    /// <summary>
    /// Verifies whether two JSON documents are equal.
    /// </summary>
    public static class JsonAssert
    {
        /// <summary>
        /// Verifies that two JSON strings are equal.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The value to be compared against.</param>
        /// <exception cref="JsonEqualException">Thrown when the JSON strings are not equal.</exception>
        public static void Equal(string? expected, string? actual)
            => Equal(expected, actual, options: null);

        /// <summary>
        /// Verifies that two JSON strings are equal.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The value to be compared against.</param>
        /// <param name="options">Options that affect the comparison, or null to use default options.</param>
        /// <exception cref="JsonEqualException">Thrown when the JSON strings are not equal.</exception>
        public static void Equal(string? expected, string? actual, JsonDiffOptions? options)
        {
            const int maxDifferenceCount = 20;
            var differences = JsonDiff.EnumerateDifferences(expected, actual, options).Take(maxDifferenceCount + 1).ToList();
            if (differences.Any())
            {
                var differenceCount = differences.Count;
                var differenceCountString = differenceCount > maxDifferenceCount ? $"{maxDifferenceCount}+" : differenceCount.ToString(CultureInfo.InvariantCulture);
                throw new JsonEqualException(differences, $"JsonAssert.Equal() Failure: {differenceCountString} difference{(differences.Count != 1 ? "s" : string.Empty)}");
            }
        }

        /// <summary>
        /// Verifies that two <see cref="JToken"/> values are equal.
        /// </summary>
        /// <param name="expected">The expected <see cref="JToken"/>.</param>
        /// <param name="actual">The <see cref="JToken"/> to be compared against.</param>
        /// <exception cref="JsonEqualException">Thrown when the <see cref="JToken"/> values are not equal.</exception>
        public static void Equal(JToken? expected, JToken? actual)
            => Equal(expected, actual, options: null);

        /// <summary>
        /// Verifies that two <see cref="JToken"/> values are equal.
        /// </summary>
        /// <param name="expected">The expected <see cref="JToken"/>.</param>
        /// <param name="actual">The <see cref="JToken"/> to be compared against.</param>
        /// <param name="options">Options that affect the comparison, or null to use default options.</param>
        /// <exception cref="JsonEqualException">Thrown when the <see cref="JToken"/> values are not equal.</exception>
        public static void Equal(JToken? expected, JToken? actual, JsonDiffOptions? options)
        {
            const int maxDifferenceCount = 20;
            var differences = JsonDiff.EnumerateDifferences(expected, actual, options).Take(maxDifferenceCount + 1).ToList();
            if (differences.Any())
            {
                var differenceCountString = differences.Count > maxDifferenceCount ? $"{maxDifferenceCount}+" : maxDifferenceCount.ToString(CultureInfo.InvariantCulture);
                throw new JsonEqualException(differences, $"JsonAssert.Equal() Failure: {differenceCountString} difference{(differences.Count != 1 ? "s" : string.Empty)}");
            }
        }

        /// <summary>
        /// Verifies that two JSON strings are not equal.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The value to be compared against.</param>
        /// <exception cref="JsonNotEqualException">Thrown when the JSON strings are equal.</exception>
        public static void NotEqual(string? expected, string? actual)
            => NotEqual(expected, actual, options: null);

        /// <summary>
        /// Verifies that two JSON strings are not equal.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The value to be compared against.</param>
        /// <param name="options">Options that affect the comparison, or null to use default options.</param>
        /// <exception cref="JsonNotEqualException">Thrown when the JSON strings are equal.</exception>
        public static void NotEqual(string? expected, string? actual, JsonDiffOptions? options)
        {
            var differences = JsonDiff.EnumerateDifferences(expected, actual, options);
            if (!differences.Any())
            {
                throw new JsonNotEqualException();
            }
        }

        /// <summary>
        /// Verifies that two <see cref="JToken"/> values are not equal.
        /// </summary>
        /// <param name="expected">The expected <see cref="JToken"/>.</param>
        /// <param name="actual">The <see cref="JToken"/> to be compared against.</param>
        /// <exception cref="JsonNotEqualException">Thrown when the <see cref="JToken"/> values are equal.</exception>
        public static void NotEqual(JToken? expected, JToken? actual)
            => NotEqual(expected, actual, options: null);

        /// <summary>
        /// Verifies that two <see cref="JToken"/> values are not equal.
        /// </summary>
        /// <param name="expected">The expected <see cref="JToken"/>.</param>
        /// <param name="actual">The <see cref="JToken"/> to be compared against.</param>
        /// <param name="options">Options that affect the comparison, or null to use default options.</param>
        /// <exception cref="JsonNotEqualException">Thrown when the <see cref="JToken"/> values are equal.</exception>
        public static void NotEqual(JToken? expected, JToken? actual, JsonDiffOptions? options)
        {
            var differences = JsonDiff.EnumerateDifferences(expected, actual, options);
            if (!differences.Any())
            {
                throw new JsonNotEqualException();
            }
        }

        #region Aliases

        /// <summary>
        /// An alias of <see cref="Equal(string, string)"/> for consistency with NUnit's Assert.AreEqual method.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The value to be compared against.</param>
        /// <exception cref="JsonEqualException">Thrown when the JSON strings are not equal.</exception>
        public static void AreEqual(string? expected, string? actual)
            => Equal(expected, actual, options: null);

        /// <summary>
        /// An alias of <see cref="Equal(string, string, JsonDiffOptions)"/> for consistency with NUnit's Assert.AreEqual method.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The value to be compared against.</param>
        /// <param name="options">Options that affect the comparison, or null to use default options.</param>
        /// <exception cref="JsonEqualException">Thrown when the JSON strings are not equal.</exception>
        public static void AreEqual(string? expected, string? actual, JsonDiffOptions? options)
            => Equal(expected, actual, options);

        /// <summary>
        /// An alias of <see cref="Equal(string, string)"/> for consistency with NUnit's Assert.AreEqual method.
        /// </summary>
        /// <param name="expected">The expected <see cref="JToken"/>.</param>
        /// <param name="actual">The <see cref="JToken"/> to be compared against.</param>
        /// <exception cref="JsonEqualException">Thrown when the <see cref="JToken"/> values are not equal.</exception>
        public static void AreEqual(JToken? expected, JToken? actual)
            => Equal(expected, actual, options: null);

        /// <summary>
        /// An alias of <see cref="Equal(string, string, JsonDiffOptions)"/> for consistency with NUnit's Assert.AreEqual method.
        /// </summary>
        /// <param name="expected">The expected <see cref="JToken"/>.</param>
        /// <param name="actual">The <see cref="JToken"/> to be compared against.</param>
        /// <param name="options">Options that affect the comparison, or null to use default options.</param>
        /// <exception cref="JsonEqualException">Thrown when the <see cref="JToken"/> values are not equal.</exception>
        public static void AreEqual(JToken? expected, JToken? actual, JsonDiffOptions? options)
            => Equal(expected, actual, options);

        /// <summary>
        /// An alias of <see cref="AreNotEqual(string, string)"/> for consistency with NUnit's Assert.AreNotEqual method.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The value to be compared against.</param>
        /// <exception cref="JsonNotEqualException">Thrown when the JSON strings are equal.</exception>
        public static void AreNotEqual(string? expected, string? actual)
            => NotEqual(expected, actual, options: null);

        /// <summary>
        /// An alias of <see cref="AreNotEqual(string, string, JsonDiffOptions)"/> for consistency with NUnit's Assert.AreNotEqual method.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The value to be compared against.</param>
        /// <param name="options">Options that affect the comparison, or null to use default options.</param>
        /// <exception cref="JsonNotEqualException">Thrown when the JSON strings are equal.</exception>
        public static void AreNotEqual(string? expected, string? actual, JsonDiffOptions? options)
            => NotEqual(expected, actual, options);

        #endregion

        #region Not Supported
#pragma warning disable SA1611 // Element parameters should be documented
#pragma warning disable SA1615 // Element return value should be documented

        /// <summary>
        /// Do not call this method.
        /// </summary>
        [Obsolete("This is an override of Object.Equals(). Call JsonAssert.Equal() instead.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static new bool Equals(object a, object b)
        {
            throw new NotSupportedException("JsonAssert.Equals should not be used");
        }

        /// <summary>
        /// Do not call this method.
        /// </summary>
        [Obsolete("This is an override of Object.ReferenceEquals(). Call JsonAssert.Equal() instead.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static new bool ReferenceEquals(object a, object b)
        {
            throw new NotSupportedException("JsonAssert.ReferenceEquals should not be used");
        }

#pragma warning restore SA1615 // Element return value should be documented
#pragma warning restore SA1611 // Element parameters should be documented
        #endregion
    }
}
