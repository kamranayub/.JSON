using DotJson;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Security;

namespace DotJson.Tests
{
    /// <summary>
    ///This is a test class for JsonServiceTests and is intended
    ///to contain all JsonServiceTests Unit Tests
    ///</summary>
    [TestClass()]
    public class JsonServiceTests
    {

        private Uri _uri = new Uri("https://github.com/api/v2/json/");

        /// <summary>
        ///A test for GET
        ///</summary>
        [TestMethod()]
        public void JsonService_GET_ShouldReturn_Value_ForGitHubService()
        {
            var svc = new JsonService(_uri);
            var json = svc.GET("repos/show/kamranayub");

            CheckRepositoriesResult(json);
        }

        /// <summary>
        ///A test for GET
        ///</summary>
        [TestMethod()]
        public void JsonService_GET_ShouldAccept_BasicAuth_Credentials()
        {
            var svc = new JsonService(_uri);

            // Setup
            // For GitHub, need to forcefully send the authorization header
            svc.AddBasicAuth("kamranayub/token", 
                System.IO.File.ReadAllText("../../../git_token.private"), true);           

            var json = svc.GET("user/show/kamranayub");

            Assert.IsNotNull(json);
            Assert.IsNotNull(json.user);
            Assert.IsNotNull(json.user.total_private_repo_count);
        }

        /// <summary>
        ///A test for POST
        ///</summary>
        [TestMethod()]
        public void JsonService_POST_ShouldReturn_Value_ForGitHubService()
        {
            var svc = new JsonService(_uri);
            var json = svc.POST("repos/show/kamranayub", new { foo = "bar" });

            CheckRepositoriesResult(json);

        }

        private void CheckRepositoriesResult(dynamic json)
        {
            Assert.IsNotNull(json, "Result is null");
            Assert.IsNotNull(json.repositories, "No repositories key");

            // Check data types
            Assert.IsTrue(json.repositories is Array, "Repositories is not an array");
            Assert.IsTrue(json.repositories[0].name is string, "Repositories[0].name is not a string");
            Assert.IsTrue(json.repositories[0].watchers is int, "Repositories[0].watchers is not an int");
            Assert.IsTrue(json.repositories[0].@private is bool, "Repositories[0].private is not a bool");
            Assert.IsTrue(json.repositories[0].created_at is DateTime, "Repositories[0].created_at is not a DateTime");
            Assert.IsTrue(json.repositories[0].url is Uri, "Repositories[0].url is not a URI");
        }
    }
}
