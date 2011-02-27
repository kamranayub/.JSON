using DotJson;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Security;
using System.Collections.Generic;

namespace DotJson.Tests
{
    /// <summary>
    ///This is a test class for JsonServiceTests and is intended
    ///to contain all JsonServiceTests Unit Tests
    ///</summary>
    [TestClass()]
    public class JsonServiceTests
    {

        private string _uri = "http://github.com/api/v2/json/";

        /// <summary>
        ///A test for GET
        ///</summary>
        [TestMethod()]
        public void JsonService_GET_ShouldReturn_Value_ForGitHubService()
        {
            var json = JsonService.For(_uri).Get("repos/show/kamranayub");

            CheckRepositoriesResult(json);

            CheckRepositoriesResult(JsonService.GetFrom("https://github.com/api/v2/json/repos/show/kamranayub"));
        }

        /// <summary>
        ///A test for GET
        ///</summary>
        [TestMethod()]
        public void JsonService_GET_ShouldAccept_BasicAuth_Credentials()
        {
            // Setup
            // For GitHub, need to forcefully send the authorization header
            var svc = JsonService.For(_uri)
                        .AuthenticateAsBasic("kamranayub/token", 
                            System.IO.File.ReadAllText("../../../git_token.private"), true);

            var json = svc.Get("user/show/kamranayub");

            Assert.IsNotNull(json);
            Assert.IsNotNull(json.user);
            Assert.IsNotNull(json.user.total_private_repo_count);
        }

        /// <summary>
        ///A test for POST
        ///</summary>
        [TestMethod()]
        public void JsonService_POST_ShouldUpdate_Profile_ForGitHubService()
        {
            var setBlog =  new Dictionary<string, string>() 
            { 
                {"login", "kamranayub"},
                {"token", System.IO.File.ReadAllText("../../../git_token.private")},
                {"values[blog]", "http://intrepidstudios.com/blog/?POST-test" }
            };

            var json = JsonService.PostTo(_uri + "user/show/kamranayub", setBlog);

            Assert.AreEqual("http://intrepidstudios.com/blog/?POST-test", json.user.blog.ToString());

            // Revert
            setBlog["values[blog]"] = "http://intrepidstudios.com/blog/";

            json = JsonService.PostTo(_uri + "user/show/kamranayub", setBlog);

            Assert.AreEqual("http://intrepidstudios.com/blog/", json.user.blog.ToString());
        }

        /// <summary>
        ///A test for POST
        ///</summary>
        [TestMethod()]
        public void JsonService_ShouldSupport_Escaping_URLs()
        {
            var json = JsonService.For("http://marketplace.envato.com/api/v2/")
                            .Get("/new-files-from-user:kayub,codecanyon.json");           

            // If it didn't work, it'd throw an exception
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
