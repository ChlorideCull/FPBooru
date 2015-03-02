using System;
using System.IO;

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
				tmp = tmp.Replace("%_-USRSTATUS-_%", "<a class=\"noButton\" href=\"/user?id=" + UserName + "\">Hello, " + UserName + "!</a>");
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
				output += "<a href=\"/show/" + img.id + "\" class=\"pic\"><img src=\"static/thumbs/" + img.imagepaths[0] + ".png\" /></a>";
			}
		}
	}
}

