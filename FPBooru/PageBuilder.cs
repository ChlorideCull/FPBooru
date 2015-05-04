﻿using System;
using System.IO;
using Nancy;
using MySql.Data.MySqlClient;
using System.Linq;

namespace FPBooru
{
	public class PageBuilder
	{
		private string rawHeader;
		private string rawBottom;
		private MySqlConnection conn;
		private ImageDBConn imgconn;

		public PageBuilder(MySqlConnection mysqlconn)
		{
			conn = mysqlconn;
			imgconn = new ImageDBConn(mysqlconn);
			rawHeader = File.ReadAllText("template/top.html");
			rawBottom = File.ReadAllText("template/bottom.html");
		}

		public string GetHeader(Request rqst) {
			string tmp = rawHeader;

			string UserName = Auth.GetUserFromSessionCookie(rqst.Headers["SeSSION"].FirstOrDefault(), conn);
			if (UserName != null)
				tmp = tmp.Replace("%_-USRSTATUS-_%", "<a class=\"noButton\" href=\"/user/" + UserName + "\">Hello, " + UserName + "!</a>");
			else
				tmp = tmp.Replace("%_-USRSTATUS-_%", "<a class=\"alignRight\" onclick=\"doLogin()\">Login</a>");

			Random rdgen = new Random();
			Image[] candidates = imgconn.GetImages(0);
			Image ourimage = candidates[rdgen.Next(candidates.Length)];
			tmp = tmp.Replace("%_-HDRIMG-_%", System.IO.Path.ChangeExtension("/static/headers/" + ourimage.imagenames[rdgen.Next(ourimage.imagenames.Length)], ".png"));
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
			output += "<div class=\"centerfix\">";
			output += "<div class=\"interstial\">";
			output += (page == 0)?"":"<a href=\"" + rqst.Url.Path + "?page=" + (page-1) + "\">Back</a> ";
			output += "<span>Page " + (page+1) + "</span>";
			output += " <a href=\"" + rqst.Url.Path + "?page=" + (page+1) + "\">Forward</a>";
			output += "</div>";
			output += "</div>";
			return output;
		}
	}
}

