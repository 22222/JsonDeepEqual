A .NET library for comparing values based on their JSON representation.


Overview
========
This library compares values for deep equality, which it defines as:

> Two objects are equal if they have the same JSON representation when serialized by [Json.NET](https://github.com/JamesNK/Newtonsoft.Json).

That means all of the hard work is done by the Json.NET library.  And Json.NET actually provides a [JToken.DeepEquals method](https://www.newtonsoft.com/json/help/html/DeepEquals.htm) that you could use directly.

So what does this library have to offer?  It provides additional features that are specific to comparing two values, including:

- Detailed descriptions of any detected differences
- Comparison options (exclude properties, ignore array order, etc.)
- Convenience methods that accept objects as parameters instead of JToken values


Installation
============
You have a few options for installing this library:

- Install the [NuGet package](https://www.nuget.org/packages/Two.JsonDeepEqual/)
- Download the assembly from the [latest release](https://github.com/22222/JsonDeepEqual/releases/latest) and reference it manually
- Copy the source code directly into your project

This project is available under either of two licenses: [MIT](LICENSE) or [The Unlicense](UNLICENSE).  The goal is to allow you to copy any of the source code from this library into your own project without having to worry about attribution or any other licensing complexity.


Getting Started
===============
You can use the the `JsonDeepEqualAssert.Equal` static method to compare two objects for equality:

```c#
using Two.JsonDeepEqual;

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
JsonDeepEqualAssert.Equal(expected, actual);
```

That example throws an exception with a message like:

```text
JsonDeepEqualAssert.Equal() Failure: 3 differences
/Message:
                    ↓ (pos 6)
    Expected: "Hello!"
    Actual:   "Hello, World!"
                    ↑ (pos 6)
/Child/Id:
    Expected: 1
    Actual:   2
/Child/Values/1:
    Expected: 2
    Actual:   4
```

You can provide options that change how the objects are compared:

```c#
using Two.JsonDeepEqual;

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
```

There's a similar `NotEqual` method that throws an exception when the objects are equal:

```c#
JsonDeepEqualAssert.NotEqual(expected, actual);
```

You can use the `JsonAssert.Equal` static method to compare JSON strings or JToken values directly:

```c#
var expectedJson = @"{ ""a"":1 }";
var actualJson = @"{ ""a"":2 }";
JsonAssert.Equal(expectedJson, actualJson);
```

Aliases of `AreEqual` and `AreNotEqual` are available for all of these methods if you want a name that's more consistent with NUnit assert methods:

```c#
JsonDeepEqualAssert.AreEqual(expected, actual);
JsonDeepEqualAssert.AreNotEqual(expected, actual);
```


Options
=======
All of the Equal, NotEqual, and EnumerateDifferences methods accept an optional "options" parameter that gives you more control over the equality comparisons.

Example:

```c#
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
    IgnoreCase = true,
    IgnoreLineEndingDifferences = true,
    IgnoreWhiteSpaceDifferences = true,
});
```

Some options in the `JsonDeepEqualDiffOptions` class only apply to JSON serialization for object comparisons:

- ExcludePropertyNames - Property names to ignore in the equality comparison.  The names can contain glob-style wildcards: `*` to match any characters, `?` to match a single character.
- PropertyFilter - A custom function to choose the `JsonProperty` values that are included in the equality comparison.  Use this if you need more control over the filtering than the "ExcludePropertyNames" option provides.
- NullValueHandling - How null values are serialized to JSON.
- DefaultValueHandling - How default values are serialized to JSON (such as `0` for integers, `""` for strings, etc.).
- ReferenceLoopHandling - How to handle circular references during JSON serialization.
- DateFormatString - A custom DateTime format string that specifies how `DateTime` and `DateTimeOffset` values are serialized
- DateTimeConverter - A custom function to adjust DateTime and DateTimeOffset values before serialization.  This can be used to control how milliseconds are truncated or rounded, for example.
- JsonAttributeHandling - Whether to respect Json.NET attributes like `JsonPropertyAttribute` and `JsonIgnoreAttribute` during JSON serialization.
- JsonConverters - Any custom `JsonConverter` objects to use during JSON serialization.

Some options in `JsonDiffOptions` class apply to any type of JSON comparison:

- ExcludePropertyPaths - Property paths to ignore in the equality comparison using [JSON pointer notation](https://tools.ietf.org/html/rfc6901).  The paths can contain glob-style wildcards: `*` to match any property name, `**` to match any path, `?` to match a single character in a property name.
- PropertyPathFilter - A custom function to choose the property paths that are included in the equality comparison.  Use this if you need more control over the filtering than the "ExcludePropertyPaths" option provides.
- IgnoreArrayElementOrder - Set to True if you want to treat two collections with the same elements in any order as equal.
- IgnoreCase - Set to true if you want to use case-insensitive comparison for all string values.
- IgnoreLineEndingDifferences - Set to true if you want to treat `\r\n`, `\r`, and `\n` as equivalent in all string values. 
- IgnoreWhiteSpaceDifferences - Set to true if you want to treat any number of consecutive whitespace characters as equivalent in all string values.


Differences
===========
The main focus of this library is the Assert classes, but there are also methods that will return a list of differences instead of throwing exceptions.

You can use the `JsonDeepEqualDiff.EnumerateDifferences` static method to compare two objects:

```c#
var expected = new { Message = "Hello!" };
var actual = new { Message = "Hello, World!" };

IEnumerable<JsonDiffNode> differences = JsonDeepEqualDiff.EnumerateDifferences(expected, actual);
Console.WriteLine(string.Join(Environment.NewLine, differences.Take(10)));

```

You can use the static `JsonDiff.EnumerateDifferences` method to compare JSON strings or JToken values directly:

```c#
var expectedJson = @"{ ""a"":1 }";
var actualJson = @"{ ""a"":2 }";

IEnumerable<JsonDiffNode> differences = JsonDiff.EnumerateDifferences(expectedJson, actualJson);
Console.WriteLine(string.Join(Environment.NewLine, differences.Take(10)));
```

These Diff methods all support the same options as the Assert method equivalents.
