.JSON is For Winners
====================

**.JSON** is a group of classes **in one file** that help you easily work with JSON as a **dynamically typed object** (`myJson.someProperty[0].name`), obtained from web services, strings, or anonymous objects.

It's made up of syntactic sugar, spice, and everything nice.
	
	// In controller
	public ActionResult Index() {
		dynamic[] repositories = JsonService.GetFrom("http://github.com/api/v2/json/repos/show/kamranayub").repositories;
		
		dynamic topRepo = repositories.OrderBy(r => r.watchers).First();
		
		return View(topRepo);
	}
	
	// In razor file
	@model dynamic
	
	<h2>@Model.name</h2>
	
	<p><a href="@Model.url">@Model.url</a> (@Model.watchers watching, @Model.forks forks)</p>

## Learn

View [the Wiki](https://github.com/kamranayub/.JSON/wiki) to learn more about **.JSON** and why it's awesome.