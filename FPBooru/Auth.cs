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
		static private Random rand = new Random();

		public static string GetUserFromSessionCookie(string cookie, MySqlConnection conn)
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

		public static string AuthenticateUser(string user, byte[] sha256password, MySqlConnection conn)
		{
			MySqlCommand cmd = new MySqlCommand("SELECT password FROM fpbooru.usrs WHERE username = \"@uname\"", conn);
			cmd.Parameters["@uname"].Value = user;
			MySqlDataReader red = cmd.ExecuteReader();
			foreach (string pw in red)
			{
				if (Convert.FromBase64String(pw) == sha256password) {
					string cookie = GetSessionCookie(user, conn);
					if (cookie != null) {
						return cookie;
					} else {
						return ResetSessionCookie(user, conn);
					}
				}
			}
			return null;
		}

		public static string GetSessionCookie(string user, MySqlConnection conn)
		{
			MySqlCommand cmd = new MySqlCommand("SELECT session FROM fpbooru.usrs WHERE username = \"@uname\"", conn);
			cmd.Parameters["@uname"].Value = user;
			MySqlDataReader red = cmd.ExecuteReader();
			foreach (string sess in red)
			{
				return sess;
			}
			return null;
		}

		public static string ResetSessionCookie(string user, MySqlConnection conn)
		{
			byte[] sess = new byte[32];
			rand.NextBytes(sess);
			string cookie = System.Convert.ToBase64String(sess);

			MySqlCommand cmd = new MySqlCommand("UPDATE fpbooru.usrs SET session = \"@sess\" WHERE username = \"@uname\"", conn);
			cmd.Parameters["@uname"].Value = user;
			cmd.Parameters["@sess"].Value = cookie;
			cmd.ExecuteNonQuery();

			return cookie;
		}
	}
}
