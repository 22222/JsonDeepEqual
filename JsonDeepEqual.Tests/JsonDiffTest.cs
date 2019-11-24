using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Two.JsonDeepEqual
{
    public class JsonDiffTest
    {
        [Fact]
        public void EnumerateJTokenDifferences_DirectCircularReferenceObjects_NoDifferences()
        {
            var expected = CreateDirectCircularReferenceObject();
            var actual = CreateDirectCircularReferenceObject();
            var differences = JsonDiff.EnumerateDifferences(expected, actual).ToList();
            Assert.Empty(differences);
        }

        private static JObject CreateDirectCircularReferenceObject()
        {
            var o = new JObject();
            o.Add(new JProperty("self", o));
            return o;
        }

        [Fact]
        public void EnumerateJTokenDifferences_ParentChildCircularReferenceObjects_NoDifferences()
        {
            var expected = CreateParentChildCircularReferenceObject();
            var actual = CreateParentChildCircularReferenceObject();
            var differences = JsonDiff.EnumerateDifferences(expected, actual).ToList();
            Assert.Empty(differences);
        }

        private static JObject CreateParentChildCircularReferenceObject()
        {
            var parentObject = new JObject();
            parentObject.Add(new JProperty("name", "parent"));

            var childObject = new JObject();
            childObject.Add(new JProperty("name", "child"));

            parentObject.Add(new JProperty("child", childObject));
            childObject.Add(new JProperty("parent", parentObject));

            return parentObject;
        }
    }
}
