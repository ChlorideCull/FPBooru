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
    }
}

