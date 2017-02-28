using System;
using System.Collections.Specialized;

namespace Nrk.HttpRequester
{
    internal static class UriBuilder
    {
        internal static string Build(string template, NameValueCollection parameters)
        {
            var uriTemplate = new UriTemplate(template);

            // Base URI is set in the HttpClient, this one is just needed for binding
            var prefix = new Uri("http://localhost");

            var uri = uriTemplate.BindByName(prefix, parameters);
            return uri.PathAndQuery;
        }
    }
}
