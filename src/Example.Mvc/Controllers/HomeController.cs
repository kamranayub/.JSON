using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DotJson;

namespace Example.Mvc.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GitHubBasicList()
        {
            // Get Kamran's repos
            var repositories = JsonService.GetUrl("https://github.com/api/v2/json/repos/show/kamranayub");

            return View(repositories);
        }

        public ActionResult GitHubBasicAuth(string username, string token)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(username))
            {
                ViewBag.Error = "Enter a user and API token in the URL (e.g. ?username=your_username&token=your_api_token) to test.";
                return View();
            }

            // Connect to GitHub
            var gitUri = new Uri("https://github.com/api/v2/json/");
            var gitService = new JsonService(gitUri);

            // Authenticate to GitHub using OAuth token
            gitService.AddBasicAuth(username + "/token", token, true);

            // Get Schacon
            var user = gitService.GET("user/show").user;

            return View(user);
        }

        public ActionResult GitHubUpdateProfile(string username, string token)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(username))
            {
                ViewBag.Error = "Enter a user and API token in the URL (e.g. ?username=your_username&token=your_api_token) to test.";
                return View();
            }

            var gitParams = new Dictionary<string, string>();

            gitParams["login"] = username;
            gitParams["token"] = token;
            gitParams["values[blog]"] = "http://github.com/" + username;

            // Get old blog
            var oldBlog = JsonService.GetUrl("https://github.com/api/v2/json/user/show/" + username).user.blog;

            // Update to new blog
            var jsonUpdate = JsonService.PostUrl("https://github.com/api/v2/json/user/show/" + username, gitParams);

            // Revert
            gitParams["values[blog]"] = oldBlog.ToString();

            var jsonReverted = JsonService.PostUrl("https://github.com/api/v2/json/user/show/" + username, gitParams);

            return View(new[] { jsonUpdate, jsonReverted });
        }

        public ActionResult EnvatoBasicList()
        {
            // Get Kamran's Code Canyon items
            var files = JsonService.GetUrl("http://marketplace.envato.com/api/v2/new-files-from-user:kayub,codecanyon.json");

            return View(files);
        }
    }
}
