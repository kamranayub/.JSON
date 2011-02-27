namespace DotJson
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Net;
    using System.Collections.Specialized;
    using System.Dynamic;
    using System.Web.Script.Serialization;
    using System.Reflection;

    /// <summary>
    /// Wraps JSON services within a class that return dynamic JSON dictionaries for
    /// easy access.
    /// </summary>
    public class JsonService
    {
        private Uri BaseUri { get; set; }

        /// <summary>
        /// Instantiate dynamic JSON service with API url (root or method url)
        /// </summary>
        /// <param name="baseUri"></param>
        public JsonService(Uri baseUri)
        {
            this.BaseUri = baseUri;
            this.Encoding = UTF8Encoding.UTF8;
        }

        /// <summary>
        /// Shortcut for new JsonService(baseUri).GET(relative)
        /// </summary>
        /// <param name="url">Full API URL to the GET metod</param>
        /// <returns></returns>
        public static dynamic GetUrl(string url)
        {
            return new JsonService(new Uri(url)).GET("");
        }

        /// <summary>
        /// Shortcut for new JsonService(baseUri).POST(relative, params)
        /// </summary>
        /// <param name="url">Full API URL to the GET metod</param>
        /// <param name="dataParams">Key/value pair query parameters (e.g. ?key=value)</param>
        /// <returns></returns>
        public static dynamic PostUrl(string url, object dataParams)
        {
            return new JsonService(new Uri(url)).POST("", dataParams);
        }

        /// <summary>
        /// Shortcut for new JsonService(baseUri).POST(relative, params)
        /// </summary>
        /// <param name="url">Full API URL to the POST metod</param>
        /// <param name="dataParams">String dictionary of Key/value pair query parameters (e.g. ?key=value)</param>
        /// <returns></returns>
        public static dynamic PostUrl(string url, IDictionary<string, string> dataParams)
        {
            return new JsonService(new Uri(url)).POST("", dataParams);
        }

        #region Public Props

        /// <summary>
        /// Gets or sets the credentials used for the request when challenged.
        /// </summary>
        public ICredentials Credentials { get; set; }

        /// <summary>
        /// Gets or sets whether to forcefully send the Authorization HTTP header on the first request.
        /// </summary>
        public bool ForceSendAuthorization { get; set; }

        /// <summary>
        /// The Encoding to use when downloading/uploading data.
        /// </summary>
        public Encoding Encoding { get; set; }

        #endregion

        /// <summary>
        /// Shortcut for adding basic auth credential
        /// </summary>
        /// <param name="userName">Your username</param>
        /// <param name="password">Your password</param>
        /// <param name="force">Forcefully send authorization header on first request</param>
        public void AddBasicAuth(string userName, string password, bool force = false)
        {
            this.AddBasicAuth(new NetworkCredential(userName, password), force);
        }

        /// <summary>
        /// Shortcut for adding basic auth credential
        /// </summary>
        /// <param name="credential">A NetworkCredential for your Basic auth</param>
        /// <param name="force">Forcefully send authorization header on first request</param>
        public void AddBasicAuth(NetworkCredential credential, bool force = false)
        {
            var cred = new CredentialCache();

            cred.Add(BaseUri, "Basic", credential);

            this.Credentials = cred;
            this.ForceSendAuthorization = force;
        }

        /// <summary>
        /// GETs from API URL and returns a dynamically typed object representing the
        /// response from the service.
        /// </summary>
        /// <param name="pageMethod">Page, method, or file to append to end of API URL</param>
        /// <returns></returns>
        public dynamic GET(string pageMethod)
        {
            return GET(pageMethod, null);
        }

        /// <summary>
        /// GETs from API URL and returns a dynamically typed object representing the
        /// response from the service.
        /// </summary>
        /// <param name="pageMethod">Page, method, or file to append to end of API URL</param>
        /// <param name="queryParams">Key/value pair query parameters (e.g. ?key=value)</param>
        /// <returns></returns>
        public dynamic GET(string pageMethod, object queryParams)
        {
            return PerformGET(pageMethod, queryParams.ToNameValueCollection());
        }

        /// <summary>
        /// GETs from API URL and returns a dynamically typed object representing the
        /// response from the service.
        /// </summary>
        /// <param name="pageMethod">Page, method, or file to append to end of API URL</param>
        /// <param name="queryParams">String dictionary of Key/value pair query parameters (e.g. ?key=value)</param>
        /// <returns></returns>
        public dynamic GET(string pageMethod, IDictionary<string, string> queryParams)
        {
            return PerformGET(pageMethod, queryParams.ToNameValueCollection());
        }

        /// <summary>
        /// POSTs to an API URL and returns a dynamically typed object representing the
        /// response from the service.
        /// </summary>
        /// <param name="pageMethod">Page, method, or file to append to end of API URL</param>
        /// <param name="formData">Key/value pair query parameters (e.g. ?key=value)</param>
        /// <returns></returns>
        public dynamic POST(string pageMethod, object formData)
        {
            return PerformPOST(pageMethod, formData.ToNameValueCollection());
        }

        /// <summary>
        /// POSTs to an API URL and returns a dynamically typed object representing the
        /// response from the service.
        /// </summary>
        /// <param name="pageMethod">Page, method, or file to append to end of API URL</param>
        /// <param name="formData">String dictionary of Key/value pair query parameters (e.g. ?key=value)</param>
        /// <returns></returns>
        public dynamic POST(string pageMethod, IDictionary<string, string> formData)
        {
            return PerformPOST(pageMethod, formData.ToNameValueCollection());
        }

        #region Private Helpers

        private dynamic PerformGET(string pageMethod, NameValueCollection queryData)
        {
            return Json.Parse(PerformRequest(pageMethod, HttpMethod.GET, queryData));
        }

        private dynamic PerformPOST(string pageMethod, NameValueCollection formData)
        {
            if (formData != null)
            {
                return Json.Parse(PerformRequest(pageMethod, HttpMethod.POST, formData));
            }
            else
                throw new ArgumentNullException("formData)", "For POST, data cannot be null.");
        }

        /// <summary>
        /// Wraps WebClient request in single method.
        /// </summary>
        /// <param name="pageMethod"></param>
        /// <param name="method"></param>
        /// <param name="requestData"></param>
        /// <returns></returns>
        private string PerformRequest(string pageMethod, HttpMethod method, NameValueCollection requestData)
        {
            using (var client = new WebClient())
            {
                if (this.Credentials == null)
                {
                    client.UseDefaultCredentials = true;
                }
                else
                {
                    // Force sending credentials on first request
                    if (this.ForceSendAuthorization)
                    {
                        var basicAuth = this.Credentials.GetCredential(this.BaseUri, "Basic");

                        if (basicAuth != null)
                        {
                            var cre = string.Format("{0}:{1}", basicAuth.UserName, basicAuth.Password);
                            var base64 = Convert.ToBase64String(System.Text.UTF8Encoding.UTF8.GetBytes(cre));

                            client.Headers.Add("Authorization", "Basic " + base64);
                        }
                    }

                    client.UseDefaultCredentials = false;
                    client.Credentials = this.Credentials;
                }

                // Send ACCEPT to accept JSON by default
                client.Headers.Add(HttpRequestHeader.Accept, "application/json");

                if (method == HttpMethod.POST)
                {
                    client.Headers.Add("Content-type", "application/x-www-form-urlencoded");

                    return Encoding.GetString(client.UploadValues(pageMethod.ToRelativeUri(BaseUri), requestData));
                }
                else
                {
                    client.QueryString = requestData;
                    return client.DownloadString(pageMethod.ToRelativeUri(BaseUri));
                }

            }
        }

        private enum HttpMethod
        {
            GET,
            POST
        }

        #endregion
    }

    /// <summary>
    /// Represents a dynamic JSON object and dictionary. It is a read-only dictionary and if
    /// a JSON property cannot be translated to a CLR property (x.prop), you can access it
    /// via the dictionary[key] syntax or compact form (e.g. "foo-bar" => foobar)
    /// </summary>
    public class Json : DynamicObject, IDictionary<string, object>
    {
        // It's case-sensitive, baby!
        private readonly IDictionary<string, dynamic> DynamicDictionary = new Dictionary<string, dynamic>(StringComparer.Ordinal);

        // Key: Compact key, Value: original value
        private readonly IDictionary<string, string> KeyDictionary = new Dictionary<string, string>(StringComparer.Ordinal);

        private readonly object OriginalObject;

        /// <summary>
        /// Creates a new Dynamic JSON dictionary with given (assumes valid) JSON string.
        /// </summary>
        /// <param name="json"></param>
        public static dynamic Parse(string json)
        {
            return new Json(json);
        }

        /// <summary>
        /// Creates a new Dynamic JSON dictionary from given anonymous object/
        /// </summary>
        /// <param name="anonObject"></param>
        /// <returns></returns>
        public static dynamic Parse(object anonObject)
        {
            return new Json(anonObject);
        }

        /// <summary>
        /// Converts an object to a JSON string.
        /// </summary>
        /// <param name="anonObject"></param>
        /// <returns></returns>
        public static string Stringify(object anonObject)
        {
            return new JavaScriptSerializer().Serialize(anonObject);
        }

        #region Constructors

        private Json(string json)
        {
            this.OriginalObject = new JavaScriptSerializer().DeserializeObject(json);

            TranslateDictionary(new JavaScriptSerializer().Deserialize<IDictionary<string, dynamic>>(json));
        }

        private Json(object anonObject)
            : this(new JavaScriptSerializer().Serialize(anonObject))
        {
            this.OriginalObject = anonObject;
        }

        /// <summary>
        /// This constructor gets called for sub-properties in the Dynamic Dictionary, recursively.
        /// </summary>
        /// <param name="dictionary"></param>
        private Json(IDictionary<string, dynamic> dictionary)
        {
            this.OriginalObject = dictionary;

            TranslateDictionary(dictionary);
        }

        #endregion

        /// <summary>
        /// Returns stringified object
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this.OriginalObject != null)
            {
                return new JavaScriptSerializer().Serialize(this.OriginalObject);
            }
            else
            {
                return base.ToString();
            }
        }

        /// <summary>
        /// Translates a JSON string, object dictionary into a dictionary that wraps
        /// inner JSON objects with the DynamicJsonDictionary as well. This makes it
        /// easy for callers to access JSON via CLR property syntax or dictionary syntax.
        /// </summary>
        /// <param name="dictionary"></param>
        private void TranslateDictionary(IDictionary<string, object> dictionary)
        {
            foreach (var kv in dictionary)
            {
                object value = kv.Value;

                if (kv.Value is IDictionary<string, object>)
                {
                    value = new Json(kv.Value as IDictionary<string, dynamic>);
                }
                else if (kv.Value is Array)
                {
                    var objects = kv.Value as object[];

                    if (objects.Any())
                    {
                        value = objects
                            .Select(o => o is IDictionary<string, dynamic> ?
                                new Json(o as IDictionary<string, dynamic>) : o).ToArray();
                    }
                }

                DynamicDictionary.Add(kv.Key, value.TryConvert());

                // Only add if existing case-sensitive key does not exist.
                if (!KeyDictionary.ContainsKey(kv.Key.ToCLSId()))
                    KeyDictionary.Add(kv.Key.ToCLSId(), kv.Key);
            }
        }

        /// <summary>
        /// Tries to look for a dictionary key by the typed property name (e.g. x.property1).
        /// </summary>
        /// <param name="jsonKeyPath"></param>
        /// <returns></returns>
        private dynamic GetPropertyValue(string jsonKeyPath)
        {
            // Find compact key, in case caller is requesting one
            KeyValuePair<string, string> compactKv = this.KeyDictionary.LastOrDefault(x =>
                x.Key.Equals(jsonKeyPath, StringComparison.Ordinal));

            if (this.DynamicDictionary.ContainsKey(jsonKeyPath))
            {
                return this.DynamicDictionary[jsonKeyPath];
            }
            else if (compactKv.Key != null)
            {
                return this.DynamicDictionary[compactKv.Value];
            }

            return null;
        }

        #region DynamicObject Overrides

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = GetPropertyValue(binder.Name);
            return result != null;
        }

        #endregion

        #region IDictionary<string, object> Members

        public void Add(string key, object value)
        {
            throw new NotImplementedException("This dictionary is read-only.");
        }

        public bool ContainsKey(string key)
        {
            return this.DynamicDictionary.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
            get { return this.DynamicDictionary.Keys; }
        }

        public bool Remove(string key)
        {
            throw new NotImplementedException("This dictionary is read-only");
        }

        public bool TryGetValue(string key, out object value)
        {
            return this.DynamicDictionary.TryGetValue(key, out value);
        }

        public ICollection<object> Values
        {
            get { return this.DynamicDictionary.Values; }
        }

        public object this[string key]
        {
            get
            {
                return this.DynamicDictionary[key];
            }
            set
            {
                throw new NotImplementedException("This dictionary is read-only");
            }
        }

        public void Add(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException("This dictionary is read-only");
        }

        public void Clear()
        {
            throw new NotImplementedException("This dictionary is read-only");
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return this.DynamicDictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            this.DynamicDictionary.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return this.DynamicDictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException("This dictionary is read-only");
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            return this.DynamicDictionary.GetEnumerator();
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return this.DynamicDictionary.GetEnumerator();
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
        /// <param name="url"></param>
        /// <param name="baseUri"></param>
        /// <returns></returns>
        public static Uri ToRelativeUri(this string url, Uri baseUri)
        {
            var endSlashRx = new System.Text.RegularExpressions.Regex("/+$");
            var beginSlashRx = new System.Text.RegularExpressions.Regex("^/+");

            // If no leading slash on baseUri, add it
            if (!endSlashRx.IsMatch(baseUri.ToString()))
                baseUri = new Uri(baseUri + "/");

            // Remove leading slash on relative, if any
            if (beginSlashRx.IsMatch(url))
                url = beginSlashRx.Replace(url, String.Empty);

            return new Uri(baseUri, Uri.EscapeDataString(url));
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
                var value = p.GetValue(thing, null);

                nvCollection.Add(p.Name, value.ToString());
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
                .Replace(input, string.Empty);
        }

        /// <summary>
        /// Try to guess type and cast as that for some JSON values.
        /// </summary>
        /// <param name="thing"></param>
        /// <returns></returns>
        public static dynamic TryConvert(this object thing)
        {
            DateTime theDate;
            Uri theUri;

            // Try converting dates
            if (thing is string && DateTime.TryParse(thing.ToString(), out theDate))
            {
                return theDate;
            }

            // Try converting absolute URLs
            if (thing is string && Uri.TryCreate(thing.ToString(), UriKind.Absolute, out theUri))
            {
                return theUri;
            }

            return thing;
        }
    }
}
