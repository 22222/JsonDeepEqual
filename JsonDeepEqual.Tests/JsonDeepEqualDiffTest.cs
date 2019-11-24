using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using Xunit;

namespace Two.JsonDeepEqual
{
    public class JsonDeepEqualDiffTest
    {
        [Theory]
        [InlineData(2, 2, 0)]
        [InlineData(1, 2, 1)]
        [InlineData(2.0f, 2.0f, 0)]
        [InlineData(1.0f, 2.0f, 1)]
        [InlineData("hello", "hello", 0)]
        [InlineData("hello", "world", 1)]
        public void EnumerateDifferences_SimpleValue(object a, object b, int expectedDifferenceCount)
        {
            var differences = JsonDeepEqualDiff.EnumerateDifferences(a, b).ToList();
            Assert.Equal(expectedDifferenceCount, differences.Count);
        }

        [Fact]
        public void EnumerateDifferences_PrimitiveArrays_ShouldBeEqual()
        {
            var a = new int[] { 1, 2, 3 };
            var b = new int[] { 1, 2, 3 };
            var differences = JsonDeepEqualDiff.EnumerateDifferences(a, b).ToList();
            Assert.Empty(differences);
        }

        [Fact]
        public void EnumerateDifferences_BinaryArrays_ShouldBeEqual()
        {
            var a = new byte[] { 1, 2, 3 };
            var b = new byte[] { 1, 2, 3 };
            var differences = JsonDeepEqualDiff.EnumerateDifferences(a, b).ToList();
            Assert.Empty(differences);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void EnumerateDifferences_PrimitiveArrays_SameLengtWithNoOverlap_ShouldHaveDifferences(bool ignoreOrder)
        {
            var a = new int[] { 1, 2, 3 };
            var b = new int[] { 4, 5, 6 };
            var differences = JsonDeepEqualDiff.EnumerateDifferences(a, b, new JsonDeepEqualDiffOptions { IgnoreArrayElementOrder = ignoreOrder }).ToList();
            if (!ignoreOrder)
            {
                Assert.Equal(3, differences.Count);

                Assert.Equal("/0", differences[0].Path);
                Assert.Equal(1, differences[0].ExpectedValue.ToObject<object>());
                Assert.Equal(4, differences[0].ActualValue.ToObject<object>());

                Assert.Equal("/1", differences[1].Path);
                Assert.Equal(2, differences[1].ExpectedValue.ToObject<object>());
                Assert.Equal(5, differences[1].ActualValue.ToObject<object>());

                Assert.Equal("/2", differences[2].Path);
                Assert.Equal(3, differences[2].ExpectedValue.ToObject<object>());
                Assert.Equal(6, differences[2].ActualValue.ToObject<object>());
            }
            else
            {
                Assert.Single(differences);

                Assert.Equal("/*", differences[0].Path);
                Assert.Equal(a, differences[0].ExpectedValue.ToObject<int[]>());
                Assert.Equal(b, differences[0].ActualValue.ToObject<int[]>());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void EnumerateDifferences_PrimitiveArrays_MissingActualElementWithNoOverlap_ShouldHaveDifferences(bool ignoreOrder)
        {
            var a = new int[] { 1, 2, 3 };
            var b = new int[] { 4, 5 };
            var differences = JsonDeepEqualDiff.EnumerateDifferences(a, b, new JsonDeepEqualDiffOptions { IgnoreArrayElementOrder = ignoreOrder }).ToList();
            if (!ignoreOrder)
            {
                Assert.Equal(3, differences.Count);

                Assert.Equal("/0", differences[0].Path);
                Assert.Equal(1, differences[0].ExpectedValue.ToObject<object>());
                Assert.Equal(4, differences[0].ActualValue.ToObject<object>());

                Assert.Equal("/1", differences[1].Path);
                Assert.Equal(2, differences[1].ExpectedValue.ToObject<object>());
                Assert.Equal(5, differences[1].ActualValue.ToObject<object>());

                Assert.Equal("/2", differences[2].Path);
                Assert.Equal(3, differences[2].ExpectedValue.ToObject<object>());
                Assert.Null(differences[2].ActualValue.ToObject<object>());
            }
            else
            {
                Assert.Equal(2, differences.Count);

                Assert.Equal("/*", differences[0].Path);
                Assert.Equal(a, differences[0].ExpectedValue.ToObject<int[]>());
                Assert.Equal(b, differences[0].ActualValue.ToObject<int[]>());

                Assert.Equal("/length", differences[1].Path);
                Assert.Equal(3, differences[1].ExpectedValue.ToObject<object>());
                Assert.Equal(2, differences[1].ActualValue.ToObject<object>());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void EnumerateDifferences_PrimitiveArrays_MissingExpectedElementWithNoOverlap_ShouldHaveDifferences(bool ignoreOrder)
        {
            var a = new int[] { 1, 2 };
            var b = new int[] { 3, 4, 5 };
            var differences = JsonDeepEqualDiff.EnumerateDifferences(a, b, new JsonDeepEqualDiffOptions { IgnoreArrayElementOrder = ignoreOrder }).ToList();
            if (!ignoreOrder)
            {
                Assert.Equal(3, differences.Count);

                Assert.Equal("/0", differences[0].Path);
                Assert.Equal(1, differences[0].ExpectedValue.ToObject<object>());
                Assert.Equal(3, differences[0].ActualValue.ToObject<object>());

                Assert.Equal("/1", differences[1].Path);
                Assert.Equal(2, differences[1].ExpectedValue.ToObject<object>());
                Assert.Equal(4, differences[1].ActualValue.ToObject<object>());

                Assert.Equal("/2", differences[2].Path);
                Assert.Null(differences[2].ExpectedValue.ToObject<object>());
                Assert.Equal(5, differences[2].ActualValue.ToObject<object>());
            }
            else
            {
                Assert.Equal(2, differences.Count);

                Assert.Equal("/*", differences[0].Path);
                Assert.Equal(a, differences[0].ExpectedValue.ToObject<int[]>());
                Assert.Equal(b, differences[0].ActualValue.ToObject<int[]>());

                Assert.Equal("/length", differences[1].Path);
                Assert.Equal(2, differences[1].ExpectedValue.ToObject<object>());
                Assert.Equal(3, differences[1].ActualValue.ToObject<object>());
            }
        }

        [Fact]
        public void EnumerateDifferences_PlainObjects_ShouldBeEqual()
        {
            var a = new object();
            var b = new object();
            var differences = JsonDeepEqualDiff.EnumerateDifferences(a, b).ToList();
            Assert.Empty(differences);
        }

        [Fact]
        public void EnumerateDifferences_TestObject1_SameObject()
        {
            var a = new TestClass1
            {
                Id = 1,
                Name = "Test",
                BinaryData = new byte[] { 1, 2, 3 },
                ChildArray = new[]
                {
                    new TestClass1Child { ChildId = 1, },
                    new TestClass1Child { ChildId = 2, },
                },
                ChildCollection = new[]
                {
                    new TestClass1Child { ChildId = 3, },
                    new TestClass1Child { ChildId = 4, },
                },
            };

            var differences = JsonDeepEqualDiff.EnumerateDifferences(a, a).ToList();
            Assert.Empty(differences);
        }

        [Fact]
        public void EnumerateDifferences_TestObject1_Equal()
        {
            var a = new TestClass1
            {
                Id = 1,
                Name = "Test",
                BinaryData = new byte[] { 1, 2, 3 },
                ChildArray = new[]
                {
                    new TestClass1Child { ChildId = 1, },
                    new TestClass1Child { ChildId = 2, },
                },
                ChildCollection = new[]
                {
                    new TestClass1Child { ChildId = 3, },
                    new TestClass1Child { ChildId = 4, },
                },
            };
            var b = new TestClass1
            {
                Id = 1,
                Name = "Test",
                BinaryData = new byte[] { 1, 2, 3 },
                ChildArray = new[]
                {
                    new TestClass1Child { ChildId = 1, },
                    new TestClass1Child { ChildId = 2, },
                },
                ChildCollection = new[]
                {
                    new TestClass1Child { ChildId = 3, },
                    new TestClass1Child { ChildId = 4, },
                },
            };
            var differences = JsonDeepEqualDiff.EnumerateDifferences(a, b).ToList();
            Assert.Empty(differences);
        }

        [Fact]
        public void EnumerateDifferences_TestObject1_DifferentValues()
        {
            var a = new TestClass1
            {
                Id = 1,
                Name = "Test",
                ChildArray = new[]
                {
                    new TestClass1Child { ChildId = 1, },
                    new TestClass1Child { ChildId = 2, },
                },
                ChildCollection = new[]
                {
                    new TestClass1Child { ChildId = 3, },
                    new TestClass1Child { ChildId = 4, },
                },
            };
            var b = new TestClass1
            {
                Id = 1,
                Name = "Test2",
                ChildArray = new[]
                {
                    new TestClass1Child { ChildId = 1, },
                    new TestClass1Child { ChildId = 2, },
                },
                ChildCollection = new[]
                {
                    new TestClass1Child { ChildId = 22, },
                    new TestClass1Child { ChildId = 4, },
                },
            };
            var differences = JsonDeepEqualDiff.EnumerateDifferences(a, b).ToList();
            Assert.Equal(2, differences.Count);

            var diff0 = differences.ElementAt(0);
            Assert.Contains("\"Test\"", diff0.ToString(), StringComparison.Ordinal);
            Assert.Contains("\"Test2\"", diff0.ToString(), StringComparison.Ordinal);

            var diff1 = differences.ElementAt(1);
            Assert.Contains("3", diff1.ToString(), StringComparison.Ordinal);
            Assert.Contains("22", diff1.ToString(), StringComparison.Ordinal);
        }

        [Fact]
        public void EnumerateDifferences_TestObject1_DifferentBinaryValues()
        {
            var a = new TestClass1
            {
                Id = 1,
                Name = "Test",
                BinaryData = new byte[] { 1, 2, 3 },
            };
            var b = new TestClass1
            {
                Id = 1,
                Name = "Test",
                BinaryData = new byte[] { 1, 2, 2 },
            };
            var differences = JsonDeepEqualDiff.EnumerateDifferences(a, b).ToList();
            Assert.Single(differences);
            Assert.Equal("\"AQID\"", differences[0].ExpectedValueDisplay);
            Assert.Equal("\"AQIC\"", differences[0].ActualValueDisplay);
        }

        [Fact]
        public void EnumerateDifferences_TestObject1_EmptyArrays()
        {
#pragma warning disable CA1825 // Avoid zero-length array allocations.
            var a = new TestClass1
            {
                Id = 1,
                Name = "Test",
                ChildArray = new TestClass1Child[0],
                ChildCollection = new TestClass1Child[0],
            };
            var b = new TestClass1
            {
                Id = 1,
                Name = "Test",
                ChildArray = new TestClass1Child[0],
                ChildCollection = new TestClass1Child[0],
            };
#pragma warning restore CA1825 // Avoid zero-length array allocations.
            var differences = JsonDeepEqualDiff.EnumerateDifferences(a, b).ToList();
            Assert.Empty(differences);
        }

        [Fact]
        public void EnumerateDifferences_TestObject1_MissingArrayAndDifferentCollectionLengths()
        {
            var a = new TestClass1
            {
                Id = 1,
                Name = "Test",
                ChildArray = new[]
                {
                    new TestClass1Child { ChildId = 1, },
                    new TestClass1Child { ChildId = 2, },
                },
                ChildCollection = new[]
                {
                    new TestClass1Child { ChildId = 3, },
                    new TestClass1Child { ChildId = 4, },
                },
            };
            var b = new TestClass1
            {
                Id = 1,
                Name = "Test",
                ChildCollection = new[]
                {
                    new TestClass1Child { ChildId = 3, },
                    new TestClass1Child { ChildId = 4, },
                    new TestClass1Child { ChildId = 5, },
                },
            };
            var differences = JsonDeepEqualDiff.EnumerateDifferences(a, b).ToList();
            Assert.Equal(2, differences.Count);

            Assert.Equal("/ChildArray", differences[0].Path);
            Assert.Equal(JArray.FromObject(a.ChildArray).ToString(), differences[0].ExpectedValue.ToString());
            Assert.Null(differences[0].ActualValue.ToObject<object>());

            Assert.Equal("/ChildCollection/2", differences[1].Path);
            Assert.Null(differences[1].ExpectedValue.ToObject<object>());
            Assert.Equal(JObject.FromObject(b.ChildCollection.ElementAt(2)).ToString(), differences[1].ActualValue.ToString());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void EnumerateDifferences_TestObject1_EmptyArrayAndDifferentCollectionLengths(bool includeDefaultValues)
        {
            var a = new TestClass1
            {
                Id = 1,
                Name = "Test",
                ChildArray = new[]
                {
                    new TestClass1Child { ChildId = 1, },
                    new TestClass1Child { ChildId = 2, },
                },
                ChildCollection = new[]
                {
                    new TestClass1Child { ChildId = 3, },
                    new TestClass1Child { ChildId = 4, },
                },
            };
            var b = new TestClass1
            {
                Id = 1,
                Name = "Test",
                ChildArray = new[]
                {
                    new TestClass1Child(),
                },
                ChildCollection = new[]
                {
                    new TestClass1Child { ChildId = 3, },
                    new TestClass1Child { ChildId = 4, },
                    new TestClass1Child { ChildId = 5, },
                },
            };

            var options = new JsonDeepEqualDiffOptions
            {
                DefaultValueHandling = includeDefaultValues ? DefaultValueHandling.Include : default(DefaultValueHandling?),
            };
            var differences = JsonDeepEqualDiff.EnumerateDifferences(a, b, options).ToList();
            Assert.Equal(3, differences.Count);

            Assert.Equal("/ChildArray/0/ChildId", differences[0].Path);
            Assert.Equal(1, differences[0].ExpectedValue.ToObject<object>());
            if (includeDefaultValues)
            {
                Assert.Equal(0, differences[0].ActualValue.ToObject<object>());
            }
            else
            {
                Assert.Null(differences[0].ActualValue.ToObject<object>());
            }

            Assert.Equal("/ChildArray/1", differences[1].Path);
            Assert.Equal(JObject.FromObject(a.ChildArray[1]).ToString(), differences[1].ExpectedValue.ToObject<object>()?.ToString());
            Assert.Null(differences[1].ActualValue.ToObject<object>());

            Assert.Equal("/ChildCollection/2", differences[2].Path);
            Assert.Null(differences[2].ExpectedValue.ToObject<object>());
            Assert.Equal(JObject.FromObject(b.ChildCollection.ElementAt(2)).ToString(), differences[2].ActualValue.ToObject<object>()?.ToString());
        }

        [Fact]
        public void EnumerateDifferences_TestObject2_DifferentClasses_NoDifferences()
        {
            var a = new TestClass2WithJsonAttributes
            {
                Id = 1,
                Name = "Test",
            };
            var b = new TestClass2WithoutJsonAttributes
            {
                Id = 1,
                Name = "Test",
            };
            var differences = JsonDeepEqualDiff.EnumerateDifferences(a, b).ToList();
            Assert.Empty(differences);
        }

        [Fact]
        public void EnumerateDifferences_TestObject2_DifferentClasses_IncludeJsonAttributeHandling_Differences()
        {
            var a = new TestClass2WithJsonAttributes
            {
                Id = 1,
                Name = "Test",
            };
            var b = new TestClass2WithoutJsonAttributes
            {
                Id = 1,
                Name = "Test",
            };
            var differences = JsonDeepEqualDiff.EnumerateDifferences(a, b, new JsonDeepEqualDiffOptions { JsonAttributeHandling = JsonAttributeHandling.Include }).ToList();
            Assert.Equal(3, differences.Count);

            Assert.Equal("/description", differences[0].Path);
            Assert.Equal("Test", differences[0].ExpectedValue.ToObject<object>());
            Assert.Null(differences[0].ActualValue.ToObject<object>());

            Assert.Equal("/Id", differences[1].Path);
            Assert.Null(differences[1].ExpectedValue.ToObject<object>());
            Assert.Equal(1, differences[1].ActualValue.ToObject<object>());

            Assert.Equal("/Name", differences[2].Path);
            Assert.Null(differences[2].ExpectedValue.ToObject<object>());
            Assert.Equal("Test", differences[2].ActualValue.ToObject<object>());
        }

        [Fact]
        public void EnumerateDifferences_DirectOneNodeCycle_DifferentRootObjects()
        {
            var obj1 = new SelfReferenceTestClass();
            obj1.Child = obj1;

            var obj2 = new SelfReferenceTestClass();
            obj2.Child = obj1;

            var differences = JsonDeepEqualDiff.EnumerateDifferences(obj1, obj2).ToList();
            Assert.Single(differences);
        }

        [Fact]
        public void EnumerateDifferences_DirectOneNodeCycle_SameRootObject()
        {
            var obj1 = new SelfReferenceTestClass();
            obj1.Child = obj1;

            var differences = JsonDeepEqualDiff.EnumerateDifferences(obj1, obj1).ToList();
            Assert.Empty(differences);
        }

        [Fact]
        public void EnumerateDifferences_Strings_IgnoreCase()
        {
            var expected = "Hello";
            var actual = "hello";
            Assert.NotEmpty(JsonDeepEqualDiff.EnumerateDifferences(expected, actual));
            Assert.Empty(JsonDeepEqualDiff.EnumerateDifferences(expected, actual, new JsonDeepEqualDiffOptions { IgnoreCase = true }));
        }

        [Fact]
        public void EnumerateDifferences_Strings_IgnoreWhiteSpaceDifferences()
        {
            var expected = "hello world";
            var actual = "hello  world";
            Assert.NotEmpty(JsonDeepEqualDiff.EnumerateDifferences(expected, actual));
            Assert.Empty(JsonDeepEqualDiff.EnumerateDifferences(expected, actual, new JsonDeepEqualDiffOptions { IgnoreWhiteSpaceDifferences = true }));
        }

        [Fact]
        public void EnumerateDifferences_Strings_IgnoreLineEndingDifferences()
        {
            var expected = "hello\nworld";
            var actual = "hello\r\nworld";
            Assert.NotEmpty(JsonDeepEqualDiff.EnumerateDifferences(expected, actual));
            Assert.Empty(JsonDeepEqualDiff.EnumerateDifferences(expected, actual, new JsonDeepEqualDiffOptions { IgnoreLineEndingDifferences = true }));
        }

        [Theory]
        [InlineData(null, 123, 456, true)]
        [InlineData("yyyy'-'MM'-'dd'T'HH':'mm':'ssK", 123, 456, true)]
        [InlineData("yyyy'-'MM'-'dd'T'HH':'mm':'ssK", 498, 499, true)]
        [InlineData("yyyy'-'MM'-'dd'T'HH':'mm':'ssK", 499, 500, true)]
        [InlineData("yyyy'-'MM'-'dd'T'HH':'mm':'ssK", 500, 501, true)]
        [InlineData("yyyy'-'MM'-'dd'T'HH':'mm':'ssK", 000, 999, true)]
        [InlineData("yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK", 000, 000, true)]
        [InlineData("yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK", 000, 999, false)]
        [InlineData("yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFK", 123, 456, false)]
        [InlineData("yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFK", 000, 001, false)]
        [InlineData("yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFK", 998, 999, false)]
        [InlineData("yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFK", 000, 999, false)]
        public void EnumerateDifferences_DateValues_DateFormatString(string dateFormatString, int expectedMs, int actualMs, bool expectEqual)
        {
            var expected = new PrimitiveValuesTestClass
            {
                DateTimeValue = new DateTime(2002, 2, 2, 12, 22, 22, expectedMs),
                NullableDateTimeValue = new DateTime(2002, 2, 2, 12, 22, 22, expectedMs),
                DateTimeOffsetValue = new DateTimeOffset(2002, 2, 2, 12, 22, 22, expectedMs, TimeSpan.Zero),
                NullableDateTimeOffsetValue = new DateTimeOffset(2002, 2, 2, 12, 22, 22, expectedMs, TimeSpan.Zero),
            };
            var actual = new PrimitiveValuesTestClass
            {
                DateTimeValue = new DateTime(2002, 2, 2, 12, 22, 22, actualMs),
                NullableDateTimeValue = new DateTime(2002, 2, 2, 12, 22, 22, actualMs),
                DateTimeOffsetValue = new DateTimeOffset(2002, 2, 2, 12, 22, 22, actualMs, TimeSpan.Zero),
                NullableDateTimeOffsetValue = new DateTimeOffset(2002, 2, 2, 12, 22, 22, actualMs, TimeSpan.Zero),
            };

            var differences = JsonDeepEqualDiff.EnumerateDifferences(expected, actual, new JsonDeepEqualDiffOptions { DateFormatString = dateFormatString });
            if (expectEqual)
            {
                Assert.Empty(differences);
            }
            else
            {
                Assert.NotEmpty(differences);
            }
        }

        [Theory]
        [InlineData(123, 456, true)]
        [InlineData(000, 001, true)]
        [InlineData(998, 999, true)]
        [InlineData(000, 999, false)]
        [InlineData(498, 499, true)]
        [InlineData(499, 500, false)]
        [InlineData(500, 501, true)]
        public void EnumerateDifferences_DateValues_DateTimeConverter_RoundSeconds(int expectedMs, int actualMs, bool expectEqual)
        {
            var expected = new PrimitiveValuesTestClass
            {
                DateTimeValue = new DateTime(2002, 2, 2, 12, 22, 22, expectedMs),
                NullableDateTimeValue = new DateTime(2002, 2, 2, 12, 22, 22, expectedMs),
                DateTimeOffsetValue = new DateTimeOffset(2002, 2, 2, 12, 22, 22, expectedMs, TimeSpan.Zero),
                NullableDateTimeOffsetValue = new DateTimeOffset(2002, 2, 2, 12, 22, 22, expectedMs, TimeSpan.Zero),
            };
            var actual = new PrimitiveValuesTestClass
            {
                DateTimeValue = new DateTime(2002, 2, 2, 12, 22, 22, actualMs),
                NullableDateTimeValue = new DateTime(2002, 2, 2, 12, 22, 22, actualMs),
                DateTimeOffsetValue = new DateTimeOffset(2002, 2, 2, 12, 22, 22, actualMs, TimeSpan.Zero),
                NullableDateTimeOffsetValue = new DateTimeOffset(2002, 2, 2, 12, 22, 22, actualMs, TimeSpan.Zero),
            };

            var differences = JsonDeepEqualDiff.EnumerateDifferences(expected, actual, new JsonDeepEqualDiffOptions
            {
                DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFK",
                DateTimeConverter = (dt) =>
                {
                    var roundedSecond = dt.Second + (dt.Millisecond >= 500 ? 1 : 0);
                    return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, roundedSecond, dt.Kind);
                },
            });
            if (expectEqual)
            {
                Assert.Empty(differences);
            }
            else
            {
                Assert.NotEmpty(differences);
            }
        }

        [Theory]
        [InlineData(123, 456, false)]
        [InlineData(000, 001, true)]
        [InlineData(000, 999, false)]
        [InlineData(001, 002, false)]
        [InlineData(003, 004, true)]
        [InlineData(997, 998, true)]
        [InlineData(998, 999, false)]
        public void EnumerateDifferences_DateValues_DateTimeConverter_SqlDateTime(int expectedMs, int actualMs, bool expectEqual)
        {
            var expected = new PrimitiveValuesTestClass
            {
                DateTimeValue = new DateTime(2002, 2, 2, 12, 22, 22, expectedMs),
                NullableDateTimeValue = new DateTime(2002, 2, 2, 12, 22, 22, expectedMs),
                DateTimeOffsetValue = new DateTimeOffset(2002, 2, 2, 12, 22, 22, expectedMs, TimeSpan.Zero),
                NullableDateTimeOffsetValue = new DateTimeOffset(2002, 2, 2, 12, 22, 22, expectedMs, TimeSpan.Zero),
            };
            var actual = new PrimitiveValuesTestClass
            {
                DateTimeValue = new DateTime(2002, 2, 2, 12, 22, 22, actualMs),
                NullableDateTimeValue = new DateTime(2002, 2, 2, 12, 22, 22, actualMs),
                DateTimeOffsetValue = new DateTimeOffset(2002, 2, 2, 12, 22, 22, actualMs, TimeSpan.Zero),
                NullableDateTimeOffsetValue = new DateTimeOffset(2002, 2, 2, 12, 22, 22, actualMs, TimeSpan.Zero),
            };

            var differences = JsonDeepEqualDiff.EnumerateDifferences(expected, actual, new JsonDeepEqualDiffOptions
            {
                DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK",
                DateTimeConverter = (dt) => new System.Data.SqlTypes.SqlDateTime(dt).Value,
            });
            if (expectEqual)
            {
                Assert.Empty(differences);
            }
            else
            {
                Assert.NotEmpty(differences);
            }
        }
    }
}
