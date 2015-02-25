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
            PageBuilder pb = new PageBuilder();
            while (!Quit) {
                new Thread(new Router(hl.GetContext(), conn, pb).Process).Start();
            }
        }
    }

    class Router
    {
        private HttpListenerContext contxt;
        private MySqlConnection conn;
        private PageBuilder pb;

        public Router(HttpListenerContext context, MySqlConnection connect, PageBuilder pb)
        {
            Console.WriteLine(context.Request.UserHostAddress + " - " + context.Request.HttpMethod + " " + context.Request.RawUrl);
            this.contxt = context;
            this.conn = connect;
            this.pb = pb;
        }

        public void Process()
        {
            if (contxt.Request.Url.AbsolutePath == "/" && contxt.Request.HttpMethod == "GET")
            {
                contxt.Response.AddHeader("cache-control", "public, max-age=300");
                string outputbuf;
                outputbuf += pb.GetHeader(Auth.ValidateSessionCookie(contxt.Request.Cookies["SeSSION"], conn));
            }
            else if (contxt.Request.Url.AbsolutePath == "/login" && contxt.Request.HttpMethod == "POST")
            {
                contxt.Response.AddHeader("cache-control", "private, max-age=0, no-store, no-cache");
            }
            else if (contxt.Request.Url.AbsolutePath == "/show" && contxt.Request.HttpMethod == "GET")
            {
                contxt.Response.AddHeader("cache-control", "public, max-age=300");
            }
            else if (contxt.Request.Url.AbsolutePath == "/artists" && contxt.Request.HttpMethod == "GET")
            {
                contxt.Response.AddHeader("cache-control", "public, max-age=3600");
            }
            else if (contxt.Request.Url.AbsolutePath == "/search" && contxt.Request.HttpMethod == "GET")
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
            else if (contxt.Request.Url.AbsolutePath.StartsWith("/static") && contxt.Request.HttpMethod == "GET")
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
