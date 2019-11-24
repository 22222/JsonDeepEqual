using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;

#pragma warning disable CA1819 // Properties should not return arrays
#pragma warning disable CA2227 // Collection properties should be read only
#pragma warning disable SA1011 // Closing square brackets should be spaced correctly
#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1649 // File name should match first type name

namespace Two.JsonDeepEqual
{
    public class TestClass1
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public byte[]? BinaryData { get; set; }

        public TestClass1Child[]? ChildArray { get; set; }

        public ICollection<TestClass1Child>? ChildCollection { get; set; }
    }

    public class TestClass1Child
    {
        public int ChildId { get; set; }
    }

    public class TestClass2WithoutJsonAttributes
    {
        public int Id { get; set; }

        public string? Name { get; set; }
    }

    public class TestClass2WithJsonAttributes
    {
        [JsonIgnore]
        public int Id { get; set; }

        [JsonProperty("description")]
        public string? Name { get; set; }
    }

    public class SelfReferenceTestClass
    {
        public SelfReferenceTestClass? Child { get; set; }
    }

    public class ListTestClass<T>
    {
        public IEnumerable<T> Enumerable { get; set; } = System.Array.Empty<T>();

        public IReadOnlyCollection<T> ReadOnlyCollection { get; set; } = System.Array.Empty<T>();

        public ICollection<T> Collection { get; set; } = System.Array.Empty<T>();

        public T[] Array { get; set; } = System.Array.Empty<T>();

        public IList<T> IList { get; set; } = System.Array.Empty<T>();

        public List<T> List { get; set; } = new List<T>();
    }

    public class DictionaryTestClass<TKey, TValue>
        where TKey : notnull
    {
        public IReadOnlyDictionary<TKey, TValue>? Dictionary { get; set; }

        public IReadOnlyDictionary<TKey, IEnumerable<TValue>>? DictionaryOfEnumerables { get; set; }

        public IReadOnlyDictionary<TKey, IReadOnlyCollection<TValue>>? DictionaryOfCollections { get; set; }
    }

    public class ReflectionValuesTestClass
    {
        public Type? Type { get; set; }

        public Type? TypeGetOnly => Type;

        public PropertyInfo? PropertyInfo { get; set; }

        public PropertyInfo? PropertyInfoGetOnly => PropertyInfo;
    }

    public class PrimitiveValuesTestClass
    {
        public static PrimitiveValuesTestClass CreateSample()
        {
            return new PrimitiveValuesTestClass
            {
                StringValue = "two",
                IntValue = 1,
                NullableIntValue = 2,
                UIntValue = 3,
                NullableUIntValue = 4,
                LongValue = 5L,
                NullableLongValue = 6L,
                ULongValue = 7L,
                NullableULongValue = 8L,
                ShortValue = 9,
                NullableShortValue = 10,
                UShortValue = 11,
                NullableUShortValue = 12,
                ByteValue = 13,
                NullableByteValue = 14,
                SByteValue = 15,
                NullableSByteValue = 16,
                BoolValue = true,
                NullableBoolValue = false,
                DecimalValue = 1.1m,
                NullableDecimalValue = 2.2m,
                DoubleValue = 3.3d,
                NullableDoubleValue = 4.4d,
                FloatValue = 5.5d,
                NullableFloatValue = 6.6d,
                DateTimeValue = new DateTime(2000, 1, 1, 12, 1, 2),
                NullableDateTimeValue = new DateTime(2000, 1, 2, 12, 1, 2),
                DateTimeOffsetValue = new DateTimeOffset(2000, 1, 3, 12, 1, 2, TimeSpan.Zero),
                NullableDateTimeOffsetValue = new DateTimeOffset(2000, 1, 4, 12, 1, 2, TimeSpan.Zero),
                TimeSpanValue = TimeSpan.FromHours(1),
                NullableTimeSpanValue = TimeSpan.FromHours(2),
                GuidValue = new Guid("e9e13594-6cb4-45f0-b20b-b4e947161256"),
                NullableGuidValue = new Guid("36408fcb-eb6c-4440-95ed-39ec43866347"),
                ByteArrayValue = new byte[] { 1, 2, 255 },
            };
        }

        public string? StringValue { get; set; }

        public int IntValue { get; set; }

        public int? NullableIntValue { get; set; }

        public uint UIntValue { get; set; }

        public uint? NullableUIntValue { get; set; }

        public long LongValue { get; set; }

        public long? NullableLongValue { get; set; }

        public ulong ULongValue { get; set; }

        public ulong? NullableULongValue { get; set; }

        public short ShortValue { get; set; }

        public short? NullableShortValue { get; set; }

        public ushort UShortValue { get; set; }

        public ushort? NullableUShortValue { get; set; }

        public byte ByteValue { get; set; }

        public byte? NullableByteValue { get; set; }

        public sbyte SByteValue { get; set; }

        public sbyte? NullableSByteValue { get; set; }

        public bool BoolValue { get; set; }

        public bool? NullableBoolValue { get; set; }

        public decimal DecimalValue { get; set; }

        public decimal? NullableDecimalValue { get; set; }

        public double DoubleValue { get; set; }

        public double? NullableDoubleValue { get; set; }

        public double FloatValue { get; set; }

        public double? NullableFloatValue { get; set; }

        public DateTime DateTimeValue { get; set; }

        public DateTime? NullableDateTimeValue { get; set; }

        public DateTimeOffset DateTimeOffsetValue { get; set; }

        public DateTimeOffset? NullableDateTimeOffsetValue { get; set; }

        public TimeSpan TimeSpanValue { get; set; }

        public TimeSpan? NullableTimeSpanValue { get; set; }

        public Guid GuidValue { get; set; }

        public Guid? NullableGuidValue { get; set; }

        public byte[]? ByteArrayValue { get; set; }
    }
}
