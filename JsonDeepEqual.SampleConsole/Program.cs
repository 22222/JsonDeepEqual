using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using Two.JsonDeepEqual;
using Two.JsonDeepEqual.Exceptions;

#pragma warning disable CA1801 // Remove unused parameter
#pragma warning disable CA1307 // Specify StringComparison

namespace SampleConsole
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Sample_Basic();
            Sample_WithOptions();
            Sample_Json();
            Sample_WithAllOptions();
            Diff_Basic();
            Diff_Json();
        }

        public static void Sample_Basic()
        {
            var expected = new
            {
                Message = "Hello!",
                Child = new { Id = 1, Values = new[] { 1, 2, 3 } },
            };
            var actual = new
            {
                Message = "Hello, World!",
                Child = new { Id = 2, Values = new[] { 1, 4, 3 } },
            };
            try
            {
                JsonDeepEqualAssert.Equal(expected, actual);
                JsonDeepEqualAssert.AreEqual(expected, actual);
            }
            catch (JsonEqualException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine();
            }

            JsonDeepEqualAssert.NotEqual(expected, actual);
            JsonDeepEqualAssert.AreNotEqual(expected, actual);
        }

        public static void Sample_WithOptions()
        {
            var expected = new
            {
                Id = 1,
                Message = "Hello!",
                Child = new { Id = 10, Values = new[] { 1, 2, 3 } },
                Created = new DateTime(2002, 2, 2, 12, 22, 23),
            };
            var actual = new
            {
                Id = 2,
                Message = "Hello, World!",
                Child = new { Id = 11, Values = new[] { 1, 4, 3 } },
                Created = new DateTime(2002, 2, 2, 12, 22, 22, 999),
            };
            JsonDeepEqualAssert.Equal(expected, actual, new JsonDeepEqualDiffOptions
            {
                ExcludePropertyNames = new[] { "Id", "Mess*" },
                ExcludePropertyPaths = new[] { "**/Values/*" },
                IgnoreArrayElementOrder = true,
                DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFK",
                DateTimeConverter = (DateTime dt) => new System.Data.SqlTypes.SqlDateTime(dt).Value,
            });
        }

        public static void Sample_Json()
        {
            var expectedJson = @"{ ""a"":1 }";
            var actualJson = @"{ ""a"":2 }";
            try
            {
                JsonAssert.Equal(expectedJson, actualJson);
            }
            catch (JsonEqualException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine();
            }
        }

        public static void Sample_WithAllOptions()
        {
            var expected = 1;
            var actual = 1;
            JsonDeepEqualAssert.Equal(expected, actual, new JsonDeepEqualDiffOptions
            {
                ExcludePropertyNames = new[] { "Id", "*DateTime" },
                PropertyFilter = (IEnumerable<JsonProperty> properties) => properties.Where(p => p.PropertyName != "Id" && p.PropertyType != typeof(DateTime)),
                NullValueHandling = NullValueHandling.Include,
                DefaultValueHandling = DefaultValueHandling.Include,
                ReferenceLoopHandling = ReferenceLoopHandling.Error,
                DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFK",
                DateTimeConverter = (DateTime dt) => new System.Data.SqlTypes.SqlDateTime(dt).Value,

                ExcludePropertyPaths = new[] { "**System/*" },
                PropertyPathFilter = (IEnumerable<string> paths) => paths.Where(p => !p.Contains("System")),
                IgnoreArrayElementOrder = true,
                IgnoreEmptyArrays = true,
                IgnoreEmptyObjects = true,
                IgnoreCase = true,
                IgnoreLineEndingDifferences = true,
                IgnoreWhiteSpaceDifferences = true,
            });
        }

        public static void Diff_Basic()
        {
            var expected = new { Message = "Hello!" };
            var actual = new { Message = "Hello, World!" };

            IEnumerable<JsonDiffNode> differences = JsonDeepEqualDiff.EnumerateDifferences(expected, actual);
            Console.WriteLine(string.Join(Environment.NewLine, differences.Take(10)));
        }

        public static void Diff_Json()
        {
            var expectedJson = @"{ ""a"":1 }";
            var actualJson = @"{ ""a"":2 }";

            IEnumerable<JsonDiffNode> differences = JsonDiff.EnumerateDifferences(expectedJson, actualJson);
            Console.WriteLine(string.Join(Environment.NewLine, differences.Take(10)));
        }
    }
}
