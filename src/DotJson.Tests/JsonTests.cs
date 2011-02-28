using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Specialized;
using DotJson;
using System.Linq;
using DotJson.Tests;
using System.Collections.Generic;

namespace DotJson.Tests
{

    /// <summary>
    ///This is a test class for ServiceExtensionsTests and is intended
    ///to contain all ServiceExtensionsTests Unit Tests
    ///</summary>
    [TestClass()]
    public class DynamicJsonObjectTests
    {

        /// <summary>
        ///A test for Json
        ///</summary>
        [TestMethod()]
        public void Json_ShouldConvert_SimpleJSON_AndReturn_2Properties()
        {
            var json = "{ foo: 'bar', baz: 'whiz' }";

            dynamic x = Json.Parse(json);

            Assert.IsNotNull(x);
            Assert.IsNotNull(x.foo);
            Assert.IsNotNull(x.baz);
            Assert.AreEqual("bar", x.foo);
            Assert.AreEqual("whiz", x.baz);
        }

        /// <summary>
        ///A test for Json
        ///</summary>
        [TestMethod()]
        public void Json_ShouldConvert_SimpleJSON_AndReturn_AllowNullPropertyValues()
        {
            var json = "{ 'baz': null }";

            dynamic x = Json.Parse(json);

            Assert.IsNotNull(x);
            Assert.AreEqual(null, x.baz);
        }

        /// <summary>
        ///A test for Json
        ///</summary>
        [TestMethod()]
        public void Json_ShouldConvert_SimpleJSON_AndReturn_NestedProperty()
        {
            var json = "{ baz: { whiz: 'bang' } }";

            dynamic x = Json.Parse(json);

            Assert.IsNotNull(x);
            Assert.IsNotNull(x.baz);
            Assert.IsNotNull(x.baz.whiz);
            Assert.AreEqual("bang", x.baz.whiz);
        }

        /// <summary>
        ///A test for Json
        ///</summary>
        [TestMethod()]
        public void Json_ShouldConvert_SimpleJSON_AndReturn_Array()
        {
            var json = "{ foo: ['a','b','c'] }";

            dynamic x = Json.Parse(json);

            Assert.IsNotNull(x);
            Assert.IsNotNull(x.foo);
            Assert.AreEqual(3, x.foo.Length);
            Assert.AreEqual("a", x.foo[0]);
            Assert.AreEqual("b", x.foo[1]);
            Assert.AreEqual("c", x.foo[2]);
        }

        /// <summary>
        ///A test for Json
        ///</summary>
        [TestMethod()]
        public void Json_ShouldConvert_SimpleJSON_AndReturn_Array_WithObjects()
        {
            var json = "{ foo: [ { baz: 'whiz' }, { bar: 'bang' } ] }";

            dynamic x = Json.Parse(json);

            Assert.IsNotNull(x);
            Assert.IsNotNull(x.foo);
            Assert.AreEqual(2, x.foo.Length);
            Assert.AreEqual("whiz", x.foo[0].baz);
            Assert.AreEqual("bang", x.foo[1].bar);
        }

        [TestMethod]
        public void Json_ShouldReturn_PropertyName_OriginallyWithDash_ByKey()
        {
            var json = "{ 'foo-bar': 'baz' }";

            dynamic x = Json.Parse(json);

            Assert.IsNotNull(x);
            Assert.IsNotNull(x["foo-bar"]);
            Assert.AreEqual("baz", x["foo-bar"]);
        }

        [TestMethod]
        public void Json_ShouldReturn_PropertyName_OriginallyWithDash_ByCompactForm()
        {
            var json = "{ 'foo-bar': 'baz' }";

            dynamic x = Json.Parse(json);

            Assert.IsNotNull(x);
            Assert.IsNotNull(x.foobar);
            Assert.AreEqual("baz", x.foobar);
        }

        [TestMethod]
        public void Json_ShouldReturn_PropertyNames_UsingCompactForms()
        {
            var json = "{ 'foo-bar': 1, 'Foo-Bar': 2, 'foo-Bar': 3, 'Foo-bar': 4 }";

            dynamic x = Json.Parse(json);

            Assert.IsNotNull(x);

            Assert.IsNotNull(x.foobar);
            Assert.AreEqual(1, x.foobar);

            Assert.IsNotNull(x.FooBar);
            Assert.AreEqual(2, x.FooBar);

            Assert.IsNotNull(x.fooBar);
            Assert.AreEqual(3, x.fooBar);

            Assert.IsNotNull(x.Foobar);
            Assert.AreEqual(4, x.Foobar);
        }

        [TestMethod]
        public void Json_ShouldHandle_CompactForms_With_DuplicateOriginals()
        {
            var json = "{ 'foo-bar': 1, 'foobar': 2 }";

            dynamic x = Json.Parse(json);

            Assert.IsNotNull(x);

            Assert.IsNotNull(x.foobar);
            Assert.AreEqual(2, x.foobar);

            Assert.IsNotNull(x["foo-bar"]);
            Assert.AreEqual(1, x["foo-bar"]);
        }

        [TestMethod]
        public void Json_ShouldHandle_CompactForms_With_DuplicateOriginals2()
        {
            var json = "{ 'foobar': 1, 'foo_bar': 2 }";

            dynamic x = Json.Parse(json);

            Assert.IsNotNull(x);

            Assert.IsNotNull(x.foobar);
            Assert.AreEqual(1, x.foobar);

            Assert.IsNotNull(x["foo_bar"]);
            Assert.AreEqual(2, x["foo_bar"]);
        }

        [TestMethod]
        public void Json_ShouldHandle_Numerical_Keys()
        {
            var json = "{ '111': true }";

            dynamic x = Json.Parse(json);

            Assert.IsNotNull(x);

            Assert.IsNotNull(x["111"]);
            Assert.AreEqual(true, x["111"]);
        }

        [TestMethod]
        public void Json_ToString_ShouldReturn_Original_JSON_String()
        {
            var json = "{\"111\":true,\"foo\":{\"prop1\":1}}";

            dynamic x = Json.Parse(json);

            Assert.IsNotNull(x);

            Assert.AreEqual(json, x.ToString());
            Assert.AreEqual("{\"prop1\":1}", x.foo.ToString());
        }

        [TestMethod]
        public void Json_ShouldHandle_Simple_Anonymous_Objects()
        {
            var jsonObject = new
            {
                foobar = 0,
                baz = 1,
                whiz = 2,
                array = new[] { 
                    'a', 'b', 'c'
                },
                other = new
                {
                    prop1 = true
                }
            };

            dynamic x = Json.Parse(jsonObject);

            Assert.IsNotNull(x);

            Assert.IsNotNull(x.foobar);
            Assert.AreEqual(0, x.foobar);

            Assert.IsNotNull(x.baz);
            Assert.AreEqual(1, x.baz);

            Assert.IsNotNull(x.whiz);
            Assert.AreEqual(2, x.whiz);

            Assert.IsNotNull(x.array);
            Assert.AreEqual(3, x.array.Length);

            Assert.IsNotNull(x.other);
            Assert.AreEqual(true, x.other.prop1);
        }

        [TestMethod]
        public void Json_ShouldAllow_ManipulationOfArrays_With_SimpleLINQ()
        {
            var json = new { array = new object[] { "a", "b", "c", 0 } };
            dynamic x = Json.Parse(json);

            Assert.IsNotNull(x);
            Assert.AreEqual(0, (x.array as object[]).Skip(3).Take(1).First());
        }

        [TestMethod]
        public void Json_ShouldAllow_ManipulationOfArrays_With_ComplexLINQ()
        {
            var json = new
            {
                items = new object[] {
                    new { sales = 5 },
                    new { sales = 20 },
                    new { sales = 8 }
                }
            };

            dynamic[] items = Json.Parse(json).items;

            Assert.IsNotNull(items);
            Assert.AreEqual(20, items.OrderByDescending(f => f.sales).First().sales);
        }
    }
}
