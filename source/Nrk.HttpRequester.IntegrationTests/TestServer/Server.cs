using System.Linq;
using System.Net.Http;
using System.Threading;
using Nancy;
using Nancy.Extensions;
using Owin;

namespace Nrk.HttpRequester.IntegrationTests.TestServer
{
    public class ServerModule : NancyModule
    {
        public ServerModule()
        {
            Get["/"] = _ => "success";

            Put["/"] = _ => "success";

            Post["/"] = _ => "success";

            Delete["/"] = _ => "success";

            Put["/headers"] = _ => 
            {
                var headers = Request.Headers.ToArray();
                return Response.AsJson(headers);
            };

            Post["/headers"] = _ =>
            {
                var headers = Request.Headers.ToArray();
                return Response.AsJson(headers);
            };

            Delete["/headers"] = _ =>
            {
                var headers = Request.Headers.ToArray();
                return Response.AsJson(headers);
            };

            Put["/content"] = _ =>
            {
                var body = Request.Body;
                return body.AsString();
            };

            Post["/content"] = _ =>
            {
                var body = Request.Body;
                return body.AsString();
            };

            Delete["/content"] = _ =>
            {
                var body = Request.Body;
                return body.AsString();
            };

            Get["/delay/{ms:int}"] = parameters =>
            {
                Thread.Sleep(parameters.ms);
                return $"Finished sleeping for {parameters.ms}ms";
            };

            Get["/headers"] = _ =>
            {
                var headers = Request.Headers.ToArray();
                return Response.AsJson(headers);
            };
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            appBuilder.UseNancy();
        }
    }
}
