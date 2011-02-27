.JSON is For Winners
====================

**.JSON** is a group of classes **in one file** that help you easily work with JSON as a **dynamically typed object** (`myJson.someProperty[0].name`), obtained from from web services, strings, or anonymous objects.

It's made up of syntactic sugar, spice, and everything nice.

### Examples ###

View full examples in */src/*.

### Install ###

 - Copy the file to your project. 
 - Add a reference to `System.Web.Extensions`. 
 - Take a deep breath
 - You're all done

## JSON Web Services - `JsonService` ##

Call `JsonService.GetUrl(url)` to access JSON-enabled web services. The class will expect that all results will be returned in JSON. It exposes several properties that you can manipulate for more control over the request.
	
	// Get my repos
	var repositories = JsonService.GetUrl("http://github.com/api/v2/repos/show/kamranayub").repositories;
	
	return View(repositories);

### The Long Way ###

`JsonService` provides two static shortcuts: `GetUrl(url)` and `PostUrl(url, params)`. Use them when you just need the bare-bones. Otherwise, instantiate it:

	var gitUri = new Uri("https://github.com/api/v2/json/");
	var gitService = new JsonService(gitUri);

### Supports Authentication, too ###

	// Connect to GitHub
	var gitUri = new Uri("https://github.com/api/v2/json/");
	var gitService = new JsonService(gitUri);

	// Authenticate to GitHub using OAuth token
	// Force authentication on first request for GitHub
	gitService.AddBasicAuth("kamranayub/token", "xxx", true);

	// Get me
	var user = gitService.GET("user/show").user;

	return View(user);

### Properties ###

- Credentials (`ICredentials`): default null
  - Gets or sets the credentials used for the request
- Encoding (`Encoding`): default UTF8
  - Gets or sets the encoding used for the request
- ForceSendAuthorization (`bool`): default false
  - Gets or sets a value indicating whether to force sending the `Authorization` HTTP header on the first request
  
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

## Oh yah, LINQ works too. ##

**"I heard you can't use dynamics with LINQ... bummer!"** ~ You

You sure about that?

	var json = new
	{
		items = new object[] {
			new { sales = 5 },
			new { sales = 20 },
			new { sales = 8 }
		}
	};
	
	// Magic!
	dynamic[] items = Json.Parse(json).items;
	
	// Returns 20
	return items
		.OrderByDescending(f => f.sales)
		.First().sales;

D'oh! To use LINQ, just cast a property you know to be an array to `dynamic[]` and all will be forgiven, grasshopper. The trick is to explicitly declare the dynamic property as a dynamic array, then the compiler understands that it should resolve it at runtime.

## Basic Type Inferrance ##

The Microsoft serialization API converts most primitive types fine, but .JSON goes a step further:

	DateTime repoCreated = json.repository.created_at;
	Uri repoUrl = json.repository.url;
	
## Special characters in key IDs? No problem. ##

### Compact Names ###

*If no naming conflicts exist*, `Json` also supports compact versions of JSON key names (strips all non-allowed characters for CLS identifiers).

A compact name is always secondary; that is, an exact match will always return first before `Json` will check the compact key dictionary.

The following would all be accessible from compact equivalents, as long as an _actual_ key by the compact equivalent doesn't exist:

 - `foo-bar` => `foobar`
 - `Foo-Bar` => `FooBar`
 - `foo-Bar` => `fooBar`
	
If an actual key **does** exist, you need to access it via the dictionary:

	var json = "{ 'foobar': 1, 'foo-bar': 2 }";
	
	// .foobar will return 1 as its an exact match
	// ["foo-bar"] will return 2 as expected
 
This is not a standard of JSON but is used to make your life easier.

#### Example ####

	// Lookups are case-sensitive to be safe.
	// "foo_bar"'s compact is "foobar" but if
	// you requested "foobar" it is an exact match
	// for an existing key, so watch out.
	string f1 = json.foobar;
	string f2 = json.FooBar;
	string f3 = json.foo_bar;
	
	// "Foo--Bar" will not have a compact equivalent
	// because it is already taken by "Foo-Bar"
	string f4 = json["Foo--Bar"];
	
### At least you have a dictionary ###

`Json` implements `IDictionary<string, object>` just for you, so you can get to any JSON key ever made:

	dynamic json = Json.Parse("{ 'foo-bar': 'baz', '111': { 'awesome': true } }");

	string fooBar = json["foo-bar"];
	bool awesome = json["111"].awesome;
	
If a key is named the same as a previous key, its value will be overwritten as per the standard.