using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Two.JsonDeepEqual.Exceptions;
using Xunit;

namespace Two.JsonDeepEqual
{
    public class JsonDeepEqualAssertTest
    {
        [Theory]
        [InlineData(2, 2, true)]
        [InlineData(2, 1, false)]
        [InlineData(2, 2f, false)]
        [InlineData(0f, 0f, true)]
        [InlineData(0f, float.Epsilon * 2, true)]
        [InlineData(0d, 0d, true)]
        [InlineData(0d, double.Epsilon * 2, true)]
        [InlineData("Hello world!", "Hello world!", true)]
        [InlineData("Hello World!", "Hello world!", false)]
        [InlineData("hello", "world", false)]
        [InlineData(2, null, false)]
        [InlineData("test", null, false)]
        [InlineData(null, null, true)]
        public void Equal_SimpleValues(object a, object b, bool expected)
        {
            if (expected)
            {
                AssertDeepEqual(a, b);
                AssertDeepEqual(b, a);
            }
            else
            {
                AssertNotDeepEqual(a, b);
                AssertNotDeepEqual(b, a);
            }
        }

        [Fact]
        public void Equal_ByteArrayWithSelf()
        {
            var a = new byte[] { 1, 171, 128, 3 };
            AssertDeepEqual(a, a);
        }

        [Fact]
        public void Equal_ByteArrays()
        {
            var a = new byte[] { 1, 171, 128, 3 };
            var b = new byte[] { 1, 171, 128, 3 };
            AssertDeepEqual(a, b);
        }

        [Fact]
        public void NotEqual_ByteArrays()
        {
            var a = new byte[] { 1, 171, 128, 3 };
            var b = new byte[] { 1, 171, 128, 2 };
            AssertNotDeepEqual(a, b);
        }

        [Fact]
        public void Equal_FloatsAlmostEqual()
        {
            var a = 0f;
            var b = float.Epsilon / 2;
            AssertDeepEqual(a, b);
        }

        [Fact]
        public void Equal_DoublesAlmostEqual()
        {
            var a = 0d;
            var b = double.Epsilon / 2;
            AssertDeepEqual(a, b);
        }

        [Fact]
        public void Equal_SameEmptyObjects()
        {
            var obj = new object();
            JsonDeepEqualAssert.Equal(obj, obj);
        }

        [Fact]
        public void Equal_EmptyObjects()
        {
            JsonDeepEqualAssert.Equal(new object(), new object());
        }

        [Fact]
        public void Equal_SameString()
        {
            JsonDeepEqualAssert.Equal("2", "2");
        }

        [Fact]
        public void NotEqual_DifferentStrings()
        {
            JsonDeepEqualAssert.NotEqual("1", "2");
        }

        #region Messages

        [Fact]
        public void Equal_DifferentStrings_ShouldThrowExceptionWithExpectedMessage()
        {
            var expected = "Test hello";
            var actual = "Test world";
            var actualException = Assert.Throws<JsonEqualException>(() => JsonDeepEqualAssert.Equal(expected, actual));

            var expectedMessage = @"JsonDeepEqualAssert.Equal() Failure: 1 difference
                    ↓ (pos 6)
    Expected: ""Test hello""
    Actual:   ""Test world""
                    ↑ (pos 6)";
            Assert.Equal(expectedMessage, actualException.Message, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void Equal_DifferentAnonymousObjects_ShouldThrowExceptionWithExpectedMessage()
        {
            var expected = new { id = 1, message = "hello" };
            var actual = new { id = 2, message = "world" };
            var actualException = Assert.Throws<JsonEqualException>(() => JsonDeepEqualAssert.Equal(expected, actual));

            var expectedMessage = @"JsonDeepEqualAssert.Equal() Failure: 2 differences
/id:
    Expected: 1
    Actual:   2
/message:
               ↓ (pos 1)
    Expected: ""hello""
    Actual:   ""world""
               ↑ (pos 1)";
            Assert.Equal(expectedMessage, actualException.Message, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void Equal_DifferentCustomObjects_ShouldThrowExceptionWithExpectedMessage()
        {
            var expected = new Company { Id = 1, Name = "hello" };
            var actual = new Company { Id = 2, Name = "world" };
            var actualException = Assert.Throws<JsonEqualException>(() => JsonDeepEqualAssert.Equal(expected, actual));

            var expectedMessage = @"JsonDeepEqualAssert.Equal() Failure: 2 differences
/Id:
    Expected: 1
    Actual:   2
/Name:
               ↓ (pos 1)
    Expected: ""hello""
    Actual:   ""world""
               ↑ (pos 1)";
            Assert.Equal(expectedMessage, actualException.Message, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void Equal_DifferentIntLists_ShouldThrowExceptionWithExpectedMessage()
        {
            var expected = new[] { 0, 1, 2 };
            var actual = new[] { 1, 2 };
            var actualException = Assert.Throws<JsonEqualException>(() => JsonDeepEqualAssert.Equal(expected, actual));

            var expectedMessage = @"JsonDeepEqualAssert.Equal() Failure: 3 differences
/0:
    Expected: 0
    Actual:   1
/1:
    Expected: 1
    Actual:   2
/2:
    Expected: 2
    Actual:   null";
            Assert.Equal(expectedMessage, actualException.Message, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void Equal_DifferentObjects_LongValues_ShouldThrowExceptionWithExpectedMessage()
        {
            var obj1 = new Person
            {
                Id = 1,
                FullName = string.Join(string.Empty, Enumerable.Range(0, 512)),
            };
            var obj2 = new Person
            {
                Id = 2,
                FullName = string.Join(string.Empty, Enumerable.Range(0, 256)) + string.Join(string.Empty, Enumerable.Range(1, 256)),
            };
            var actualException = Assert.Throws<JsonEqualException>(() => JsonDeepEqualAssert.Equal(obj1, obj2));

            var expectedMessage = @"JsonDeepEqualAssert.Equal() Failure: 2 differences
/Id:
    Expected: 1
    Actual:   2
/FullName:
                                   ↓ (pos 659)
    Expected: …4925025125225325425525625725825926026126226326426526626726826…
    Actual:   …4925025125225325425512345678910111213141516171819202122232425…
                                   ↑ (pos 659)";
            Assert.Equal(expectedMessage, actualException.Message, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void Equal_LotsOfDifferences_ShouldThrowExceptionWithExpectedMessage()
        {
            var obj1 = new ListTestClass<int>
            {
                List = Enumerable.Range(0, 512).ToList(),
            };
            var obj2 = new ListTestClass<int>
            {
                List = Enumerable.Range(1, 512).ToList(),
            };
            var actualException = Assert.Throws<JsonEqualException>(() => JsonDeepEqualAssert.Equal(obj1, obj2));

            var expectedStartMessage = @"JsonDeepEqualAssert.Equal() Failure: 20+ differences
/List/0:
    Expected: 0
    Actual:   1
/List/1:
    Expected: 1
    Actual:   2";
            Assert.StartsWith(
                expectedStartMessage.Replace("\r", string.Empty, StringComparison.Ordinal),
                actualException.Message.Replace("\r", string.Empty, StringComparison.Ordinal),
                StringComparison.Ordinal
            );
        }

        [Fact]
        public void NotEqual_SameString_ShouldThrowExceptionWithExpectedMessage()
        {
            var expected = "hello";
            var actual = "hello";
            var actualException = Assert.Throws<JsonNotEqualException>(() => JsonDeepEqualAssert.NotEqual(expected, actual));

            var expectedMessage = "JsonDeepEqualAssert.NotEqual() Failure";
            Assert.Equal(expectedMessage, actualException.Message, ignoreLineEndingDifferences: true);
        }

        #endregion

        [Fact]
        public void Equal_EqualAddresses()
        {
            var a = CreateSampleAddress();
            var b = CreateSampleAddress();
            AssertDeepEqual(a, b);
        }

        [Fact]
        public void Equal_DifferentAddresses_ShouldThrowException()
        {
            var a = new Address
            {
                Id = 1,
                Lines = new[] { "123 Fake ST", "Arlington, VA 22222" },
            };
            var b = new Address
            {
                Id = 1,
                Lines = new[] { "321 Fake ST", "Arlington, VA 22222" },
            };
            AssertNotDeepEqual(a, b);
            AssertDeepEqual(a, b, new JsonDeepEqualDiffOptions
            {
                ExcludePropertyNames = new[] { nameof(Address.Lines) },
            });

            var actualException = Assert.Throws<JsonEqualException>(() => JsonDeepEqualAssert.Equal(a, b));
            Assert.Single(actualException.Differences);
            Assert.Equal("/Lines/0", actualException.Differences.ElementAt(0).Path);
            Assert.Equal("\"123 Fake ST\"", actualException.Differences.ElementAt(0).ExpectedValueDisplay);
            Assert.Equal("\"321 Fake ST\"", actualException.Differences.ElementAt(0).ActualValueDisplay);
        }

        [Fact]
        public void Equal_EqualCompanies()
        {
            var a = CreateSampleCompany();
            var b = CreateSampleCompany();
            AssertDeepEqual(a, b);
        }

        [Fact]
        public void Equal_DifferentCompanies_ShouldThrowException()
        {
            var a = CreateSampleCompany();
            var b = CreateSampleCompany();
            b.Employees.First().FullName = "Robert Plant";

            Assert.NotEqual(a, b);
            AssertDeepEqual(a, b, new JsonDeepEqualDiffOptions
            {
                ExcludePropertyPaths = new[] { "/Employees/**" },
            });
            AssertDeepEqual(a, b, new JsonDeepEqualDiffOptions
            {
                ExcludePropertyPaths = new[] { "/Employees/*/FullName" },
            });
            AssertDeepEqual(a, b, new JsonDeepEqualDiffOptions
            {
                ExcludePropertyPaths = new[] { "**Employees/*/FullName" },
            });
            AssertDeepEqual(a, b, new JsonDeepEqualDiffOptions
            {
                ExcludePropertyNames = new[] { "FullName" },
            });
            AssertDeepEqual(a, b, new JsonDeepEqualDiffOptions
            {
                ExcludePropertyNames = new[] { "*Name*" },
            });
            AssertDeepEqual(a, b, new JsonDeepEqualDiffOptions
            {
                ExcludePropertyPaths = new[] { "Employees/0/*Name*" },
            });

            var actualException = Assert.Throws<JsonEqualException>(() => JsonDeepEqualAssert.Equal(a, b));
            Assert.Single(actualException.Differences);
            Assert.Equal("/Employees/0/FullName", actualException.Differences.ElementAt(0).Path);
            Assert.Equal("\"Robert Paulson\"", actualException.Differences.ElementAt(0).ExpectedValueDisplay);
            Assert.Equal("\"Robert Plant\"", actualException.Differences.ElementAt(0).ActualValueDisplay);
        }

        [Fact]
        public void Equal_DifferentCompaniesWithPrivateGetters()
        {
            var a = new CompanyPrivateGetters
            {
                Id = 1,
                Name = "The Company",
            };
            var b = new CompanyPrivateGetters
            {
                Id = 1,
                Name = "A Company",
            };
            AssertDeepEqual(a, b);
        }

        [Fact]
        public void Equal_EqualPeople_Cycle()
        {
            var a = CreateSamplePerson_OwnFatherAndMotherSomehow();
            var b = CreateSamplePerson_OwnFatherAndMotherSomehow();
            AssertDeepEqual(a, b);
        }

        [Fact]
        public void NotEqual_DifferentPeople_PartialCycle()
        {
            var a = CreateSamplePerson_OwnFatherAndMotherSomehow();
            var b = CreateSamplePerson_OwnFatherAndMotherSomehow();
            b.Mother = a;
            AssertNotDeepEqual(a, b);
        }

        [Fact]
        public void Equal_EqualPeople_OwnGrandpa()
        {
            var a = CreateSamplePerson_OwnGrandpa();
            var b = CreateSamplePerson_OwnGrandpa();
            AssertDeepEqual(a, b);
        }

        [Fact]
        public void NotEqual_DifferentPeople_OwnGrandpa()
        {
            var a = CreateSamplePerson_OwnGrandpa();
            var b = CreateSamplePerson_OwnGrandpa();
            b.Father!.FullName = "Bob";

            AssertNotDeepEqual(a, b);
        }

        [Fact]
        public void Equal_EmptyListTestObjectsWithSameType()
        {
            var obj1 = new ListTestClass<string>();
            var obj2 = new ListTestClass<string>();
            AssertDeepEqual(obj1, obj2);
        }

        [Fact]
        public void Equal_ListTestObjectSamples()
        {
            var obj1 = new ListTestClass<string>()
            {
                Enumerable = new[] { "test" },
                ReadOnlyCollection = new[] { "test2", "test22" },
                Collection = new[] { "test3" },
                Array = new[] { "test4" },
                IList = new[] { "test5" },
                List = new List<string> { "test6" },
            };
            var obj2 = new ListTestClass<string>()
            {
                Enumerable = new[] { "test" },
                ReadOnlyCollection = new[] { "test2", "test22" },
                Collection = new[] { "test3" },
                Array = new[] { "test4" },
                IList = new[] { "test5" },
                List = new List<string> { "test6" },
            };
            AssertDeepEqual(obj1, obj2);
        }

        [Fact]
        public void Equal_ListTestObjectsSamples_DifferentListTypes()
        {
            var obj1 = new ListTestClass<string>()
            {
                Enumerable = new[] { "test" },
                ReadOnlyCollection = new[] { "test2", "test22" },
                Collection = new[] { "test3" },
                Array = new[] { "test4" },
                IList = new[] { "test5" },
                List = new List<string> { "test6" },
            };
            var obj2 = new ListTestClass<string>()
            {
                Enumerable = new List<string> { "test" },
                ReadOnlyCollection = new List<string> { "test2", "test22" },
                Collection = new List<string> { "test3" },
                Array = new[] { "test4" },
                IList = new List<string> { "test5" },
                List = new List<string> { "test6" },
            };
            AssertDeepEqual(obj1, obj2);
        }

        [Fact]
        public void NotEqual_DifferentListTestObjectsSamples()
        {
            var obj1 = new ListTestClass<string>()
            {
                Enumerable = new[] { "test" },
                ReadOnlyCollection = new[] { "test2", "test22" },
                Collection = new[] { "test3" },
                Array = new[] { "test4" },
                IList = new[] { "test5" },
                List = new List<string> { "test6" },
            };
            var obj2 = new ListTestClass<string>()
            {
                Enumerable = new[] { "TEST" },
                ReadOnlyCollection = new[] { "TEST2", "TEST22" },
                Collection = new[] { "TEST3" },
                Array = new[] { "TEST4" },
                IList = new[] { "TEST5" },
                List = new List<string> { "TEST6" },
            };
            AssertNotDeepEqual(obj1, obj2);
        }

        [Fact]
        public void Equal_EmptyListTestObjectsWithDifferentTypes()
        {
            var obj1 = new ListTestClass<string>();
            var obj2 = new ListTestClass<int>();
            AssertDeepEqual(obj1, obj2);
        }

        [Fact]
        public void Equal_EmptyListTestObjectsWithDifferentCompatibleTypes()
        {
            var obj1 = new ListTestClass<Person>();
            var obj2 = new ListTestClass<Employee>();
            AssertDeepEqual(obj1, obj2);
        }

        [Fact]
        public void Equal_EmptyDictionaryTestObjectsWithSameType()
        {
            var obj1 = new DictionaryTestClass<int, Person>();
            var obj2 = new DictionaryTestClass<int, Person>();
            AssertDeepEqual(obj1, obj2);
        }

        [Fact]
        public void Equal_EmptyDictionariesWithSameType()
        {
            var obj1 = new DictionaryTestClass<int, Person>
            {
                Dictionary = new Dictionary<int, Person>(),
                DictionaryOfEnumerables = new Dictionary<int, IEnumerable<Person>>(),
                DictionaryOfCollections = new Dictionary<int, IReadOnlyCollection<Person>>(),
            };
            var obj2 = new DictionaryTestClass<int, Person>()
            {
                Dictionary = new Dictionary<int, Person>(),
                DictionaryOfEnumerables = new Dictionary<int, IEnumerable<Person>>(),
                DictionaryOfCollections = new Dictionary<int, IReadOnlyCollection<Person>>(),
            };
            AssertDeepEqual(obj1, obj2);
        }

        [Fact]
        public void Equal_DictionariesWithSameValuesButDifferentListTypes_()
        {
            var person = new Person { FullName = "Robert Paulson" };
            var obj1 = new DictionaryTestClass<int, Person>
            {
                Dictionary = new Dictionary<int, Person>() { [1] = person },
                DictionaryOfEnumerables = new Dictionary<int, IEnumerable<Person>>() { [1] = new Person[] { person } },
                DictionaryOfCollections = new Dictionary<int, IReadOnlyCollection<Person>>() { [1] = new Person[] { person } },
            };
            var obj2 = new DictionaryTestClass<int, Person>()
            {
                Dictionary = new Dictionary<int, Person>() { [1] = person },
                DictionaryOfEnumerables = new Dictionary<int, IEnumerable<Person>>() { [1] = new List<Person> { person } },
                DictionaryOfCollections = new Dictionary<int, IReadOnlyCollection<Person>>() { [1] = new List<Person> { person } },
            };
            AssertDeepEqual(obj1, obj2);
        }

        [Fact]
        public void Equal_DictionaryTestObjectsWithEqualValuesButDifferentListTypes()
        {
            var person1 = new Person { FullName = "Robert Paulson" };
            var person2 = new Person { FullName = "Robert Paulson" };

            var obj1 = new DictionaryTestClass<int, Person>
            {
                Dictionary = new Dictionary<int, Person>() { [1] = person1 },
                DictionaryOfEnumerables = new Dictionary<int, IEnumerable<Person>>() { [1] = new Person[] { person1 } },
                DictionaryOfCollections = new Dictionary<int, IReadOnlyCollection<Person>>() { [1] = new Person[] { person1 } },
            };
            var obj2 = new DictionaryTestClass<int, Person>()
            {
                Dictionary = new Dictionary<int, Person>() { [1] = person2 },
                DictionaryOfEnumerables = new Dictionary<int, IEnumerable<Person>>() { [1] = new List<Person> { person2 } },
                DictionaryOfCollections = new Dictionary<int, IReadOnlyCollection<Person>>() { [1] = new List<Person> { person2 } },
            };

            AssertDeepEqual(obj1, obj2);
        }

        [Fact]
        public void NotEqual_ReflectionPropertiesObjectSample()
        {
            var obj1 = new ReflectionValuesTestClass()
            {
                Type = typeof(string),
                PropertyInfo = typeof(string).GetRuntimeProperty(nameof(string.Length)),
            };
            var obj2 = new ReflectionValuesTestClass()
            {
                Type = typeof(ReflectionValuesTestClass),
                PropertyInfo = typeof(ReflectionValuesTestClass).GetRuntimeProperty(nameof(ReflectionValuesTestClass.PropertyInfo)),
            };
            AssertNotDeepEqual(obj1, obj2);
        }

        [Fact]
        public void Equal_EmptyArrays()
        {
#pragma warning disable CA1825 // Avoid zero-length array allocations.
            var obj1 = new string[0];
            var obj2 = new string[0];
#pragma warning restore CA1825 // Avoid zero-length array allocations.
            AssertDeepEqual(obj1, obj2);
        }

        [Fact]
        public void Equal_PrimitiveValuesObjectSample()
        {
            var obj1 = PrimitiveValuesTestClass.CreateSample();
            var obj2 = PrimitiveValuesTestClass.CreateSample();
            AssertDeepEqual(obj1, obj2);
        }

        [Fact]
        public void Equal_Arrays_IgnoreOptions()
        {
            var expected = new[] { 1, 2, 3 };
            var actual = new[] { 3, 2, 1 };
            AssertNotDeepEqual(expected, actual);
            AssertDeepEqual(expected, actual, new JsonDeepEqualDiffOptions { IgnoreArrayElementOrder = true });
        }

        [Fact]
        public void Equal_Strings_IgnoreOptions()
        {
            var expected = "Hell o\nWorld";
            var actual = "hell  o\r\nworld";
            AssertNotDeepEqual(expected, actual);
            AssertDeepEqual(expected, actual, new JsonDeepEqualDiffOptions { IgnoreCase = true, IgnoreWhiteSpaceDifferences = true, IgnoreLineEndingDifferences = true });
        }

        #region Helpers

        private void AssertDeepEqual(object? expected, object? actual)
        {
            JsonDeepEqualAssert.Equal(expected, actual);
            JsonDeepEqualAssert.AreEqual(expected, actual);
            Assert.Throws<JsonNotEqualException>(() => JsonDeepEqualAssert.NotEqual(expected, actual));
            Assert.Throws<JsonNotEqualException>(() => JsonDeepEqualAssert.AreNotEqual(expected, actual));
            AssertDeepEqual(expected, actual, null);
        }

        private static void AssertDeepEqual(object? expected, object? actual, JsonDeepEqualDiffOptions? options)
        {
            JsonDeepEqualAssert.Equal(expected, actual, options);
            JsonDeepEqualAssert.AreEqual(expected, actual, options);
            Assert.Throws<JsonNotEqualException>(() => JsonDeepEqualAssert.NotEqual(expected, actual, options));
            Assert.Throws<JsonNotEqualException>(() => JsonDeepEqualAssert.AreNotEqual(expected, actual, options));
        }

        private void AssertNotDeepEqual(object? expected, object? actual)
        {
            JsonDeepEqualAssert.NotEqual(expected, actual);
            JsonDeepEqualAssert.AreNotEqual(expected, actual);
            Assert.Throws<JsonEqualException>(() => JsonDeepEqualAssert.Equal(expected, actual));
            Assert.Throws<JsonEqualException>(() => JsonDeepEqualAssert.AreEqual(expected, actual));
            AssertNotDeepEqual(expected, actual, null);
        }

        private static void AssertNotDeepEqual(object? expected, object? actual, JsonDeepEqualDiffOptions? options)
        {
            JsonDeepEqualAssert.NotEqual(expected, actual, options);
            JsonDeepEqualAssert.AreNotEqual(expected, actual, options);
            Assert.Throws<JsonEqualException>(() => JsonDeepEqualAssert.Equal(expected, actual, options));
            Assert.Throws<JsonEqualException>(() => JsonDeepEqualAssert.AreEqual(expected, actual, options));
        }

        #endregion

        #region Samples

        private Address CreateSampleAddress()
        {
            var address = new Address
            {
                Id = 1,
                Lines = new[] { "123 Fake ST", "Arlington, VA 22222" },
            };
            return address;
        }

        private Company CreateSampleCompany()
        {
            var company = new Company
            {
                Id = 1,
                Name = "The Company",
                Employees = new[]
                {
                    new Employee
                    {
                        Id = 2,
                        FullName = "Robert Paulson",
                        Addresses = new[]
                        {
                            new Address { Id = 3, AddressType = AddressType.Home, Lines = new[] { "123 Fake ST", "Arlington, VA 22222" } },
                            new Address { Id = 4, AddressType = AddressType.Work, Lines = new[] { "2 Company BLVD", "Arlington, VA 22222" } },
                        },
                        Phones = new[]
                        {
                            new Phone { Id = 5, PhoneType = PhoneType.Cell, Number = "555-555-5555", },
                        },
                    },
                    new Employee
                    {
                        Id = 6,
                        FullName = "Jenny Heath",
                        Phones = new[]
                        {
                            new Phone { Id = 7, PhoneType = PhoneType.Home, Number = "555-867-5309", },
                        },
                    },
                },
            };
            return company;
        }

        private Person CreateSamplePerson_OwnFatherAndMotherSomehow()
        {
            var person = new Person { FullName = "a" };
            person.Father = person;
            person.Mother = person;
            person.Children = new[] { person };
            return person;
        }

        private Person CreateSamplePerson_OwnGrandpa()
        {
            var protagonistFather = new Person { FullName = "Protagonist's Father" };
            var protagonistMother = new Person { FullName = "Protagonist's Mother" };
            var protagonist = new Person { FullName = "Protagonist", Father = protagonistFather, Mother = protagonistMother };

            var widow = new Person { FullName = "Widow" };
            var deadMan = new Person { FullName = "Dead Man " };
            var widowDaughter = new Person { FullName = "Widow's Daughter", Father = deadMan, Mother = widow };

            var protagonistBaby = new Person { FullName = "Protagonist's Baby", Father = protagonist, Mother = widow };
            var protagonistStepBrotherAndStepGrandchild = new Person { FullName = "Grandchild", Father = protagonistFather, Mother = widowDaughter };

            protagonist.Spouses = new[] { widow };
            widow.Spouses = new[] { deadMan, protagonist };
            deadMan.Spouses = new[] { widow };
            protagonistMother.Spouses = new[] { protagonistFather };
            protagonistFather.Spouses = new[] { protagonistMother, widowDaughter };
            widowDaughter.Spouses = new[] { protagonistFather };

            protagonist.Children = new[] { protagonistBaby };
            protagonistFather.Children = new[] { protagonist, protagonistStepBrotherAndStepGrandchild };
            protagonistMother.Children = new[] { protagonist };
            widow.Children = new[] { widowDaughter, protagonistBaby };
            widowDaughter.Children = new[] { protagonistStepBrotherAndStepGrandchild };

            return protagonist;
        }

        #endregion
    }
}
