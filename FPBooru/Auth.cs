using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Net;

namespace FPBooru
{
    static class Auth
    {
        public static string ValidateSessionCookie(Cookie cookie, MySqlConnection conn)
        {
            MySqlCommand cmd = new MySqlCommand("SELECT username FROM fpbooru.usrs WHERE session = \"@sess\"", conn);
            cmd.Parameters["@sess"].Value = cookie.Value;
            MySqlDataReader red = cmd.ExecuteReader();
            foreach (string user in red)
            {
                return user;
            }
            return null;
        }

        public static Cookie GetSessionCookie(string user, MySqlConnection conn)
        {
            return new Cookie();
        }

        public static Cookie ResetSessionCookie(string user, MySqlConnection conn)
        {
            return new Cookie();
        }
    }
}
