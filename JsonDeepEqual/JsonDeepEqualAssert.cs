using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Two.JsonDeepEqual.Exceptions;

namespace Two.JsonDeepEqual
{
    /// <summary>
    /// Verifies whether two objects are equal based on their JSON representation.
    /// </summary>
    public static class JsonDeepEqualAssert
    {
        /// <summary>
        /// Verifies that two objects are equal, using a JSON serialization comparer.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The value to be compared against.</param>
        /// <exception cref="JsonEqualException">Thrown when the objects are not equal.</exception>
        public static void Equal(object? expected, object? actual)
            => Equal(expected, actual, options: null);

        /// <summary>
        /// Verifies that two objects are equal, using a JSON serialization comparer.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The value to be compared against.</param>
        /// <param name="options">Options that control the comparison, or null to use default options.</param>
        /// <exception cref="JsonEqualException">Thrown when the objects are not equal.</exception>
        public static void Equal(object? expected, object? actual, JsonDeepEqualDiffOptions? options)
        {
            const int maxDifferenceCount = 20;
            var differences = JsonDeepEqualDiff.EnumerateDifferences(expected, actual, options).Take(maxDifferenceCount + 1).ToList();
            if (differences.Any())
            {
                var differenceCount = differences.Count;
                var differenceCountString = differenceCount > maxDifferenceCount ? $"{maxDifferenceCount}+" : differenceCount.ToString(CultureInfo.InvariantCulture);
                throw new JsonEqualException(differences, $"JsonDeepEqualAssert.Equal() Failure: {differenceCountString} difference{(differences.Count != 1 ? "s" : string.Empty)}");
            }
        }

        /// <summary>
        /// Verifies that two objects are not equal, using a JSON serialization comparer.
        /// </summary>
        /// <param name="expected">The expected object.</param>
        /// <param name="actual">The actual object.</param>
        /// <exception cref="JsonNotEqualException">Thrown when the objects are equal.</exception>
        public static void NotEqual(object? expected, object? actual)
            => NotEqual(expected, actual, options: null);

        /// <summary>
        /// Verifies that two objects are not equal, using a JSON serialization comparer.
        /// </summary>
        /// <param name="expected">The expected object.</param>
        /// <param name="actual">The actual object.</param>
        /// <param name="options">Options that control the comparison, or null to use default options.</param>
        /// <exception cref="JsonNotEqualException">Thrown when the objects are equal.</exception>
        public static void NotEqual(object? expected, object? actual, JsonDeepEqualDiffOptions? options)
        {
            var differences = JsonDeepEqualDiff.EnumerateDifferences(expected, actual, options);
            if (!differences.Any())
            {
                throw new JsonNotEqualException();
            }
        }

        #region Aliases

        /// <summary>
        /// An alias of <see cref="Equal(object, object)"/> for consistency with NUnit's Assert.AreEqual method.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The value to be compared against.</param>
        /// <exception cref="JsonEqualException">Thrown when the objects are not equal.</exception>
        public static void AreEqual(object? expected, object? actual)
            => Equal(expected, actual, options: null);

        /// <summary>
        /// An alias of <see cref="Equal(object, object, JsonDeepEqualDiffOptions)"/> for consistency with NUnit's Assert.AreEqual method.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The value to be compared against.</param>
        /// <param name="options">Options that control the comparison, or null to use default options.</param>
        /// <exception cref="JsonEqualException">Thrown when the objects are not equal.</exception>
        public static void AreEqual(object? expected, object? actual, JsonDeepEqualDiffOptions? options)
            => Equal(expected, actual, options);

        /// <summary>
        /// An alias of <see cref="NotEqual(object, object)"/> for consistency with NUnit's Assert.AreNotEqual method.
        /// </summary>
        /// <param name="expected">The expected object.</param>
        /// <param name="actual">The actual object.</param>
        /// <exception cref="JsonNotEqualException">Thrown when the objects are equal.</exception>
        public static void AreNotEqual(object? expected, object? actual)
            => NotEqual(expected, actual, options: null);

        /// <summary>
        /// An alias of <see cref="NotEqual(object, object, JsonDeepEqualDiffOptions)"/> for consistency with NUnit's Assert.AreNotEqual method.
        /// </summary>
        /// <param name="expected">The expected object.</param>
        /// <param name="actual">The actual object.</param>
        /// <param name="options">Options that control the comparison, or null to use default options.</param>
        /// <exception cref="JsonNotEqualException">Thrown when the objects are equal.</exception>
        public static void AreNotEqual(object? expected, object? actual, JsonDeepEqualDiffOptions? options)
            => NotEqual(expected, actual, options);

        #endregion

        #region Not Supported
#pragma warning disable SA1611 // Element parameters should be documented
#pragma warning disable SA1615 // Element return value should be documented

        /// <summary>
        /// Do not call this method.
        /// </summary>
        [Obsolete("This is an override of Object.Equals(). Call JsonDeepEqualAssert.Equal() instead.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static new bool Equals(object a, object b)
        {
            throw new NotSupportedException("JsonDeepEqualAssert.Equals should not be used");
        }

        /// <summary>
        /// Do not call this method.
        /// </summary>
        [Obsolete("This is an override of Object.ReferenceEquals(). Call JsonDeepEqualAssert.Equal() instead.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static new bool ReferenceEquals(object a, object b)
        {
            throw new NotSupportedException("JsonDeepEqualAssert.ReferenceEquals should not be used");
        }

#pragma warning restore SA1615 // Element return value should be documented
#pragma warning restore SA1611 // Element parameters should be documented
        #endregion
    }
}
