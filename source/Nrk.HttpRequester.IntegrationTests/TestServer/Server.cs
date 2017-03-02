using System.Linq;
using System.Threading;
using Nancy;
using Owin;

namespace Nrk.HttpRequester.IntegrationTests.TestServer
{
    public class ServerModule : NancyModule
    {
        public ServerModule()
        {
            Get["/"] = _ => "success";

            Get["/delay/{ms:int}"] = parameters =>
            {
                var delay = (int)parameters.ms;
                Thread.Sleep(delay);
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
