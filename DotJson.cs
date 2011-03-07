/******************************
 * .JSON - Classes for dealing with dynamic JSON
 * http://github.com/kamranayub/.JSON/
 * 
 * Copyright 2011, Kamran Ayub. 
 * Dual licensed under the MIT or GPL Version 2 licenses (just like jQuery)
 * 
 ******************************/
namespace DotJson
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Dynamic;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using System.Web.Script.Serialization;

    /// <summary>
    /// Wraps JSON services within a class that return dynamic JSON dictionaries for
    /// easy access.
    /// </summary>
    public class JsonService
    {
        /// <summary>
        /// Instantiate dynamic JSON service with API url (root or method url). Private; use .Url(url) to start the fluent interface.
        /// </summary>
        /// <param name="baseUrlOrMethodUrl">The root API url or a method URL</param>
        private JsonService(string baseUrlOrMethodUrl)
        {
            this.BaseUri = new Uri(baseUrlOrMethodUrl);
            this.Encoding = UTF8Encoding.UTF8;
        }

        #region Private Props

        /// <summary>
        /// The base URI or method URI
        /// </summary>
        private Uri BaseUri { get; set; }

        /// <summary>
        /// Gets or sets the credentials used for the request when challenged.
        /// </summary>
        private ICredentials Credentials { get; set; }

        /// <summary>
        /// Gets or sets whether to forcefully send the Authorization HTTP header on the first request.
        /// </summary>
        private bool ForceSendAuthorization { get; set; }

        /// <summary>
        /// The Encoding to use when downloading/uploading data.
        /// </summary>
        private Encoding Encoding { get; set; }

        #endregion

        #region Static Methods

        /// <summary>
        /// Instantiate a new fluent interface for JsonService
        /// </summary>
        /// <param name="url">The root API or method URL to post/get to</param>
        /// <returns>JsonService</returns>
        public static JsonService For(string url)
        {
            return new JsonService(url);
        }

        /// <summary>
        /// Shortcut for JsonService.Url(baseUri).Get()
        /// </summary>
        /// <param name="url">Full API URL to the GET method</param>
        /// <returns>Json</returns>
        public static dynamic GetFrom(string url)
        {
            return JsonService.For(url).Get();
        }

        /// <summary>
        /// Shortcut for JsonService.Url(baseUri).Get(params)
        /// </summary>
        /// <param name="url">Full API URL to the GET method</param>
        /// <param name="queryParams">Key/value pair query data parameters (e.g. ?key=value)</param>
        /// <returns>Json</returns>
        public static dynamic GetFrom(string url, object queryParams)
        {
            return JsonService.For(url).Get(queryParams);
        }

        /// <summary>
        /// Shortcut for JsonService.Url(baseUri).Get(params)
        /// </summary>
        /// <param name="url">Full API URL to the GET method</param>
        /// <param name="queryData">Key/value pair query data parameters (e.g. ?key=value)</param>
        /// <returns>Json</returns>
        public static dynamic GetFrom(string url, IDictionary<string, string> queryData)
        {
            return JsonService.For(url).Get(queryData);
        }

        /// <summary>
        /// Shortcut for JsonService.Url(baseUri).Post(params)
        /// </summary>
        /// <param name="url">Full API URL to the GET method</param>
        /// <param name="dataParams">Key/value pair form data parameters (e.g. ?key=value)</param>
        /// <returns>Json</returns>
        public static dynamic PostTo(string url, object dataParams)
        {
            return JsonService.For(url).Post(dataParams);
        }

        /// <summary>
        /// Shortcut for JsonService.Url(baseUri).Post(params)
        /// </summary>
        /// <param name="url">Full API URL to the POST method</param>
        /// <param name="dataParams">String dictionary of Key/value pair form data parameters (e.g. ?key=value)</param>
        /// <returns>Json</returns>
        public static dynamic PostTo(string url, IDictionary<string, string> dataParams)
        {
            return JsonService.For(url).Post(dataParams);
        }

        #endregion

        #region Fluent Methods

        /// <summary>
        /// Set to use a specific encoding for requests
        /// </summary>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public JsonService UseEncoding(Encoding encoding)
        {
            this.Encoding = encoding; return this;
        }

        /// <summary>
        /// Force sending Authorization HTTP header on the request
        /// </summary>
        /// <returns></returns>
        public JsonService ForceAuthorization()
        {
            this.ForceSendAuthorization = true; return this;
        }

        /// <summary>
        /// Shortcut for adding basic auth credential
        /// </summary>
        /// <param name="userName">Your username</param>
        /// <param name="password">Your password</param>
        /// <param name="force">Forcefully send authorization header on first request</param>
        public JsonService AuthenticateAsBasic(string userName, string password, bool force = false)
        {
            return this.AuthenticateAsBasic(new NetworkCredential(userName, password), force);
        }

        /// <summary>
        /// Shortcut for adding basic auth credential
        /// </summary>
        /// <param name="credential">A NetworkCredential for your Basic auth</param>
        /// <param name="force">Forcefully send authorization header on first request</param>
        public JsonService AuthenticateAsBasic(NetworkCredential credential, bool force = false)
        {
            return this.AuthenticateAs(new CredentialCache() { { BaseUri, "Basic", credential } }, force);
        }

        /// <summary>
        /// Authenticate using an ICredentials (Windows, Digest, NTLM, etc.)
        /// </summary>
        /// <param name="credentials"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        public JsonService AuthenticateAs(ICredentials credentials, bool force = false)
        {
            this.Credentials = credentials;
            this.ForceSendAuthorization = force;

            return this;
        }

        #endregion

        #region Public Get/Post Methods

        /// <summary>
        /// GETs from API URL and returns a dynamically typed object representing the
        /// response from the service.
        /// </summary>
        /// <param name="pageMethod">Page, method, or file to append to end of API URL. Leave blank to use base url.</param>
        /// <returns></returns>
        public dynamic Get(string pageMethod = "")
        {
            return Get(pageMethod, null);
        }

        /// <summary>
        /// GETs base URL and returns a dynamically typed object representing the
        /// response from the service.
        /// </summary>
        /// <param name="queryParams">Anonymous object representing key/value pairs for querystring</param>
        /// <returns>Json</returns>
        public dynamic Get(object queryParams)
        {
            return Get(String.Empty, queryParams);
        }

        /// <summary>
        /// GETs base URL and returns a dynamically typed object representing the
        /// response from the service.
        /// </summary>
        /// <param name="queryParams">Dictionary representing key/value pairs for querystring</param>
        /// <returns>Json</returns>
        public dynamic Get(IDictionary<string, string> queryParams)
        {
            return Get(String.Empty, queryParams);
        }

        /// <summary>
        /// GETs from API URL and returns a dynamically typed object representing the
        /// response from the service.
        /// </summary>
        /// <param name="pageMethod">Page, method, or file to append to end of API URL</param>
        /// <param name="queryParams">Key/value pair query parameters (e.g. ?key=value)</param>
        /// <returns></returns>
        public dynamic Get(string pageMethod, object queryParams)
        {
            return PerformRequest(pageMethod, HttpMethod.GET, queryParams.ToNameValueCollection());
        }

        /// <summary>
        /// GETs from API URL and returns a dynamically typed object representing the
        /// response from the service.
        /// </summary>
        /// <param name="pageMethod">Page, method, or file to append to end of API URL</param>
        /// <param name="queryParams">String dictionary of Key/value pair query parameters (e.g. ?key=value)</param>
        /// <returns></returns>
        public dynamic Get(string pageMethod, IDictionary<string, string> queryParams)
        {
            return PerformRequest(pageMethod, HttpMethod.GET, queryParams.ToNameValueCollection());
        }

        /// <summary>
        /// POSTs to the base URL and returns a dynamically typed object representing the
        /// response from the service.
        /// </summary>
        /// <param name="formData">Key/value pair query parameters (e.g. ?key=value)</param>
        /// <returns></returns>
        public dynamic Post(object formData)
        {
            return Post(String.Empty, formData.ToNameValueCollection());
        }

        /// <summary>
        /// POSTs to the base URL and returns a dynamically typed object representing the
        /// response from the service.
        /// </summary>
        /// <param name="formData">Key/value pair dictionary for query parameters (e.g. ?key=value)</param>
        /// <returns></returns>
        public dynamic Post(IDictionary<string, string> formData)
        {
            return Post(String.Empty, formData);
        }

        /// <summary>
        /// POSTs to an API URL and returns a dynamically typed object representing the
        /// response from the service.
        /// </summary>
        /// <param name="pageMethod">Page, method, or file to append to end of API URL</param>
        /// <param name="formData">Key/value pair query parameters (e.g. ?key=value)</param>
        /// <returns></returns>
        public dynamic Post(string pageMethod, object formData)
        {
            return PerformRequest(pageMethod, HttpMethod.POST, formData.ToNameValueCollection());
        }

        /// <summary>
        /// POSTs to an API URL and returns a dynamically typed object representing the
        /// response from the service.
        /// </summary>
        /// <param name="pageMethod">Page, method, or file to append to end of API URL</param>
        /// <param name="formData">String dictionary of Key/value pair query parameters (e.g. ?key=value)</param>
        /// <returns></returns>
        public dynamic Post(string pageMethod, IDictionary<string, string> formData)
        {
            return PerformRequest(pageMethod, HttpMethod.POST, formData.ToNameValueCollection());
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Wraps WebClient request in single method.
        /// </summary>
        /// <param name="pageMethod"></param>
        /// <param name="method"></param>
        /// <param name="requestData"></param>
        /// <returns>Json</returns>
        private dynamic PerformRequest(string pageMethod, HttpMethod method, NameValueCollection requestData)
        {
            using (var client = new EnhancedWebClient())
            {
                if (this.Credentials == null)
                    client.UseDefaultCredentials = true;
                else
                {
                    // Force sending credentials on first request
                    if (this.ForceSendAuthorization)
                    {
                        var basicAuth = this.Credentials.GetCredential(this.BaseUri, "Basic");

                        if (basicAuth != null)
                        {
                            var cre = string.Format("{0}:{1}", basicAuth.UserName, basicAuth.Password);
                            var base64 = Convert.ToBase64String(Encoding.GetBytes(cre));

                            client.Headers.Add("Authorization", "Basic " + base64);
                        }
                    }

                    client.Credentials = this.Credentials;
                }

                // Send ACCEPT to accept JSON by default
                client.Headers.Add(HttpRequestHeader.Accept, "application/json");

                if (method == HttpMethod.POST)
                {
                    if (requestData == null)
                        throw new ArgumentNullException("requestData", "For POST, form data cannot be null.");

                    client.Headers.Add("Content-type", "application/x-www-form-urlencoded");

                    return Json.Parse(Encoding.GetString(client.UploadValues(pageMethod.ToRelativeUri(BaseUri), requestData)));
                }
                else
                {
                    client.QueryString = requestData;
                    return Json.Parse(client.DownloadString(pageMethod.ToRelativeUri(BaseUri)));
                }

            }
        }

        private enum HttpMethod { GET, POST }

        #endregion
    }

    /// <summary>
    /// Overrides the base WebClient to add decompression support
    /// </summary>
    public class EnhancedWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);

            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            return request;
        }
    }

    /// <summary>
    /// Represents a dynamic JSON object and dictionary. It is a read-only dictionary and if
    /// a JSON property cannot be translated to a CLR property (x.prop), you can access it
    /// via the dictionary[key] syntax or compact form (e.g. "foo-bar" => foobar)
    /// </summary>
    public class Json : DynamicObject
    {
        #region Private

        // It's case-sensitive, baby!
        private readonly IDictionary<string, dynamic> DynamicDictionary =
            new Dictionary<string, dynamic>(StringComparer.Ordinal);

        // Key: Compact key, Value: original value
        private readonly IDictionary<string, string> KeyDictionary =
            new Dictionary<string, string>(StringComparer.Ordinal);

        // Store original deserialized object for serialization
        private readonly object OriginalObject;

        // Store ref to a serializer to avoid instantiating them all over
        private static readonly JavaScriptSerializer Serializer = new JavaScriptSerializer();

        #endregion

        #region Cascading Constructors

        private Json(object anonObject)
            : this(Serializer.Serialize(anonObject))
        { }

        private Json(string json)
            : this(Serializer.Deserialize<IDictionary<string, object>>(json))
        { }

        /// <summary>
        /// Final constructor that always gets called
        /// </summary>
        /// <param name="dictionary"></param>
        private Json(IDictionary<string, object> dictionary)
        {
            this.OriginalObject = dictionary;

            PopulateDictionary(dictionary);
        }

        #endregion

        #region Public Methods/Properties

        /// <summary>
        /// Creates a new Dynamic JSON dictionary with given (assumes valid) JSON string.
        /// </summary>
        /// <param name="json">A JSON string to convert</param>
        /// <returns>Json</returns>
        public static dynamic Parse(string json)
        {
            // Handle arrays
            if (json.Trim().IndexOf('[') == 0)
            {
                return Serializer.Deserialize<IDictionary<string, object>[]>(json)
                    .Select(x => new Json(x)).ToArray();
            }

            return new Json(json);
        }

        /// <summary>
        /// Creates a new Dynamic JSON dictionary from given anonymous object/
        /// </summary>
        /// <param name="anonObject">An object to convert to JSON</param>
        /// <returns>Json</returns>
        public static dynamic Parse(object anonObject)
        {
            return new Json(anonObject);
        }

        /// <summary>
        /// Converts an object to a JSON string.
        /// </summary>
        /// <param name="anonObject">Object to serialize</param>
        /// <param name="pretty">Whether or not to pretty format the output</param>
        /// <returns>A JSON String</returns>
        public static string Stringify(object anonObject, bool pretty = false)
        {
            return new Json(anonObject).ToString(pretty);
        }

        /// <summary>
        ///  Support dictionary key getting
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[string key]
        {
            get { return this.DynamicDictionary[key]; }
        }

        /// <summary>
        /// Returns stringified object
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.ToString(false);
        }

        /// <summary>
        /// Converts current JSON object to a JSON string (if it is indeed a JSON object). Prettifies the output, if asked to.
        /// </summary>
        /// <param name="pretty">Whether or not to "prettify" the output</param>
        /// <returns></returns>
        public string ToString(bool pretty)
        {
            var json = Serializer.Serialize(this.OriginalObject ?? this);

            return pretty ? json.PrettyJson() : json;
        }        

        #endregion

        #region Private Helpers

        /// <summary>
        /// Translates a JSON string, object dictionary into a dictionary that wraps
        /// inner JSON objects with the Json object as well. This makes it
        /// easy for callers to access JSON via CLR property syntax or dictionary syntax.
        /// </summary>
        /// <param name="dictionary"></param>
        /// <remarks>
        /// null is an acceptable value!
        /// </remarks>
        private void PopulateDictionary(IDictionary<string, object> dictionary)
        {
            foreach (var kv in dictionary)
            {
                var value = GetJsonOrOriginal(kv.Value);

                if (value is Array)
                    value = ((object[])value).Select(o => GetJsonOrOriginal(o)).ToArray();

                DynamicDictionary.Add(kv.Key, DotJsonExtensions.TryConvert(value));

                // Only add if existing case-sensitive key does not exist.
                if (!KeyDictionary.ContainsKey(kv.Key.ToCLSId()))
                    KeyDictionary.Add(kv.Key.ToCLSId(), kv.Key);
            }
        }

        private object GetJsonOrOriginal(object o)
        {
            return o is IDictionary<string, object> ?
                new Json(o as IDictionary<string, object>) : o;
        }

        #endregion

        #region DynamicObject Overrides

        /// <summary>
        /// Tries to look for a dictionary key by the typed property name (e.g. x.property1).
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="result"></param>
        /// <returns>True if found a match, false if not.</returns>
        /// <remarks>
        /// null is an acceptable value! Judge success on whether we found a dictionary match.
        /// </remarks>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var property = binder.Name;

            // Get the key to use
            // If no existing key exists, try to get the compact version
            var key = (!this.DynamicDictionary.ContainsKey(property) &&
                this.KeyDictionary.ContainsKey(property)) ?
                this.KeyDictionary[property] : property;

            if (this.DynamicDictionary.ContainsKey(key))
                result = this.DynamicDictionary[key];
            else
                result = null;

            return this.DynamicDictionary.ContainsKey(key);
        }

        /// <summary>
        /// Forward non-existant method invocations to dictionary
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="args"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = DynamicDictionary.GetType().InvokeMember(binder.Name, BindingFlags.InvokeMethod, null, this.DynamicDictionary, args);
            return result != null;
        }

        #endregion
    }


    /// <summary>
    /// A set of extensions to help
    /// </summary>
    public static class DotJsonExtensions
    {
        /// <summary>
        /// Fixes up inconsistencies with how new Uri treats generating a URL. 
        /// Notably, removing/adding trailing or leading slashes.
        /// </summary>
        /// <param name="relativeUrl"></param>
        /// <param name="baseUri"></param>
        /// <returns></returns>
        public static Uri ToRelativeUri(this string relativeUrl, Uri baseUri)
        {
            // Ignore if no relative URL
            if (String.IsNullOrEmpty(relativeUrl))
                return baseUri;

            var endSlashRx = new System.Text.RegularExpressions.Regex("/+$");
            var beginSlashRx = new System.Text.RegularExpressions.Regex("^/+");

            // If no ending slash on baseUri, add it
            if (!endSlashRx.IsMatch(baseUri.ToString()))
                baseUri = new Uri(baseUri + "/");

            // Remove leading slash on relative, if any
            if (beginSlashRx.IsMatch(relativeUrl))
                relativeUrl = beginSlashRx.Replace(relativeUrl, String.Empty);

            return new Uri(baseUri, Uri.EscapeDataString(relativeUrl));
        }

        /// <summary>
        /// Gets an anonymous object's properties as a NameValueCollection.
        /// </summary>
        /// <param name="thing"></param>
        /// <returns></returns>
        public static NameValueCollection ToNameValueCollection(this object thing)
        {
            if (thing == null)
                return null;

            var nvCollection = new NameValueCollection();

            foreach (var p in thing.GetType().GetProperties())
            {
                nvCollection.Add(p.Name, p.GetValue(thing, null).ToString());
            }

            return nvCollection;
        }

        /// <summary>
        /// Converts a string dictionary to a NameValueCollection.
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static NameValueCollection ToNameValueCollection(this IDictionary<string, string> dict)
        {
            if (dict == null)
                return null;

            var nvCollection = new NameValueCollection();

            foreach (var kv in dict)
            {
                nvCollection.Add(kv.Key, kv.Value);
            }

            return nvCollection;
        }

        /// <summary>
        /// Converts a string to a CLS-compliant ID.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToCLSId(this string input)
        {
            return new System.Text.RegularExpressions.Regex(@"[^\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nd}\p{Nl}\p{Mn}\p{Mc}\p{Cf}\p{Pc}\p{Lm}]")
                .Replace(input, String.Empty);
        }

        /// <summary>
        /// Try to guess type and cast as that for some JSON values.
        /// </summary>
        /// <param name="thing"></param>
        /// <returns></returns>
        public static dynamic TryConvert(this object thing)
        {
            DateTime theDate; Uri theUri;

            // Try converting dates
            if (thing is string && DateTime.TryParse(thing.ToString(), out theDate))
                return theDate;

            // Try converting absolute URLs
            if (thing is string && Uri.TryCreate(thing.ToString(), UriKind.Absolute, out theUri))
                return theUri;

            return thing;
        }

        /// <summary>
        /// Formats a JSON string by walking through it and examining the contents.
        /// </summary>
        /// <param name="json">Unformatted JSON string, expects JavaScriptSerializer.Deserialize() output</param>
        /// <returns>Formatted JSON string</returns>
        /// <remarks>
        /// [ { should have line breaks and tabs after them
        /// ] } should have line breaks and tabs before them
        /// : should have a space after it
        /// , should have a line break and tab
        /// </remarks>
        public static string PrettyJson(this string json)
        {
            var sbOutput = new StringBuilder();
            bool inQuotes = false;
            int level = 0;

            Action Tabify = () =>
            {
                var chars = new char[level];

                for (var i = 0; i < level; i++)
                    chars[i] = '\t';

                sbOutput.Append(chars);
            };

            for (var i = 0; i < json.Length; i++)
            {
                var curChar = json[i];

                // Ignore escaped quotes
                if (curChar == '"' && json[i - 1] != '\\')
                {
                    if (!inQuotes)
                        inQuotes = true;
                    else
                        inQuotes = false;
                }

                // Don't format anything within quotes
                if (!inQuotes)
                {
                    if (curChar == '{' || curChar == '[' || curChar == ',')
                    {
                        if (curChar != ',') level++;
                        sbOutput.Append(curChar);
                        sbOutput.AppendLine();
                        Tabify();
                    }
                    else if (curChar == '}' || curChar == ']')
                    {
                        level--;
                        sbOutput.AppendLine();
                        Tabify();
                        sbOutput.Append(curChar);
                    }
                    else if (curChar == ':')
                    {
                        sbOutput.Append(curChar + " ");
                    }
                    else
                    {
                        sbOutput.Append(curChar);
                    }
                }
                else
                {
                    sbOutput.Append(curChar);
                }
            }

            return sbOutput.ToString();
        }
    }
}