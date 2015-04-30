using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Threading;
using Nancy;
using Nancy.Hosting.Self;
using Nancy.ViewEngines;
using System.Diagnostics;
using System.IO;
using Nancy.Conventions;

namespace FPBooru
{
	static class Program
	{
		public static Uri OurHost = new Uri("http://192.168.56.101:8097");

		static void Main(string[] args)
		{
			HostConfiguration hc = new HostConfiguration();
			hc.UrlReservations.CreateAutomatically = true;
			#if DEBUG
			StaticConfiguration.DisableErrorTraces = false;
			#endif
			using (var host = new NancyHost(hc, OurHost))
			{
				host.Start();
				Console.WriteLine("Listening on " + OurHost);
				Thread.Sleep(Timeout.Infinite);
			}
		}
	}

	public class CustomBootstrap : DefaultNancyBootstrapper { 
		protected override IEnumerable<Type> ViewEngines {
			get {
				return new Type[] { typeof(RawViewEngine) };
			}
		}

		protected override void ConfigureConventions(Nancy.Conventions.NancyConventions nancyConventions) {
			base.ConfigureConventions(nancyConventions);
			nancyConventions.StaticContentsConventions.Clear();
			nancyConventions.StaticContentsConventions.Add(
				StaticContentConventionBuilder.AddDirectory("static", @"static/")
			);
		}

		protected override void ApplicationStartup(Nancy.TinyIoc.TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines) {
			pipelines.AfterRequest.AddItemToEndOfPipeline(new Action<NancyContext>(ctx => {
				string etag = ctx.Request.Headers.IfNoneMatch.FirstOrDefault();
				string remoteetag;
				if (ctx.Response.Headers.TryGetValue("ETag", out remoteetag) && (etag == remoteetag)) {
					ctx.Response.StatusCode = Nancy.HttpStatusCode.NotModified;
					ctx.Response.Contents = null;
				}
			}));
			base.ApplicationStartup(container, pipelines);
		}
	}

	public class Router : NancyModule
	{
		private MySqlConnection conn;
		private PageBuilder pb;
		private ImageDBConn imgconn;

		private static string MYSQL_IP = "localhost";
		private static string MYSQL_USER = "root";
		private static string MYSQL_PASS = "hellainsecure";

		public Router()
		{
			this.conn = new MySqlConnection("Server=" + MYSQL_IP + ";Database=fpbooru;Uid=" + MYSQL_USER + ";Pwd=" + MYSQL_PASS + ";SslMode=Preferred;ConvertZeroDateTime=True;");
			conn.Open();
			this.pb = new PageBuilder();
			this.imgconn = new ImageDBConn(conn);


			Get["/"] = ctx => {
				string outputbuf = "";
				int page = 0;
				outputbuf += pb.GetHeader(Auth.GetUserFromSessionCookie(this.Request.Headers["SeSSION"].FirstOrDefault(), conn));
				outputbuf += "<div id=\"interstial\">";
				outputbuf += "<h1>The Front Page.</h1>";
				outputbuf += "The cream of the crop, the best of the best. Community submitted images, voted on by the community.";
				outputbuf += "</div>";
				outputbuf += "<span id=\"pageind\">Page " + page+1 + "</span>";
				outputbuf += "<div id=\"mainbody\">";
				outputbuf += pb.GetImageGrid(imgconn.GetImages(0));
				outputbuf += "</div>";
				outputbuf += pb.GetBottom();
				return Negotiate
					.WithContentType("text/html")
					.WithHeader("cache-control", "public, max-age=60")
					.WithView("dummy.rawhtml")
					.WithModel(outputbuf);
			};

			Get["/about"] = ctx => {
				string outputbuf = "";
				return Negotiate
					.WithContentType("text/html")
					.WithHeader("cache-control", "public, max-age=300")
					.WithView("dummy.rawhtml")
					.WithModel(outputbuf);
			};

			Get["/login"] = ctx => {
				string outputbuf = "";

				outputbuf += pb.GetHeader(Auth.GetUserFromSessionCookie(this.Request.Headers["SeSSION"].FirstOrDefault(), conn));
				outputbuf += "<form action=\"login\" method=\"post\">";
				outputbuf += "Username: <input type=\"text\" name=\"user\" />";
				outputbuf += "Password: <input type=\"password\" name=\"pass\" />";
				outputbuf += "<input type=\"submit\" value=\"Login\" />";
				outputbuf += "</form>";
				outputbuf += pb.GetBottom();

				return Negotiate
					.WithContentType("text/html")
					.WithHeader("cache-control", "public, max-age=18000")
					.WithView("dummy.rawhtml")
					.WithModel(outputbuf);
			};

			Post["/login"] = ctx => {
				string cookie = Auth.AuthenticateUser(ctx.Request.Form["user"], (new SHA256Managed()).ComputeHash(System.Text.Encoding.UTF8.GetBytes(ctx.Request.Form["pass"])), conn);
				if (cookie != null) {
					return Negotiate
						.WithStatusCode(Nancy.HttpStatusCode.TemporaryRedirect)
						.WithHeader("Set-Cookie", cookie)
						.WithHeader("Location", "/")
						.WithHeader("cache-control", "private, max-age=0, no-store, no-cache");
				} else {
					return Negotiate
						.WithStatusCode(Nancy.HttpStatusCode.TemporaryRedirect)
						.WithHeader("Location", "/login")
						.WithHeader("cache-control", "private, max-age=0, no-store, no-cache");
				}
			};

			Get["/image/{id:int}"] = ctx => {
				string outputbuf = "";
				return Negotiate
					.WithContentType("text/html")
					.WithHeader("cache-control", "public, max-age=3600")
					.WithView("dummy.rawhtml")
					.WithModel(outputbuf);
			};

			Get["/tag/{id:int}"] = ctx => {
				string outputbuf = "";
				int page = 0;
				outputbuf += "<span id=\"pageind\">Page " + page+1 + "</span>";
				outputbuf += "<div id=\"mainbody\">";
				outputbuf += pb.GetImageGrid(imgconn.GetImages(page, this.Context.Parameters["id"]));
				outputbuf += "</div>";
				return Negotiate
					.WithContentType("text/html")
					.WithHeader("cache-control", "public, max-age=3600")
					.WithView("dummy.rawhtml")
					.WithModel(outputbuf);
			};

			Get["/user/{id:string}"] = ctx => {
				string outputbuf = "";
				return Negotiate
					.WithContentType("text/html")
					.WithHeader("cache-control", "public, max-age=3600")
					.WithView("dummy.rawhtml")
					.WithModel(outputbuf);
			};

			Get["/artists"] = ctx => {
				string outputbuf = "";
				return Negotiate
					.WithContentType("text/html")
					.WithHeader("cache-control", "public, max-age=3600")
					.WithView("dummy.rawhtml")
					.WithModel(outputbuf);
			};

			Get["/search"] = ctx => {
				string outputbuf = "";
				return Negotiate
					.WithContentType("text/html")
					.WithHeader("cache-control", "public, max-age=300")
					.WithHeader("vary", "cookie")
					.WithView("dummy.rawhtml")
					.WithModel(outputbuf);
			};

			Post["/upload"] = ctx => {
				string output = "";

				//Process the file
				HttpFile file = this.Context.Request.Files.FirstOrDefault();
				string name = ((long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds) + "_" + (new Random()).Next();
				System.IO.FileStream mainfile = System.IO.File.Create(System.IO.Path.GetFullPath("static/images/" + name + System.IO.Path.GetExtension(file.Name)));

				file.Value.CopyTo(mainfile);
				mainfile.Close();
				output += "--> File retrieved\n";

				ProcessStartInfo psi;
				Process ps;
				bool failed = false;

				output += "--> Generating thumbnail\n";
				if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
					psi = new ProcessStartInfo("mogrify.exe");
					psi.Arguments = "-path static/thumbs/ -thumbnail 648x324^^ -gravity center -extent 648x324 -format jpg " + System.IO.Path.GetFullPath("static/images/" + name) + System.IO.Path.GetExtension(file.Name);
				} else {
					psi = new ProcessStartInfo("mogrify");
					psi.Arguments = "-path static/thumbs/ -thumbnail 648x324^ -gravity center -extent 648x324 -format jpg \"" + System.IO.Path.GetFullPath("static/images/" + name) + System.IO.Path.GetExtension(file.Name) + "\"";
				}
				psi.RedirectStandardError = true;
				psi.UseShellExecute = false;
				ps = Process.Start(psi);
				output += ps.StandardError.ReadToEnd();
				while (!ps.HasExited)
					Thread.Sleep(0);
				failed = (ps.ExitCode != 0);

				output += "--> Generating header\n";
				if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
					psi = new ProcessStartInfo("mogrify.exe");
					psi.Arguments = "-path static/headers/ -thumbnail 1920x100^^ -gravity center -extent 1920x100 -format png " + System.IO.Path.GetFullPath("static/images/" + name) + System.IO.Path.GetExtension(file.Name);
				} else {
					psi = new ProcessStartInfo("mogrify");
					psi.Arguments = "-path static/headers/ -thumbnail 1920x100^ -gravity center -extent 1920x100 -format png \"" + System.IO.Path.GetFullPath("static/images/" + name) + System.IO.Path.GetExtension(file.Name)+ "\"";
				}
				psi.RedirectStandardError = true;
				psi.UseShellExecute = false;
				ps = Process.Start(psi);
				output += ps.StandardError.ReadToEnd();
				while (!ps.HasExited)
					Thread.Sleep(0);
				failed = (ps.ExitCode != 0);

				output += "--> Mogrify reports " + (failed?"that it didn't work. Bailing.":"nothing unusual.") + "\n";
				long ourid = 0;
				if (!failed) {
					//Add to the database, resolve tags, create them if not found.
					Image img = new Image();
					img.imagenames = new string[] {name + System.IO.Path.GetExtension(file.Name)};
					img.thumbnailname = name + ".jpg";
					img.tagids = new long[] {};
					ourid = imgconn.AddImage(img);
					output += "--> Image registered in database as ID " + ourid + "\n";
				} else {
					File.Delete(System.IO.Path.GetFullPath("static/images/" + name) + System.IO.Path.GetExtension(file.Name));
					output += "--> Image deleted to save space.\n";
				}

				if ((ourid != 0) && !failed) {
					string outputbuf = "";
					outputbuf += pb.GetHeader(Auth.GetUserFromSessionCookie(this.Request.Headers["SeSSION"].FirstOrDefault(), conn));
					outputbuf += "<div id=\"interstial\">";
					outputbuf += "<h1>Upload complete!</h1>";
					outputbuf += "You can view your image <a href=\"/image/" + ourid + "\">here</a>.";

					outputbuf += "<br />";
					outputbuf += "Guru Meditation: <br /><pre>";
					outputbuf += output;
					outputbuf += "</pre></div>";
					outputbuf += pb.GetBottom();
					return Negotiate
						.WithContentType("text/html")
						.WithHeader("cache-control", "private, max-age=0, no-store, no-cache")
						.WithView("dummy.rawhtml")
						.WithModel(outputbuf);
				} else {
					string outputbuf = "";
					outputbuf += pb.GetHeader(Auth.GetUserFromSessionCookie(this.Request.Headers["SeSSION"].FirstOrDefault(), conn));
					outputbuf += "<div id=\"interstial\">";
					outputbuf += "<h1>There was an error uploading your image!</h1>";
					outputbuf += "If you believe there is something wrong with the server, contact an admin with the log below.<br />";
					outputbuf += "There are a couple of things that you can fix yourself, however.<br />";

					outputbuf += "<ul>";
					outputbuf += "<li>If the guru meditation below mentions \"corrupt image\" or \"crc error\", make sure that it is an image, and try reconstructing the file by opening it in paint and saving it as PNG.</li>";
					outputbuf += "</ul>";

					outputbuf += "<br />";
					outputbuf += "Guru Meditation: <br /><pre>";
					outputbuf += output;
					outputbuf += "</pre></div>";
					outputbuf += pb.GetBottom();
					return Negotiate
						.WithContentType("text/html")
						.WithHeader("cache-control", "private, max-age=0, no-store, no-cache")
						.WithView("dummy.rawhtml")
						.WithModel(outputbuf)
						.WithStatusCode(Nancy.HttpStatusCode.InternalServerError);
				}
			};

			Get["/upload"] = ctx => {
				string outputbuf = "";
				outputbuf += pb.GetHeader(Auth.GetUserFromSessionCookie(this.Request.Headers["SeSSION"].FirstOrDefault(), conn));
				outputbuf += "<div id=\"interstial\">";
				outputbuf += "<form action=\"upload\" method=\"post\" enctype=\"multipart/form-data\">";
				outputbuf += "Currently supported files are: GIF, JPG, PNG, SVG, WebP and WebM.<br />";
				outputbuf += "<label for=\"img\">File:</label>";
				outputbuf += "<input type=\"file\" name=\"img\" accept=\"image/gif,image/jpeg,image/png,image/svg+xml,image/webp,video/webm\" /><br />";
				outputbuf += "<label for=\"tags\">Tags:</label>";
				outputbuf += "<input type=\"text\" name=\"tags\" /><br />";
				outputbuf += "<input type=\"submit\" value=\"Upload\" />";
				outputbuf += "</form>";
				outputbuf += "</div>";
				return Negotiate
					.WithContentType("text/html")
					.WithHeader("cache-control", "public, max-age=86400")
					.WithView("dummy.rawhtml")
					.WithModel(outputbuf);
			};
		}
	}
}
