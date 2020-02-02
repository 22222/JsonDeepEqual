using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Two.JsonDeepEqual.Utilities;

namespace Two.JsonDeepEqual
{
    /// <summary>
    /// Options that control how two JSON values are compared.
    /// </summary>
    public class JsonDiffOptions
    {
        /// <summary>
        /// Paths to exclude from the comparison, in [JSON pointer notation](https://tools.ietf.org/html/rfc6901) with support for glob-style wildcards (* and **).
        /// </summary>
        public IReadOnlyCollection<string>? ExcludePropertyPaths { get; set; }

        /// <summary>
        /// A custom filter that chooses the paths to include in the comparison.
        /// This is a more advanced alternative to the <see cref="ExcludePropertyPaths"/> property.
        /// </summary>
        public Func<IEnumerable<string>, IEnumerable<string>>? PropertyPathFilter { get; set; }

        /// <summary>
        /// Creates a <see cref="IJsonPropertyPathFilter"/> for these options, or returns null if no filter is needed.
        /// </summary>
        internal Func<IEnumerable<string>, IEnumerable<string>>? ToJsonPropertyPathFilterOrNull()
        {
            var options = this;
            Func<IEnumerable<string>, IEnumerable<string>>? excludePropertyPathFilter = null;
            if (options.ExcludePropertyPaths != null && options.ExcludePropertyPaths.Any())
            {
                excludePropertyPathFilter = new JsonPropertyPathFilter(options.ExcludePropertyPaths).Apply;
            }

            Func<IEnumerable<string>, IEnumerable<string>>? customFilter = options.PropertyPathFilter;
            if (excludePropertyPathFilter != null && customFilter != null)
            {
                return new AggregateJsonPropertyPathFilter(excludePropertyPathFilter, customFilter).Apply;
            }
            return excludePropertyPathFilter ?? customFilter;
        }

        /// <summary>
        /// If true, two arrays will be considered equal if they contain the same elements in any order.
        /// </summary>
        public bool IgnoreArrayElementOrder { get; set; }

        /// <summary>
        /// If true, an empty array will be considered equal to a missing or null value.
        /// </summary>
        public bool IgnoreEmptyArrays { get; set; }

        /// <summary>
        /// If true, an empty object will be considered equal to a missing or null value.
        /// </summary>
        public bool IgnoreEmptyObjects { get; set; }

        /// <summary>
        /// If true, ignores case differences in all string values and property names.
        /// </summary>
        public bool IgnoreCase { get; set; }

        /// <summary>
        /// If true, treats treats \r\n, \r, and \n as equivalent in all string values.
        /// </summary>
        public bool IgnoreLineEndingDifferences { get; set; }

        /// <summary>
        /// If true, treats spaces, tabs, and other whitespace in any non-zero quantity as equivalent.
        /// </summary>
        public bool IgnoreWhiteSpaceDifferences { get; set; }
    }

    /// <summary>
    /// A filter on JSON property paths.
    /// </summary>
    internal interface IJsonPropertyPathFilter
    {
        /// <summary>
        /// Filters the given property paths, which are in [JSON pointer notation](https://tools.ietf.org/html/rfc6901).
        /// </summary>
        /// <param name="propertyPaths">The property paths to filter.</param>
        /// <returns>The property paths that should be included in a comparison.</returns>
        IEnumerable<string> Apply(IEnumerable<string> propertyPaths);
    }

    /// <summary>
    /// A <see cref="IJsonPropertyPathFilter"/> for the <see cref="JsonDiffOptions.ExcludePropertyPaths"/> property.
    /// </summary>
    internal sealed class JsonPropertyPathFilter : IJsonPropertyPathFilter
    {
        private readonly IReadOnlyCollection<string> excludePaths;
        private readonly IReadOnlyCollection<Regex> excludePathRegexes;

        public JsonPropertyPathFilter(IReadOnlyCollection<string> excludePropertyPaths)
        {
            this.excludePaths = excludePropertyPaths
                .Where(path => !GlobConvert.IsGlobPattern(path))
                .Where(path => !string.IsNullOrEmpty(path))
                .ToArray();
            this.excludePathRegexes = excludePropertyPaths
                .Where(GlobConvert.IsGlobPattern)
                .Select(globPattern => GlobConvert.CreatePathRegexOrNull(globPattern, ignoreCase: true))
                .Where(regex => regex != null).Cast<Regex>()
                .ToArray();
        }

        /// <inheritdoc />
        public IEnumerable<string> Apply(IEnumerable<string> propertyPaths)
        {
            var result = propertyPaths;
            foreach (var excludePath in excludePaths)
            {
                result = result.Where(propertyPath => !string.Equals(propertyPath, excludePath, StringComparison.OrdinalIgnoreCase));
            }
            foreach (var excludePathRegex in excludePathRegexes)
            {
                result = result.Where(propertyPath => !excludePathRegex.IsMatch(propertyPath));
            }
            return result;
        }
    }

    /// <summary>
    /// A <see cref="IJsonPropertyPathFilter"/> that applies two or more filters.
    /// </summary>
    internal sealed class AggregateJsonPropertyPathFilter : IJsonPropertyPathFilter
    {
        public AggregateJsonPropertyPathFilter(params Func<IEnumerable<string>, IEnumerable<string>>[] filters)
            : this((IReadOnlyCollection<Func<IEnumerable<string>, IEnumerable<string>>>)filters) { }

        public AggregateJsonPropertyPathFilter(IReadOnlyCollection<Func<IEnumerable<string>, IEnumerable<string>>> filters)
        {
            this.InnerFilters = filters ?? throw new ArgumentNullException(nameof(filters));
            if (!this.InnerFilters.Any())
            {
                throw new ArgumentException("Must have at least one filter");
            }
        }

        /// <summary>
        /// The filters that are applied by this aggregate filter.
        /// </summary>
        public IReadOnlyCollection<Func<IEnumerable<string>, IEnumerable<string>>> InnerFilters { get; }

        /// <inheritdoc />
        public IEnumerable<string> Apply(IEnumerable<string> properties)
        {
            var result = properties;
            foreach (var filter in InnerFilters)
            {
                result = filter(result);
            }
            return result;
        }
    }
}
