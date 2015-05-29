using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Nancy;
using Nancy.Conventions;
using Nancy.Cookies;
using Nancy.Hosting.Self;
using Nancy.ViewEngines;

namespace FPBooru
{
	static class Program
	{
		public static Uri OurHost = new Uri("http://192.168.56.101:8097");

		#if AGPLRelease
		public static bool IsAGPL = true;
		#elif MITRelease
		public static bool IsAGPL = false;
		#else
		#error No license specified! Define either AGPLRelease or MITRelease!
		#endif

		static void Main(string[] args)
		{
			HostConfiguration hc = new HostConfiguration();
			hc.UrlReservations.CreateAutomatically = true;
			#if DEBUG
			//There is a null reference exception thrown in the NancyHost functions somewhere, but I can't debug it.
			hc.UnhandledExceptionCallback = new Action<Exception>((exc)=>{
				Console.WriteLine(exc);
				Debugger.Break();
			});
			StaticConfiguration.DisableErrorTraces = false;
			StaticConfiguration.EnableRequestTracing = true;
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
		private PluginManager plugman;

		private static string MYSQL_IP = "localhost";
		private static string MYSQL_USER = "root";
		private static string MYSQL_PASS = "hellainsecure";

		public Router()
		{
			this.conn = new MySqlConnection("Server=" + MYSQL_IP + ";Database=fpbooru;Uid=" + MYSQL_USER + ";Pwd=" + MYSQL_PASS + ";SslMode=Preferred;ConvertZeroDateTime=True;");
			conn.Open();
			this.plugman = new PluginManager(conn);
			this.pb = new PageBuilder(plugman, conn);
			this.imgconn = new ImageDBConn(conn);

			Get["/"] = ctx => {
				string outputbuf = "";
				long page;
				outputbuf += pb.GetHeader(Request);
				outputbuf += "<div class=\"interstial color primary\">";
				outputbuf += "<h1>The Front Page.</h1>";
				outputbuf += "The perfect mix of new and popular. See what the site and community has to offer.";
				outputbuf += "</div>";
				outputbuf += pb.GetPageIndicator(Request, out page);
				outputbuf += "<div id=\"mainbody\">";
				outputbuf += pb.GetImageGrid(imgconn.GetImages(page));
				outputbuf += "</div>";
				outputbuf += pb.GetBottom();
				return Negotiate
					.WithContentType("text/html")
					.WithHeader("cache-control", "public, s-maxage=30, max-age=5")
					.WithView("dummy.rawhtml")
					.WithModel(outputbuf);
			};

			Get["/about"] = ctx => {
				string outputbuf = "";
				outputbuf += pb.GetHeader(Request);
				outputbuf += "<div class=\"interstial color primary\">";
				#if DEBUG
				outputbuf += "<h1>Debug/Developer Information</h1>";
				outputbuf += pb.GetTable(new [] {"Info", "Value"}, new string[][] {
					new [] {"Operating System", System.Environment.OSVersion.VersionString},
					new [] {"Runtime Version", System.Environment.Version.ToString()},
					new [] {"Is 64 bit?", System.Environment.Is64BitProcess.ToString()},
					new [] {"Command Line", System.Environment.CommandLine},
					new [] {"Culture", Thread.CurrentThread.CurrentCulture.EnglishName},
					new [] {"License", (Program.IsAGPL?"AGPL":"MIT")}
				});
				outputbuf += "<br />";
				#endif
				outputbuf += "<h1>About</h1>";
				outputbuf += "FPBooru is dual-licensed, both as free open-source software under the" +
					" <a href=\"https://www.gnu.org/licenses/agpl-3.0.txt\">GNU Affero General Public License</a>, and the more" +
					" permissive <a href=\"http://opensource.org/licenses/MIT\">MIT License</a>. There may be missing features from" +
					" the MIT release compared to the AGPL release, depending on the programmer who implements the feature, and" +
					" his/hers license preference. This version is an " + (Program.IsAGPL?"AGPL build.":"MIT build.") + " The source" +
					" is available on <a href=\"https://github.com/ChlorideCull/FPBooru\">GitHub</a>.";
				outputbuf += pb.GetBottom();
				return Negotiate
					.WithContentType("text/html")
					.WithHeader("cache-control", "public, max-age=18000")
					.WithView("dummy.rawhtml")
					.WithModel(outputbuf);
			};

			Get["/register"] = ctx => {
				string outputbuf = "";

				outputbuf += pb.GetHeader(Request);
				outputbuf += "<div class=\"interstial color contrast2\">";
				outputbuf += "<form action=\"/register\" method=\"post\" enctype=\"multipart/form-data\">";
				outputbuf += pb.GetTable(new string[] {}, new string[][] {
					new [] { "<label for=\"pass\">Username</label>", "<input type=\"text\" name=\"user\" />" },
					new [] { "<label for=\"pass\">Password</label>", "<input type=\"password\" name=\"pass\" />" },
					new [] { "<label for=\"passrep\">Repeat Password</label>", "<input type=\"password\" name=\"passrep\" />" },
					new [] { "<label for=\"email\">EMail</label>", "<input type=\"email\" name=\"email\" />" }
				});
				outputbuf += "<input type=\"submit\" value=\"Register\" />";
				outputbuf += "</form>";
				outputbuf += "</div>";
				outputbuf += pb.GetBottom();

				return Negotiate
					.WithContentType("text/html")
					.WithHeader("cache-control", "public, max-age=18000")
					.WithView("dummy.rawhtml")
					.WithModel(outputbuf);
			};

			Post["/register"] = ctx => {
				string outputbuf = "";
				string username = ((DynamicDictionary)Context.Request.Form)["user"].Value;
				string password = ((DynamicDictionary)Context.Request.Form)["pass"].Value;
				if (password == ((DynamicDictionary)Context.Request.Form)["passrep"].Value) {
					bool regdone = Auth.RegisterUser(plugman, username, password, conn);
					if (regdone) {
						string cookie = Auth.AuthenticateUser(plugman, username, password, conn);
						if (cookie == null)
							throw new NullReferenceException("Failed to authenticate with newly registered user");
						outputbuf += pb.GetHeader(Request);
						outputbuf += "<div class=\"interstial color contrast2\">";
						outputbuf += "<h1>Thank you for registering, " + pb.Sanitize(((DynamicDictionary)Context.Request.Form)["user"].Value) + "!</h1>";
						outputbuf += "<a href=\"/\">Return to the front page</a>";
						outputbuf += "</div>";
						outputbuf += pb.GetBottom();
						//Gets stuck in Negotiate?
						return Negotiate
							.WithCookie(new NancyCookie("SeSSION", cookie))
							.WithHeader("cache-control", "private, max-age=0, no-store, no-cache")
							.WithView("dummy.rawhtml")
							.WithModel(outputbuf);
					}
				}
				outputbuf += pb.GetHeader(Request);
				outputbuf += "<div class=\"interstial color contrast2\">";
				outputbuf += "<h1>Registration Failed!</h1>";
				outputbuf += "<a href=\"/register\">Try Again</a> ";
				outputbuf += "<a href=\"/\">Return to the front page</a>";
				outputbuf += "</div>";
				outputbuf += pb.GetBottom();
				return Negotiate
					.WithStatusCode(Nancy.HttpStatusCode.InternalServerError)
					.WithHeader("cache-control", "private, max-age=0, no-store, no-cache")
					.WithView("dummy.rawhtml")
					.WithModel(outputbuf);
			};

			Get["/login"] = ctx => {
				string outputbuf = "";

				outputbuf += pb.GetHeader(Request);
				outputbuf += "<div class=\"interstial color contrast2\">";
				outputbuf += "<h1>Login or <a href=\"/register\">Register</a></h1>";
				outputbuf += "<form action=\"/login\" method=\"post\" enctype=\"multipart/form-data\">";
				outputbuf += pb.GetTable(new string[] {}, new string[][] {
					new [] { "<label for=\"pass\">Username</label>", "<input type=\"text\" name=\"user\" />" },
					new [] { "<label for=\"pass\">Password</label>", "<input type=\"password\" name=\"pass\" />" }
				});
				outputbuf += "<input type=\"submit\" value=\"Login\" />";
				outputbuf += "</form>";
				outputbuf += "</div>";
				outputbuf += pb.GetBottom();

				return Negotiate
					.WithContentType("text/html")
					.WithHeader("cache-control", "public, max-age=18000")
					.WithView("dummy.rawhtml")
					.WithModel(outputbuf);
			};

			Post["/login"] = ctx => {
				/* Bug? Mono seems to be confused "`Nancy.DynamicDictionaryValue' does not contain a definition for `Form'"
				 * yet the /upload POST route seems to work, being written the same way. We circumvent it by casting Form to
				 * a DynamicDictionary, which it seems to always be.
				 * 
				 * Before:
				 *      Context.Request.Form["user"].Value
				 * After:
				 *      ((DynamicDictionary)Context.Request.Form)["user"].Value
				 */
				string cookie = Auth.AuthenticateUser(plugman, ((DynamicDictionary)Context.Request.Form)["user"].Value, ((DynamicDictionary)Context.Request.Form)["pass"].Value, conn);
				if (cookie != null) {
					string outputbuf = pb.GetHeader(Request);
					outputbuf += "<div class=\"interstial color contrast2\">";
					outputbuf += "<h1>Welcome, " + pb.Sanitize(((DynamicDictionary)Context.Request.Form)["user"]) + "</h1>";
					outputbuf += "<a href=\"/\">Return to the front page</a>";
					outputbuf += "</div>";
					outputbuf += pb.GetBottom();
					return Negotiate
						.WithCookie(new NancyCookie("SeSSION", cookie))
						.WithHeader("cache-control", "private, max-age=0, no-store, no-cache")
						.WithView("dummy.rawhtml")
						.WithModel(outputbuf);
				} else {
					string outputbuf = pb.GetHeader(Request);
					outputbuf += "<div class=\"interstial color contrast2\">";
					outputbuf += "<h1>Login failed.</h1>";
					outputbuf += "<a href=\"/login\">Return to the login page</a><br />";
					outputbuf += "<a href=\"/register\">Create an account</a><br />";
					outputbuf += "</div>";
					outputbuf += pb.GetBottom();
					return Negotiate
						.WithHeader("cache-control", "private, max-age=0, no-store, no-cache")
						.WithView("dummy.rawhtml")
						.WithModel(outputbuf);
				}
			};

			Get["/image/{id:long}"] = ctx => {
				string outputbuf = "";
				Image img = imgconn.GetImage(Context.Parameters["id"]);
				outputbuf += pb.GetHeader(Request);
				outputbuf += "<div class=\"centerfix\">";
				foreach (string imagepath in img.imagenames) {
					outputbuf += "<img class=\"fullimage\" src=\"/static/images/" + imagepath + "\" />";
				}
				outputbuf += "</div>";
				outputbuf += "<div class=\"interstial color contrast2\">";
				string ourtags = "";
				foreach (long tagid in img.tagids)
					ourtags += imgconn.ResolveTag(tagid) + ", ";
				outputbuf += pb.GetTable(new string[] {}, new string[][] {
					new [] {"Tags", pb.CreateTagEditor(ourtags, true)},
					new [] {"Uploader", img.uploader}
				});
				outputbuf += "</div>";
				outputbuf += pb.GetBottom();
				return Negotiate
					.WithContentType("text/html")
					.WithHeader("cache-control", "public, max-age=3600")
					.WithView("dummy.rawhtml")
					.WithModel(outputbuf);
			};

			Get["/tag/{id:long}"] = ctx => {
				string outputbuf = "";
				long page;
				outputbuf += pb.GetHeader(Request);
				outputbuf += pb.GetPageIndicator(Request, out page);
				outputbuf += "<div id=\"mainbody\">";
				outputbuf += pb.GetImageGrid(imgconn.GetImages(page, new long[] {this.Context.Parameters["id"]}));
				outputbuf += "</div>";
				return Negotiate
					.WithContentType("text/html")
					.WithHeader("cache-control", "public, max-age=3600")
					.WithView("dummy.rawhtml")
					.WithModel(outputbuf);
			};

			Get["/user/{id}"] = ctx => {
				string outputbuf = "";
				outputbuf += pb.GetHeader(Request);
				outputbuf += "<div class=\"centerfix\">";
				outputbuf += "<div class=\"interstial color primary user\">" +
						"<img id=\"userAvatar\" class=\"color contrast2\" src=\"http://www.gravatar.com/avatar/invalid?s=256\" />" +
						"<div id=\"userPins\" class=\"color contrast2\">" +
							/*"<div class=\"userPin color-contrast3\">" +
								"<img src=\"http://i.imgur.com/RDDaGeq.png\" />" +
								"<div>" +
									"<h1>root</h1>" +
									"<h2>I am become shell, destroyer of servers</h2>" +
									"<p>Has server-level access.</p>" +
								"</div>" +
							"</div>" +*/
					"</div>" +
					"<div id=\"userData\">" +
						"<div id=\"userTitle\">" +
							"<h1>" + pb.Sanitize(ctx.id) + "</h1>" +
							"<span> @" + pb.Sanitize(ctx.id) + "</span>" +
						"</div>" +
						"<hr />" +
					/*"<table>" +
							"<tr>" +
								"<td>Twitter</td>" +
								"<td>@ChlorideCull</td>" +
							"</tr>" +
						"</table>" +
						"<hr />" +*/
						"<p id=\"userDesc\">" +
							"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor\n      in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum." +
						"</p>" +
					"</div>" +
				"</div>";
				outputbuf += "</div>";
				outputbuf += pb.GetBottom();
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
				System.IO.FileStream mainfile = System.IO.File.Create(System.IO.Path.GetFullPath("static/images/" + name + System.IO.Path.GetExtension(file.Name).ToLower()));

				file.Value.CopyTo(mainfile);
				mainfile.Close();
				output += "--> File retrieved\n";

				bool failed = false;
				string extension = System.IO.Path.GetExtension(file.Name);
				for (int i = 0; i < 3; i++) {
					output += "--> Generating thumbnail\n";
					output += FileIO.GenerateImage("static/thumbs/",
						System.IO.Path.GetFullPath("static/images/" + name) + extension,
						"648x324", "jpg", out failed);

					output += "--> Generating header\n";
					output += FileIO.GenerateImage("static/headers/",
						System.IO.Path.GetFullPath("static/images/" + name) + extension,
						"1920x100", "png", out failed);
					
					output += "--> ImageMagick reports " + (failed?"that it didn't work. Attempting repairs.":"nothing unusual.") + "\n";
					if (failed) {
						output += FileIO.RepairImage(System.IO.Path.GetFullPath("static/images/" + name) + extension, out failed);
						if (failed) {
							output += "--> Repair failed. Bailing.\n";
							break;
						} else {
							output += "--> Repair successful. Eventual animation might be missing.\n";
							extension = ".png";
						}
					} else {
						break;
					}
				}

				string tagstring = pb.Sanitize(Context.Request.Form["tags"].Value);
				output += "--> Tags registered as \"" + tagstring + "\"\n";
				List<long> tags = new List<long>();
				foreach (string tagname in tagstring.Split(new [] {','}, StringSplitOptions.RemoveEmptyEntries)) {
					string tagnamepretty = tagname.Trim();
					if (tagnamepretty == "")
						continue;
					tags.Add(imgconn.ResolveTag(tagnamepretty, true));
					output += "---> Tag \"" + tagnamepretty + "\" was given ID " + tags.Last() + ".\n";
				}

				long ourid = 0;
				if (!failed) {
					//Add to the database, resolve tags, create them if not found.
					Image img = new Image();
					img.imagenames = new string[] {name + System.IO.Path.GetExtension(file.Name)};
					img.thumbnailname = name + ".jpg";
					img.uploader = (Auth.GetUserFromSessionCookie(plugman, Context.Request.Headers["SeSSION"].FirstOrDefault(), conn) ?? "Anonymous");
					img.tagids = tags.ToArray();
					ourid = imgconn.AddImage(img);
					output += "--> Image registered in database as ID " + ourid + ".\n";
				} else {
					#if DEBUG
					output += "--> File is named " + name + System.IO.Path.GetExtension(file.Name);
					#else
					File.Delete(System.IO.Path.GetFullPath("static/images/" + name) + System.IO.Path.GetExtension(file.Name));
					output += "--> Image deleted to save space.\n";
					#endif
				}

				if ((ourid != 0) && !failed) {
					string outputbuf = "";
					outputbuf += pb.GetHeader(Request);
					outputbuf += "<div class=\"interstial color primary\">";
					outputbuf += "<h1>Upload complete!</h1>";
					outputbuf += "You can view your image <a href=\"/image/" + ourid + "\">here</a>.";

					#if DEBUG
					outputbuf += "<br />";
					outputbuf += "Guru Meditation: <br /><pre>";
					outputbuf += output;
					outputbuf += "</pre>";
					#endif
					outputbuf += "</div>";
					outputbuf += pb.GetBottom();
					return Negotiate
						.WithContentType("text/html")
						.WithHeader("cache-control", "private, max-age=0, no-store, no-cache")
						.WithView("dummy.rawhtml")
						.WithModel(outputbuf);
				} else {
					string outputbuf = "";
					outputbuf += pb.GetHeader(Request);
					outputbuf += "<div class=\"interstial color primary\">";
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
				outputbuf += pb.GetHeader(Request);
				outputbuf += "<div class=\"interstial color contrast2\">";
				outputbuf += "<form action=\"/upload\" method=\"post\" enctype=\"multipart/form-data\">";
				outputbuf += "Currently supported files are: GIF, JPG, PNG, SVG and WebP<br />";
				outputbuf += pb.GetTable(new string[] {}, new string[][] {
					new [] {
						"<label for=\"img\">File</label>",
						"<input type=\"file\" name=\"img\" accept=\"image/gif,image/jpeg,image/png,image/svg+xml,image/webp\" /><br />"
					},
					new [] {
						"<label for=\"tags\">Tags</label>",
						pb.CreateTagEditor("", false) + "<br />"
					},
				});
				outputbuf += "<input type=\"submit\" value=\"Upload\" />";
				outputbuf += "</form>";
				outputbuf += "</div>";
				outputbuf += pb.GetBottom();
				return Negotiate
					.WithContentType("text/html")
					.WithHeader("cache-control", "public, max-age=86400")
					.WithView("dummy.rawhtml")
					.WithModel(outputbuf);
			};
		}
	}
}
