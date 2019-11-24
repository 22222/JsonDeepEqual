using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Two.JsonDeepEqual.Utilities;

namespace Two.JsonDeepEqual
{
    /// <summary>
    /// Options that control how two objects are serialized to JSON and compared.
    /// </summary>
    public class JsonDeepEqualDiffOptions : JsonDiffOptions
    {
        /// <summary>
        /// Property names to exclude from the comparison, with support for glob-style wildcards (* and **).
        /// These properties will be ignored in all objects.
        /// </summary>
        public IReadOnlyCollection<string>? ExcludePropertyNames { get; set; }

        /// <summary>
        /// A custom filter that chooses the properties included in the comparison.
        /// This is a more advanced alternative to the <see cref="ExcludePropertyNames"/> property.
        /// </summary>
        public Func<IEnumerable<JsonProperty>, IEnumerable<JsonProperty>>? PropertyFilter { get; set; }

        /// <summary>
        /// Specifies how null values are handled during JSON serialization.
        /// The default is <see cref="Newtonsoft.Json.NullValueHandling.Ignore"/>.
        /// </summary>
        public NullValueHandling? NullValueHandling { get; set; }

        /// <summary>
        /// Specifies how default values are handled during JSON serialization.
        /// The default is <see cref="Newtonsoft.Json.DefaultValueHandling.Ignore"/>.
        /// </summary>
        public DefaultValueHandling? DefaultValueHandling { get; set; }

        /// <summary>
        /// Specifies how circular references are handled during JSON serialization.
        /// The default is <see cref="Newtonsoft.Json.ReferenceLoopHandling.Ignore"/>.
        /// </summary>
        public ReferenceLoopHandling? ReferenceLoopHandling { get; set; }

        /// <summary>
        /// Specifies how <see cref="DateTime"/> and <see cref="DateTimeOffset"/> values
        /// are formatted when writing JSON text.
        /// Default is "yyyy'-'MM'-'dd'T'HH':'mm':'ssK".
        /// </summary>
        public string? DateFormatString { get; set; }

        /// <summary>
        /// Adjusts a <see cref="DateTime"/> value before JSON serialization.
        /// This can be used to choose the precision of DateTime values (such as truncating or rounding milliseconds).
        /// </summary>
        public Func<DateTime, DateTime>? DateTimeConverter { get; set; }

        /// <summary>
        /// Sepcifies how JSON attributes (like <see cref="JsonPropertyAttribute"/> and <see cref="JsonIgnoreAttribute"/>) are handled during JSON serialization.
        /// The default is <see cref="JsonAttributeHandling.Ignore"/>.
        /// </summary>
        public JsonAttributeHandling? JsonAttributeHandling { get; set; }

        /// <summary>
        /// Any custom converters to use during JSON serialization.
        /// </summary>
        public IReadOnlyCollection<JsonConverter>? JsonConverters { get; set; }

        /// <summary>
        /// Creates a <see cref="JsonSerializer"/> for these options.
        /// </summary>
        internal JsonSerializer ToJsonSerializer()
        {
            var options = this;
            var jsonSerializer = new JsonSerializer
            {
                NullValueHandling = options.NullValueHandling ?? Newtonsoft.Json.NullValueHandling.Ignore,
                DefaultValueHandling = options.DefaultValueHandling ?? Newtonsoft.Json.DefaultValueHandling.Ignore,
                ReferenceLoopHandling = options.ReferenceLoopHandling ?? Newtonsoft.Json.ReferenceLoopHandling.Ignore,
                DateFormatString = options.DateFormatString ?? "yyyy'-'MM'-'dd'T'HH':'mm':'ssK",
            };

            if (options.JsonConverters != null)
            {
                foreach (var jsonConverter in options.JsonConverters)
                {
                    jsonSerializer.Converters.Add(jsonConverter);
                }
            }
            jsonSerializer.Converters.Add(new StringEnumConverter());
            jsonSerializer.Converters.Add(new JsonDeepEqualDateTimeConverter(options.DateTimeConverter));

            jsonSerializer.ContractResolver = new JsonDeepEqualContractResolver(options);

            return jsonSerializer;
        }

        /// <summary>
        /// Creates a <see cref="IJsonPropertyFilter"/> for these options, or returns null if no filter is needed.
        /// </summary>
        internal Func<IEnumerable<JsonProperty>, IEnumerable<JsonProperty>>? ToJsonPropertyFilterOrNull()
        {
            var options = this;

            Func<IEnumerable<JsonProperty>, IEnumerable<JsonProperty>>? excludePropertyNameFilter = null;
            if (options.ExcludePropertyNames != null && options.ExcludePropertyNames.Any())
            {
                excludePropertyNameFilter = new JsonPropertyFilter(options.ExcludePropertyNames).Apply;
            }

            Func<IEnumerable<JsonProperty>, IEnumerable<JsonProperty>>? customFilter = options.PropertyFilter;
            if (excludePropertyNameFilter != null && customFilter != null)
            {
                return new AggregateJsonPropertyFilter(excludePropertyNameFilter, customFilter).Apply;
            }
            return excludePropertyNameFilter ?? customFilter;
        }
    }

    /// <summary>
    /// Handling options for JSON attributes (like <see cref="JsonPropertyAttribute"/> and <see cref="JsonIgnoreAttribute"/>).
    /// </summary>
    public enum JsonAttributeHandling
    {
        /// <summary>
        /// True if JSON attributes should affect serialization.
        /// </summary>
        Include = 0,

        /// <summary>
        /// Ignore JSON attributes when serializing and deserializing objects.
        /// </summary>
        Ignore = 1,
    }

    /// <summary>
    /// A filter on the JSON properties to include in a comparison.
    /// </summary>
    internal interface IJsonPropertyFilter
    {
        /// <summary>
        /// Filters the given properties to only the properties that should be included in a comparison.
        /// </summary>
        /// <param name="properties">The properties to filter.</param>
        /// <returns>The properties that should be included in the comparison.</returns>
        IEnumerable<JsonProperty> Apply(IEnumerable<JsonProperty> properties);
    }

    /// <summary>
    /// A <see cref="IJsonPropertyFilter"/> for the <see cref="JsonDeepEqualDiffOptions.ExcludePropertyNames"/> property.
    /// </summary>
    internal sealed class JsonPropertyFilter : IJsonPropertyFilter
    {
        private readonly IReadOnlyCollection<string> excludeNames;
        private readonly IReadOnlyCollection<Regex> excludeNameRegexes;

        public JsonPropertyFilter(IReadOnlyCollection<string> excludeNames)
        {
            this.excludeNames = excludeNames
                .Where(path => !GlobConvert.IsGlobPattern(path))
                .Where(path => !string.IsNullOrEmpty(path))
                .ToArray();
            this.excludeNameRegexes = excludeNames
                .Where(GlobConvert.IsGlobPattern)
                .Select(globPattern => GlobConvert.CreatePathRegexOrNull(globPattern, ignoreCase: true))
                .Where(regex => regex != null).Cast<Regex>()
                .ToArray();
        }

        /// <inheritdoc />
        public IEnumerable<JsonProperty> Apply(IEnumerable<JsonProperty> properties)
        {
            var result = properties;
            foreach (var excludeName in excludeNames)
            {
                result = result.Where(property => !string.Equals(property.PropertyName, excludeName, StringComparison.OrdinalIgnoreCase));
            }
            foreach (var excludeNameRegex in excludeNameRegexes)
            {
                result = result.Where(property => !excludeNameRegex.IsMatch(property.PropertyName));
            }
            return result;
        }
    }

    /// <summary>
    /// A <see cref="IJsonPropertyFilter"/> that applies two or more filters.
    /// </summary>
    internal sealed class AggregateJsonPropertyFilter : IJsonPropertyFilter
    {
        public AggregateJsonPropertyFilter(params Func<IEnumerable<JsonProperty>, IEnumerable<JsonProperty>>[] filters)
            : this((IReadOnlyCollection<Func<IEnumerable<JsonProperty>, IEnumerable<JsonProperty>>>)filters) { }

        public AggregateJsonPropertyFilter(IReadOnlyCollection<Func<IEnumerable<JsonProperty>, IEnumerable<JsonProperty>>> filters)
        {
            InnerFilters = filters ?? throw new ArgumentNullException(nameof(filters));
            if (!InnerFilters.Any())
            {
                throw new ArgumentException("Must have at least one inner filter");
            }
        }

        /// <summary>
        /// The filters that are applied by this aggregate filter.
        /// </summary>
        public IReadOnlyCollection<Func<IEnumerable<JsonProperty>, IEnumerable<JsonProperty>>> InnerFilters { get; }

        /// <inheritdoc />
        public IEnumerable<JsonProperty> Apply(IEnumerable<JsonProperty> properties)
        {
            var result = properties;
            foreach (var filter in InnerFilters)
            {
                result = filter(result);
            }
            return result;
        }
    }

    /// <summary>
    /// A custom contract resolver that attempt to ignore JSON attributes that modify the properties
    /// (like <see cref="JsonPropertyAttribute.PropertyName"/> and <see cref="JsonIgnoreAttribute"/>)
    /// and applies any property filters.
    /// </summary>
    internal class JsonDeepEqualContractResolver : DefaultContractResolver
    {
        private readonly Func<IEnumerable<JsonProperty>, IEnumerable<JsonProperty>>? jsonPropertyFilterOrNull;
        private readonly JsonAttributeHandling jsonAttributeHandling;

        public JsonDeepEqualContractResolver()
            : this(null) { }

        public JsonDeepEqualContractResolver(JsonDeepEqualDiffOptions? options)
        {
            this.jsonPropertyFilterOrNull = options?.ToJsonPropertyFilterOrNull();
            this.jsonAttributeHandling = options?.JsonAttributeHandling ?? JsonAttributeHandling.Ignore;
        }

        /// <inheritdoc />
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (jsonAttributeHandling == JsonAttributeHandling.Ignore)
            {
                // Don't allow attributes to change the property name.
                property.PropertyName = property.UnderlyingName;

                // Don't allow attributes to ignore properties.
                if (property.Ignored)
                {
                    property.Ignored = false;
                }
            }

            // Properties can be ignored by an optional filter.
            if (!property.Ignored && jsonPropertyFilterOrNull != null && !jsonPropertyFilterOrNull(new[] { property }).Any())
            {
                property.Ignored = true;
            }

            return property;
        }
    }

    /// <summary>
    /// A custom subclass of <see cref="DateTimeConverterBase"/> that supports custom date/time conversion.
    /// </summary>
    internal class JsonDeepEqualDateTimeConverter : DateTimeConverterBase
    {
        public JsonDeepEqualDateTimeConverter(Func<DateTime, DateTime>? dateTimeConverter)
        {
            DateTimeConverter = dateTimeConverter;
        }

        /// <summary>
        /// An optional converter for <see cref="DateTime"/> values that adjusts them before serialization.
        /// </summary>
        public Func<DateTime, DateTime>? DateTimeConverter { get; }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            string text;
            if (value is DateTime dateTime)
            {
                dateTime = DateTimeConverter?.Invoke(dateTime) ?? dateTime;
                text = dateTime.ToString(serializer.DateFormatString, CultureInfo.InvariantCulture);
            }
            else if (value is DateTimeOffset dateTimeOffset)
            {
                if (DateTimeConverter != null)
                {
                    var dateTimePart = dateTimeOffset.DateTime;
                    dateTimePart = DateTimeConverter(dateTimePart);
                    dateTimeOffset = new DateTimeOffset(dateTimePart, dateTimeOffset.Offset);
                }
                text = dateTimeOffset.ToString(serializer.DateFormatString, CultureInfo.InvariantCulture);
            }
            else
            {
                throw new JsonSerializationException($"Unexpected value when converting date. Expected DateTime or DateTimeOffset, got {value?.GetType().FullName ?? "null"}.");
            }
            writer.WriteValue(text);
        }

        /// <inheritdoc />
        public override bool CanRead => false;

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            => throw new NotSupportedException();
    }
}
