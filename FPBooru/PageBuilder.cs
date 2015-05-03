using System;
using System.IO;
using Nancy;

namespace FPBooru
{
	public class PageBuilder
	{
		private string rawHeader;
		private string rawBottom;

		public PageBuilder()
		{
			rawHeader = File.ReadAllText("template/top.html");
			rawBottom = File.ReadAllText("template/bottom.html");
		}

		public string GetHeader(string UserName) {
			string tmp = rawHeader;
			if (UserName != null)
				tmp = tmp.Replace("%_-USRSTATUS-_%", "<a class=\"noButton\" href=\"/user/" + UserName + "\">Hello, " + UserName + "!</a>");
			else
				tmp = tmp.Replace("%_-USRSTATUS-_%", "<a class=\"alignRight\" onclick=\"doLogin()\">Login</a>");
			return tmp;
		}

		public string GetBottom() {
			return rawBottom;
		}

		public string GetImageGrid(Image[] images) {
			string output = "";
			foreach (Image img in images) {
				output += "<div class=\"pic\"><a href=\"/image/" + img.id + "\"><img src=\"/static/thumbs/" + img.thumbnailname + "\" /></a></div>";
			}
			return output;
		}

		public string GetPageIndicator(Request rqst, out uint page) {
			try {
				page = (rqst.Query.page ?? 0);
			} catch (InvalidCastException _) {
				page = 0;
			}
			string output = "";
			output += "<div id=\"interstial\">";
			output += (page == 0)?"":"<a href=\"" + rqst.Url.Path + "?page=" + (page-1) + "\">Back</a> ";
			output += "<span>Page " + (page+1) + "</span>";
			output += " <a href=\"" + rqst.Url.Path + "?page=" + (page+1) + "\">Forward</a>";
			output += "</div>";
			return output;
		}
	}
}

