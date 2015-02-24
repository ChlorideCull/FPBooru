using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace FPBooru
{
    class Auth
    {
        static string ValidateSessionCookie(byte[] cookie, MySqlConnection conn)
        {
            MySqlCommand cmd = new MySqlCommand("SELECT username FROM fpbooru.usrs WHERE session = \"@sess\"", conn);
            cmd.Parameters["@sess"].Value = cookie;
            MySqlDataReader red = cmd.ExecuteReader();
            foreach (string user in red)
            {
                return user;
            }
            return null;
        }

        static byte[] GetSessionCookie(string user, MySqlConnection conn)
        {

        }

        static byte[] ResetSessionCookie(string user, MySqlConnection conn)
        {

        }
    }
}
