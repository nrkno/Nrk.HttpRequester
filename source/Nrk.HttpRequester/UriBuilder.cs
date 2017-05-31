using System;
using System.Collections.Specialized;

namespace Nrk.HttpRequester
{
    public static class UriBuilder
    {
        public static string Build(string template, NameValueCollection parameters)
        {
            var uriTemplate = new UriTemplate(template);

            // Base URI is set in the HttpClient, this one is just needed for binding
            var prefix = new Uri("http://localhost");

            var noEmptyParametersCollection = new NameValueCollection();
            foreach (string name in parameters)
            {
                var value = parameters[name];
                if (!string.IsNullOrEmpty(value))
                    noEmptyParametersCollection.Add(name, value);
            }

            var uri = uriTemplate.BindByName(prefix, noEmptyParametersCollection);
            return uri.PathAndQuery;
        }
    }
}
