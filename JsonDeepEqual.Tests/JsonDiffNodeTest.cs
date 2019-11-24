using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Two.JsonDeepEqual
{
    public class JsonDiffNodeTest
    {
        [Theory]
        [InlineData(2, "2")]
        [InlineData(2.0f, "2.0")]
        [InlineData(2.123f, "2.123")]
        [InlineData(2.0d, "2.0")]
        [InlineData(2.123d, "2.123")]
        [InlineData("", "\"\"")]
        [InlineData("2", "\"2\"")]
        [InlineData("Hello world!", "\"Hello world!\"")]
        [InlineData("\"hi\"", "\"\\\"hi\\\"\"")]
        [InlineData(@"domain\user", @"""domain\\user""")]
        [InlineData(null, "null")]
        [InlineData(true, "true")]
        [InlineData(false, "false")]
        public void ToString_SimpleValue(object value, string expectedValue)
        {
            var valueJToken = value != null ? JToken.FromObject(value) : JValue.CreateNull();
            var difference = new JsonDiffNode("/Test", valueJToken, null);
            var actualMessage = difference.ToString();

            var expectedMessage = $@"/Test:
    Expected: {expectedValue}
    Actual:   null";
            Assert.Equal(expectedMessage, actualMessage, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void ToString_StringValues()
        {
            var difference = new JsonDiffNode("/Test", JToken.FromObject("Hello, World"), JToken.FromObject("Hello, blorld"));
            var actualMessage = difference.ToString();

            var expectedMessage = @"/Test:
                      ↓ (pos 8)
    Expected: ""Hello, World""
    Actual:   ""Hello, blorld""
                      ↑ (pos 8)";
            Assert.Equal(expectedMessage, actualMessage, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void ToString_StringValues_CaseDifference()
        {
            var difference = new JsonDiffNode("/Test", JToken.FromObject("Hello, World"), JToken.FromObject("Hello, world"));
            var actualMessage = difference.ToString();

            var expectedMessage = @"/Test:
                      ↓ (pos 8)
    Expected: ""Hello, World""
    Actual:   ""Hello, world""
                      ↑ (pos 8)";
            Assert.Equal(expectedMessage, actualMessage, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void ToString_StringValues_NoDifference()
        {
            var difference = new JsonDiffNode("/Test", JToken.FromObject("Hello, World"), JToken.FromObject("Hello, World"));
            var actualMessage = difference.ToString();

            var expectedMessage = @"/Test:
    Expected: ""Hello, World""
    Actual:   ""Hello, World""";
            Assert.Equal(expectedMessage, actualMessage, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void ToString_DecimalValues()
        {
            var difference = new JsonDiffNode("/Test", JToken.FromObject(2.123m), JToken.FromObject(2m));
            var actualMessage = difference.ToString();

            var expectedMessage = $@"/Test:
    Expected: 2.123
    Actual:   2.0";
            Assert.Equal(expectedMessage, actualMessage, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void ToString_EnumValues_WithStringEnumConverter()
        {
            var jsonSerializer = new JsonSerializer();
            jsonSerializer.Converters.Add(new StringEnumConverter());
            var difference = new JsonDiffNode("/Test", JToken.FromObject(StringComparison.OrdinalIgnoreCase, jsonSerializer), JToken.FromObject(StringComparison.Ordinal, jsonSerializer));
            var actualMessage = difference.ToString();

            var expectedMessage = @"/Test:
                      ↓ (pos 8)
    Expected: ""OrdinalIgnoreCase""
    Actual:   ""Ordinal""
                      ↑ (pos 8)";
            Assert.Equal(expectedMessage, actualMessage, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void ToString_EnumValues_WithoutStringEnumConverter()
        {
            var difference = new JsonDiffNode("/Test", JToken.FromObject(StringComparison.OrdinalIgnoreCase), JToken.FromObject(StringComparison.Ordinal));
            var actualMessage = difference.ToString();

            var expectedMessage = @"/Test:
    Expected: 5
    Actual:   4";
            Assert.Equal(expectedMessage, actualMessage, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void ToString_EnumValues_MissingName()
        {
            var jsonSerializer = new JsonSerializer();
            jsonSerializer.Converters.Add(new StringEnumConverter());
            var difference = new JsonDiffNode("/Test", JToken.FromObject((StringComparison)2222, jsonSerializer), JToken.FromObject((StringComparison)22222, jsonSerializer));
            var actualMessage = difference.ToString();

            var expectedMessage = @"/Test:
    Expected: 2222
    Actual:   22222";
            Assert.Equal(expectedMessage, actualMessage, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void ToString_ByteArrayValues()
        {
            var difference = new JsonDiffNode("/Test", JToken.FromObject(new byte[] { 1, 171, 128, 3 }), JToken.FromObject(new byte[] { 2 }));
            var actualMessage = difference.ToString();

            var expectedMessage = $@"/Test:
                ↓ (pos 2)
    Expected: ""AauAAw==""
    Actual:   ""Ag==""
                ↑ (pos 2)";
            Assert.Equal(expectedMessage, actualMessage, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void ToString_LongStringValues_DifferenceInMiddle()
        {
            var expectedValue = string.Join(string.Empty, Enumerable.Range(0, 512));
            var actualValue = string.Join(string.Empty, Enumerable.Range(0, 256)) + string.Join(string.Empty, Enumerable.Range(0, 256));
            // Assert.Equal('"' + expectedValue + '"', '"' + actualValue + '"');

            var difference = new JsonDiffNode("/Test", JToken.FromObject(expectedValue), JToken.FromObject(actualValue));
            var actualMessage = difference.ToString();

            var expectedMessage = $@"/Test:
                                   ↓ (pos 659)
    Expected: …4925025125225325425525625725825926026126226326426526626726826…
    Actual:   …4925025125225325425501234567891011121314151617181920212223242…
                                   ↑ (pos 659)";
            Assert.Equal(expectedMessage, actualMessage, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void ToString_LongStringValues_DifferenceInMiddle_BarelyTruncated()
        {
            var str1 = string.Join(string.Empty, Enumerable.Range(0, 20).Select(i => (i % 10).ToString(CultureInfo.InvariantCulture)));
            var str2 = string.Join(string.Empty, Enumerable.Range(0, 40).Select(i => (i % 10).ToString(CultureInfo.InvariantCulture)));

            var expectedValue = str1 + 'a' + str2;
            var actualValue = str1 + 'b' + str2;
            // Assert.Equal('"' + expectedValue + '"', '"' + actualValue + '"');

            var difference = new JsonDiffNode("/Test", JToken.FromObject(expectedValue), JToken.FromObject(actualValue));
            var actualMessage = difference.ToString();

            var expectedMessage = $@"/Test:
                                   ↓ (pos 21)
    Expected: …01234567890123456789a0123456789012345678901234567890123456789…
    Actual:   …01234567890123456789b0123456789012345678901234567890123456789…
                                   ↑ (pos 21)";
            Assert.Equal(expectedMessage, actualMessage, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void ToString_LongStringValues_DifferenceInMiddle_NotTruncated()
        {
            var str1 = string.Join(string.Empty, Enumerable.Range(0, 19).Select(i => (i % 10).ToString(CultureInfo.InvariantCulture)));
            var str2 = string.Join(string.Empty, Enumerable.Range(0, 39).Select(i => (i % 10).ToString(CultureInfo.InvariantCulture)));

            var expectedValue = str1 + 'a' + str2;
            var actualValue = str1 + 'b' + str2;
            // Assert.Equal('"' + expectedValue + '"', '"' + actualValue + '"');

            var difference = new JsonDiffNode("/Test", JToken.FromObject(expectedValue), JToken.FromObject(actualValue));
            var actualMessage = difference.ToString();

            var expectedMessage = $@"/Test:
                                  ↓ (pos 20)
    Expected: ""0123456789012345678a012345678901234567890123456789012345678""
    Actual:   ""0123456789012345678b012345678901234567890123456789012345678""
                                  ↑ (pos 20)";
            Assert.Equal(expectedMessage, actualMessage, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void ToString_LongStringValues_DifferenceAtStart()
        {
            var expectedValue = string.Join(string.Empty, Enumerable.Range(0, 512));
            var actualValue = string.Join(string.Empty, Enumerable.Range(1, 512));
            // Assert.Equal('"' + expectedValue + '"', '"' + actualValue + '"');

            var difference = new JsonDiffNode("/Test", JToken.FromObject(expectedValue), JToken.FromObject(actualValue));
            var actualMessage = difference.ToString();

            var expectedMessage = $@"/Test:
               ↓ (pos 1)
    Expected: ""01234567891011121314151617181920212223242…
    Actual:   ""12345678910111213141516171819202122232425…
               ↑ (pos 1)";
            Assert.Equal(expectedMessage, actualMessage, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void ToString_LongStringValues_DifferenceAtEnd()
        {
            var expectedValue = string.Join(string.Empty, Enumerable.Range(0, 512)) + "ab";
            var actualValue = string.Join(string.Empty, Enumerable.Range(0, 512)) + "ac";
            // Assert.Equal('"' + expectedValue + '"', '"' + actualValue + '"');

            var difference = new JsonDiffNode("/Test", JToken.FromObject(expectedValue), JToken.FromObject(actualValue));
            var actualMessage = difference.ToString();

            var expectedMessage = $@"/Test:
                                   ↓ (pos 1428)
    Expected: …5506507508509510511ab""
    Actual:   …5506507508509510511ac""
                                   ↑ (pos 1428)";
            Assert.Equal(expectedMessage, actualMessage, ignoreLineEndingDifferences: true);
        }
    }
}
