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

		public static string AuthenticateUser(string user, byte[] sha256password)
		{
			throw NotImplementedException();
			return GetSessionCookie(user);
		}

		public static string GetSessionCookie(string user, MySqlConnection conn)
		{
			return "";
		}

		public static string ResetSessionCookie(string user, MySqlConnection conn)
		{
			return "";
		}
	}
}
