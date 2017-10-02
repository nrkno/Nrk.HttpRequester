[![NuGet](https://img.shields.io/nuget/v/NRK.HttpRequester.svg)](https://www.nuget.org/packages/NRK.HttpRequester/)
[![Build status](https://ci.appveyor.com/api/projects/status/x69p1i51fwscr9nn/branch/master?svg=true)](https://ci.appveyor.com/project/NRKOpensource/nrk-httprequester/branch/master)
# Nrk.HttpRequester
Library for sending Http Requests, including a fluent interface for creating HttpClient instances

## Why
This library tries to solve the common issues with HttpClient, by giving an easy way to set up an HttpClient that can be re-used through the life of an application.

The recommended usage of HttpClient is not to dispose it after every request. It indirectly implements the IDisposable interface, which can explain the misunderstanding. Instantiating an HttpClient for every request can possible exhaust the number of sockets available under heavy loads. [Microsoft patterns & practices](https://github.com/mspnp/performance-optimization/blob/master/ImproperInstantiation/docs/ImproperInstantiation.md) goes into detail on this issue.

A re-usable HttpClient introduces the risk of not honoring DNS changes. We try to fix this by setting the `ConnectionLeaseTimeout` for the Service Endpoint, which is by default infinite.

## Usage
### Build an IHttpClient
The fluent interface allows you to set Timeout, DefaultRequestheaders, DelegatingHandler and DelegatingHandlers for caching.:
```cs
var httpClient = WebRequestHttpClientFactory
    .Configure(new Uri("www.yourBaseUrl.com"))
    .WithDefaultRequestHeaders(new Dictionary<string, string> {{"header", "value"}}) // added to all requests
    .WithHandler(yourHandler)
    .WithConnectionLeaseTimeout(60000)
    .Create();
```
### New up a WebRequester
```cs
var webRequester = new WebRequester(httpClient);
```

`WebRequester` is an immutable object. If you need a modified version, this can be done with the `With(Action<HttpRequestMessage> message)` method. E.g:
```cs
var webRequester = new WebRequester(httpClient);
var childRequester = webRequester.With(m => m.Headers.Add("request-specific-header", "request-specific value"));
```
`childRequester` is a copy of `webRequester` with the same configuration (including `IHttpClient`), except it adds a "request-specific-header" to every outgoing request. This is useful for request-ids and request correlation.




### Use the WebRequester
The following methods are available from IWebRequester:

```cs
        Task<string> GetResponseAsStringAsync(string url, int retries = 0);
        Task<string> GetResponseAsStringAsync(string pathTemplate, NameValueCollection parameters, int retries = 0);
        Task<HttpResponseMessage> GetResponseAsync(string url, int retries = 0);
        Task<HttpResponseMessage> GetResponseAsync(string pathTemplate, NameValueCollection parameters, int retries = 0);
```

You can either send a path `example/path/123` or a [URI Template](https://tools.ietf.org/html/rfc6570) string with matching parameters in a [`NameValueCollection`](https://msdn.microsoft.com/en-us/library/system.collections.specialized.namevaluecollection(v=vs.110).aspx):

```cs
var response = webRequester.GetResponseAsync(
        "example/path/{id}",
        new NameValueCollection { { "id", "123" } }
    );
```

## Contributing

### Building and packaging
Use `build.cmd` to run our [FAKE](http://fsharp.github.io/FAKE/) build script. This will restore NuGet packages, build the project, run the tests and create a NuGet package.

### Code style
Make sure your editor has support for `.editorconfig` files ([http://editorconfig.org/](http://editorconfig.org/)), or use the same formatting as specified in the file.

### Versioning
Versioning of the NuGet package adhers to the [Semantic Versioning 2.0](http://semver.org/) standard:

> Given a version number MAJOR.MINOR.PATCH, increment the:

> 1. MAJOR version when you make incompatible API changes,
> 2. MINOR version when you add functionality in a backwards-compatible manner, and
> 3. PATCH version when you make backwards-compatible bug fixes.

> Additional labels for pre-release and build metadata are available as extensions to the MAJOR.MINOR.PATCH format.

## License
MIT
