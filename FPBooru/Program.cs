using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Threading;
using Nancy;
using Nancy.Hosting.Self;

namespace FPBooru
{
	static class Program
	{
		static void Main(string[] args)
		{
			using (var host = new NancyHost(new Uri("http://localhost:80")))
			{
				host.Start();
				Console.WriteLine("Listening on localhost:80");
				Thread.Sleep(Timeout.Infinite);
			}
		}
	}

	public class Router : NancyModule
	{
		private MySqlConnection conn;
		private PageBuilder pb;

		private static string MYSQL_IP = "localhost";
		private static string MYSQL_USER = "root";
		private static string MYSQL_PASS = "hellainsecure";

		public Router()
		{
			this.conn = new MySqlConnection("Server=" + MYSQL_IP + ";Database=fpbooru;Uid=" + MYSQL_USER + ";Pwd=" + MYSQL_PASS + ";SslMode=Preferred;ConvertZeroDateTime=True;");
			this.pb = new PageBuilder();

			Get["/"] = ctx => {
				string outputbuf = "";
				int page = 0;
				outputbuf += pb.GetHeader(Auth.ValidateSessionCookie(ctx.Request.Headers["SeSSION"], conn));
				outputbuf += "<div id=\"interstial\">";
				outputbuf += "<h1>The Front Page.</h1>";
				outputbuf += "The cream of the crop, the best of the best. Community submitted images, voted on by the community.";
				outputbuf += "</div>";
				outputbuf += "Page " + page+1;
				outputbuf += "<div id=\"mainbody\">";
				outputbuf += pb.GetImageGrid(ImageDBConn.GetImages(conn, page));
				outputbuf += "</div>";
				return Negotiate
					.WithContentType("text/html")
					.WithHeader("cache-control", "public, max-age=300")
					.WithModel(outputbuf);
			};

			Post["/login"] = ctx => {
				string outputbuf = "";
				return Negotiate
					.WithContentType("text/html")
					.WithHeader("cache-control", "private, max-age=0, no-store, no-cache")
					.WithModel(outputbuf);
			};

			Get["/show/{id:int}"] = ctx => {
				string outputbuf = "";
				return Negotiate
					.WithContentType("text/html")
					.WithHeader("cache-control", "public, max-age=300")
					.WithModel(outputbuf);
			};

			Get["/user/{id:string}"] = ctx => {
				string outputbuf = "";
				return Negotiate
					.WithContentType("text/html")
					.WithHeader("cache-control", "public, max-age=3600")
					.WithModel(outputbuf);
			};

			Get["/artists"] = ctx => {
				string outputbuf = "";
				return Negotiate
					.WithContentType("text/html")
					.WithHeader("cache-control", "public, max-age=3600")
					.WithModel(outputbuf);
			};

			Get["/search"] = ctx => {
				string outputbuf = "";
				return Negotiate
					.WithContentType("text/html")
					.WithHeader("cache-control", "public, max-age=300")
					.WithHeader("vary", "cookie")
					.WithModel(outputbuf);
			};

			Post["/upload"] = ctx => {
				string outputbuf = "";
				return Negotiate
					.WithContentType("text/html")
					.WithHeader("cache-control", "private, max-age=0, no-store, no-cache")
					.WithModel(outputbuf);
			};

			Get["/upload"] = ctx => {
				string outputbuf = "";
				return Negotiate
					.WithContentType("text/html")
					.WithHeader("cache-control", "public, max-age=86400")
					.WithModel(outputbuf);
			};

			Get["/static/{filename*}"] = ctx => Negotiate.WithStatusCode(502).WithHeader("cache-control", "private, max-age=0, no-store, no-cache").WithModel("Internal server error: This subdirectory should be served by nginx");
		}
	}
}
