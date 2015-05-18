using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Net;
using PluginInterface;

namespace FPBooru
{
	static class Auth
	{
		static private Random rand = new Random();
		public static bool RegisterUser(PluginManager pm, string user, string password, MySqlConnection conn) {
			if (pm.hasAuthPlugin) {
				foreach (IPlugin plugin in pm.loadedplugins) {
					if ((plugin.GetInformation().Permissions & Permission.HandleAuth) == Permission.HandleAuth)
						return plugin.CreateAccount(user, password);
				}
			} else {
				MySqlCommand cmd = new MySqlCommand("INSERT INTO fpbooru.usrs (username, password, session) VALUES (@usrnme, @basepassword, @sess)", conn);
				cmd.Parameters.Clear();
				cmd.Parameters.AddWithValue("@usrnme", user);
				cmd.Parameters.AddWithValue("@basepassword", Convert.ToBase64String((new SHA256Managed()).ComputeHash(System.Text.Encoding.UTF8.GetBytes(password))));
				cmd.Parameters.AddWithValue("@sess", rand.Next().ToString());
				int result = cmd.ExecuteNonQuery();
				if (result > 0)
					ResetSessionCookie(user, conn);
				else
					return false;
				return true;
			}
		}

		public static string GetUserFromSessionCookie(PluginManager pm, string cookie, MySqlConnection conn)
		{
			if (pm.hasAuthPlugin) {
				foreach (IPlugin plugin in pm.loadedplugins) {
					if ((plugin.GetInformation().Permissions & Permission.HandleAuth) == Permission.HandleAuth)
						return plugin.GetAuthenticatedUser(cookie);
				}
			} else {
				MySqlCommand cmd = new MySqlCommand("SELECT username FROM fpbooru.usrs WHERE session = \"@sess\"", conn);
				cmd.Parameters.Clear();
				cmd.Parameters.AddWithValue("@sess", cookie);
				using (MySqlDataReader red = cmd.ExecuteReader()) {
					foreach (string user in red) {
						return user;
					}
					return null;
				}
			}
		}

		public static string AuthenticateUser(PluginManager pm, string user, string password, MySqlConnection conn)
		{
			if (pm.hasAuthPlugin) {
				foreach (IPlugin plugin in pm.loadedplugins) {
					if ((plugin.GetInformation().Permissions & Permission.HandleAuth) == Permission.HandleAuth)
						return plugin.Authenticate(user, password);
				}
			} else {
				byte[] sha256password = (new SHA256Managed()).ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
				MySqlCommand cmd = new MySqlCommand("SELECT password FROM fpbooru.usrs WHERE username = \"@uname\"", conn);
				cmd.Parameters.Clear();
				cmd.Parameters.AddWithValue("@uname", user);
				using (MySqlDataReader red = cmd.ExecuteReader()) {
					foreach (string pw in red) {
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
			}
		}

		public static string GetSessionCookie(string user, MySqlConnection conn)
		{
			MySqlCommand cmd = new MySqlCommand("SELECT session FROM fpbooru.usrs WHERE username = \"@uname\"", conn);
			cmd.Parameters.Clear();
			cmd.Parameters.AddWithValue("@uname", user);
			using (MySqlDataReader red = cmd.ExecuteReader()) {
				foreach (string sess in red) {
					return sess;
				}
				return null;
				}
		}

		public static string ResetSessionCookie(string user, MySqlConnection conn)
		{
			byte[] sess = new byte[32];
			rand.NextBytes(sess);
			string cookie = System.Convert.ToBase64String(sess);

			MySqlCommand cmd = new MySqlCommand("UPDATE fpbooru.usrs SET session = \"@sess\" WHERE username = \"@uname\"", conn);
			cmd.Parameters.Clear();
			cmd.Parameters.AddWithValue("@uname", user);
			cmd.Parameters.AddWithValue("@sess", cookie);
			cmd.ExecuteNonQuery();

			return cookie;
		}
	}
}
