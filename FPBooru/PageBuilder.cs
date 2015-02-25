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
            rawHeader = File.ReadAllText("template/top.pphtml");
            rawBottom = File.ReadAllText("template/bottom.pphtml");
        }

        public string GetHeader(string UserName, int UserID) {
            string tmp = rawHeader;
            if (UserName != null)
                tmp = tmp.Replace("%_-USRSTATUS-_%", "<a class=\"noButton\" href=\"/user?id=" + UserID + "\">Hello, " + UserName + "!</a>");
            else
                tmp = tmp.Replace("%_-USRSTATUS-_%", "<a class=\"alignRight\" onclick=\"doLogin()\">Login</a>");
            return tmp;
        }

        public string GetBottom() {
            return rawBottom;
        }
    }
}

