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
            Get["/get"] = _ => "success";

            Put["/put"] = _ => "success";

            Post["/post"] = _ => "success";

            Delete["/delete"] = _ => "success";

            Put["/put/headers"] = _ => Response.AsJson(Request.Headers.ToArray());
            
            Post["/post/headers"] = _ => Response.AsJson(Request.Headers.ToArray());

            Delete["/delete/headers"] = _ => Response.AsJson(Request.Headers.ToArray());

            Get["/get/headers"] = _ => Response.AsJson(Request.Headers.ToArray());

            Put["/put/content"] = _ => Request.Body.AsString();

            Post["/post/content"] = _ => Request.Body.AsString();
            
            Delete["/delete/content"] = _ => Request.Body.AsString();

            Get["/delay/{ms:int}"] = parameters =>
            {
                Thread.Sleep(parameters.ms);
                return $"Finished sleeping for {parameters.ms}ms";
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
