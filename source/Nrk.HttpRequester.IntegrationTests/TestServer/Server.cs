using Nancy;
using Owin;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace Nrk.HttpRequester.IntegrationTests.TestServer
{
    public class ServerModule : NancyModule
    {
        public ServerModule()
        {

            Get("/get", args => "success");

            Put("/put", args => "success");

            Post("/post", args => "success");

            Delete("/delete", args => "success");

            Put("/put/headers", args => Response.AsJson(Request.Headers.ToArray()));

            Post("/post/headers", args => Response.AsJson(Request.Headers.ToArray()));

            Delete("/delete/headers", args => Response.AsJson(Request.Headers.ToArray()));

            Get("/get/headers", args => Response.AsJson(Request.Headers.ToArray()));

            Get("/get/cookies", args => Response.AsJson(Request.Cookies));

            Put("/put/content", args => new StreamReader(Request.Body).ReadToEnd());

            Post("/post/content", args => new StreamReader(Request.Body).ReadToEnd());

            Delete("/delete/content", args => new StreamReader(Request.Body).ReadToEnd());

            Get("/delay/{ms:int}", args =>
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(args.ms));
                return $"Finished sleeping for {args.ms}ms";
            });

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
