using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Specialized;
using DotJson;

namespace DotJson.Tests
{
    
    
    /// <summary>
    ///This is a test class for ServiceExtensionsTests and is intended
    ///to contain all ServiceExtensionsTests Unit Tests
    ///</summary>
    [TestClass()]
    public class ServiceExtensionsTests
    {

        /// <summary>
        ///A test for GetPropValueCollection
        ///</summary>
        [TestMethod()]
        public void GetPropValueCollection_ShouldConvert_BasicNameValuePair()
        {
            var o = new { name = "value", name2 = "value2" };
            var col = o.ToNameValueCollection();

            Assert.IsNotNull(col);
            Assert.AreEqual(2, col.Count);
            Assert.AreEqual("name", col.Keys[0]);
            Assert.AreEqual("value", col[0]);
            Assert.AreEqual("name2", col.Keys[1]);
            Assert.AreEqual("value2", col[1]);
        }
    }
}
