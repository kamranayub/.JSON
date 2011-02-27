# .JSON is For Winners #

*.JSON* is a single-file group of classes that help you easily work with JSON as a *dynamically typed object* (`myJson.someProperty[0].name`), obtained from from web services, strings, or anonymous objects.

It's syntactic sugar.

# Examples #

View full examples in */src/*.

## JSON Web Services - `JsonService` ##

Create a new `JsonService` to access JSON-enabled web services. The class will expect that all results will be returned in JSON. It exposes several properties that you can manipulate for more control over the request.
	
	var gitUri = new Uri("https://github.com/api/v2/json/");
	var gitService = new JsonService(gitUri));
	
	// Authentication? No problem. Force authentication on first request for GitHub.
	gitService.AddBasicAuth("kamranayub/token", "xxx", force: true);	
		
	// Use the GET to get, POST to post
	// Each takes a second anonymous object or Dictionary to represent query string
	// name/value pairs.
	// They return "dynamic" types.
	dynamic repository = gitService.GET("repos/show/kamranayub/dotjson").repository;
	
	// Access as properties
	string repoName = repository.name;
	string repoDesc = repository.description;

	// Access arrays
	dynamic repositories = gitService.GET("repos/kamranayub").repositories;
	
	foreach (dynamic r in repositories) {
		// Do something
		// r.name;
		// r.description;
		// r.url
	}

	// It will even handle basic types for you
	int repoWatchers = repository.watchers;
	DateTime repoCreated = repository.created_at;
	bool repoPrivate = repository.private;
	Uri repoUrl = repository.url;

## Standalone JSON Parsing - `Json` ##

Don't fret if you don't want to use this only for web services. Why not just create a `Json` object?

	dynamic json = Json.Parse("{ 'foo': 'bar', 'baz': [ 'a', 'b', 'c' ], 'deeper': { 'object': 1 } }");
	
	// Get some properties...
	string foo = json.foo;
	string[] = json.baz;
	
	// And back to a string again...
	json.ToString();
	
	// Or go deeper and get inner JSON...
	json.deeper.ToString();

## Hell, why not roll your own JSON? ##

I mean, who's going to stop you?

	var myJson = new
	{
		foo = "baz",
		bar = new object[] { 
			"value1", 
			new { foo = "bar" }
		}
	};

	// This is a shortcut for `new JavaScriptSerializer.Serialize(object)`
	// in System.Web.Extensions.Services
	string jsonString = Json.Stringify(myJson);
	
	// Or manipulate it like other examples
	var json = Json.Parse(myJson);
	
## Special characters in key IDs? No problem. ##

`Json` implements `IDictionary<string, object>` just for you, so you can get to any JSON key ever made:

	dynamic json = Json.Parse("{ 'foo-bar': 'baz', '111': { 'awesome': true } }");

	string fooBar = json["foo-bar"];
	bool awesome = json["111"].awesome;

Or, if no naming conflicts exist, `Json` also supports compact versions of properties (strips all non-allowed characters for CLS identifiers).

A compact name is always secondary; that is, an exact match will always return first before `Json` will check the compact key dictionary.

The following would all be accessible from compact equivalents, as long as an _actual_ key by that name doesn't exist:

 - foo-bar => foobar
 - Foo-Bar => FooBar
 - foo-Bar => fooBar
	
If an actual key *does* exist, you need to access the special-character key via the dictionary:

	dynamic json = "{ 'foobar': 1, 'foo-bar': 2 }";
	
	// .foobar will return 1 as its an exact match
	// ["foo-bar"] will return 2 as expected
 
This is not a standard of JSON but is used to help when dynamically accessing the JSON properties and provider for ease of use.
 
### Example ###

	// In this case, both "foo-bar" and "Foo-Bar" are 
	// accessible both in C# and VB.NET due to the 
	// beauty of dynamic typing and having control over lookups.
	// "foo_bar" does not need a compact equivalent.
	string f1 = json.foobar;
	string f2 = json.FooBar;
	string f3 = json.foo_bar;
	
	// "Foo--Bar" will not have a compact equivalent
	// because it is already taken by "Foo-Bar"
	string f4 = json["Foo--Bar"];
	
If a key is named the same as a previous key, its value will be overwritten as per the standard.

## So what about LINQ? ##

Because these objects and properties are built dynamically, .NET does not support lambda expressions when using dynamics. However, you can do it manually:

	var json = new { array = new object[] { "a", "b", "c", 0 } };
	dynamic x = Json.Parse(json);

	returns (x.array as object[])
		.Cast<string>().FirstOrDefault(y => y == "a"));
