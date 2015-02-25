using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Threading;

namespace FPBooru
{
    static class Program
    {
        private static MySqlConnection conn;
        private static bool Quit = false;

        static void Main(string[] args)
        {
            HttpListener hl = new HttpListener();
            hl.Prefixes.Add("http://*:80/");
            hl.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            hl.Start();
            Console.WriteLine("Listening on *:80");
            while (!Quit) {
                new Thread(new Router(hl.GetContext()).Process).Start();
            }
        }
    }

    class Router
    {
        private HttpListenerContext contxt;
        public Router(HttpListenerContext context)
        {
            Console.WriteLine(context.Request.UserHostAddress + " - " + context.Request.HttpMethod + " " + context.Request.RawUrl);
            this.contxt = context;
        }

        public void Process()
        {
            if (contxt.Request.Url.AbsolutePath == "/")
            {
                contxt.Response.AddHeader("cache-control", "public, max-age=300");
            }
            else if (contxt.Request.Url.AbsolutePath == "/login")
            {
                contxt.Response.AddHeader("cache-control", "private, max-age=0, no-store, no-cache");
            }
            else if (contxt.Request.Url.AbsolutePath == "/show")
            {
                contxt.Response.AddHeader("cache-control", "public, max-age=300");
            }
            else if (contxt.Request.Url.AbsolutePath == "/artists")
            {
                contxt.Response.AddHeader("cache-control", "public, max-age=3600");
            }
            else if (contxt.Request.Url.AbsolutePath == "/search")
            {
                contxt.Response.AddHeader("cache-control", "public, max-age=300");
                contxt.Response.AddHeader("vary", "cookie");
            }
            else if (contxt.Request.Url.AbsolutePath == "/upload" && contxt.Request.HttpMethod == "POST")
            {
                contxt.Response.AddHeader("cache-control", "private, max-age=0, no-store, no-cache");
            }
            else if (contxt.Request.Url.AbsolutePath == "/upload" && contxt.Request.HttpMethod == "GET")
            {
                contxt.Response.AddHeader("cache-control", "public, max-age=86400");
            }
            else if (contxt.Request.Url.AbsolutePath.StartsWith("/static"))
            {
                contxt.Response.AddHeader("cache-control", "public, max-age=86400");
            }
            else
            {
                contxt.Response.AddHeader("cache-control", "private, max-age=0, no-store, no-cache");
                contxt.Response.StatusCode = 404;
            }
        }
    }
}
