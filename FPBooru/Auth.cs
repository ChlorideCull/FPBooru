using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Net;
using PluginInterface;
using Nancy.Helpers;

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
				return false;
			} else {
				MySqlCommand cmd = new MySqlCommand("INSERT INTO fpbooru.usrs (username, password, session) VALUES (@usrnme, @basepassword, @sess)", conn);
				cmd.Parameters.Clear();
				cmd.Parameters.AddWithValue("@usrnme", user);
				cmd.Parameters.AddWithValue("@basepassword", Convert.ToBase64String((new SHA256Managed()).ComputeHash(System.Text.Encoding.UTF8.GetBytes(password))));
				cmd.Parameters.AddWithValue("@sess", rand.Next().ToString());
				try {
					int result = cmd.ExecuteNonQuery();
					if (result > 0)
						ResetSessionCookie(user, conn);
					else
						return false;
					return true;
				} catch (MySqlException) {
					return false;
				}
			}
		}

		public static string GetUserFromSessionCookie(PluginManager pm, string cookie, MySqlConnection conn)
		{
			if (pm.hasAuthPlugin) {
				foreach (IPlugin plugin in pm.loadedplugins) {
					if ((plugin.GetInformation().Permissions & Permission.HandleAuth) == Permission.HandleAuth)
						return plugin.GetAuthenticatedUser(cookie);
				}
				return null;
			} else {
				MySqlCommand cmd = new MySqlCommand("SELECT username FROM fpbooru.usrs WHERE session = @sess", conn);
				cmd.Parameters.Clear();
				cmd.Parameters.AddWithValue("@sess", HttpUtility.UrlDecode(cookie));
				using (MySqlDataReader red = cmd.ExecuteReader()) {
					while (red.Read()) {
						return red.GetString(red.GetOrdinal("username"));
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
				return null;
			} else {
				Console.WriteLine("no auth plugin");
				byte[] sha256password = (new SHA256Managed()).ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
				MySqlCommand cmd = new MySqlCommand("SELECT password FROM fpbooru.usrs WHERE username = @uname", conn);
				cmd.Parameters.Clear();
				cmd.Parameters.AddWithValue("@uname", user);
				using (MySqlDataReader red = cmd.ExecuteReader()) {
					while (red.Read()) {
						string pw = red.GetString(red.GetOrdinal("password"));
						Console.WriteLine(pw + " == " + Convert.ToBase64String(sha256password));
						if (pw == Convert.ToBase64String(sha256password)) {
							red.Close();
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
				while (red.Read()) {
					return red.GetString(red.GetOrdinal("session"));
				}
				return null;
			}
		}

		public static string ResetSessionCookie(string user, MySqlConnection conn)
		{
			byte[] sess = new byte[32];
			rand.NextBytes(sess);
			string cookie = System.Convert.ToBase64String(sess);

			MySqlCommand cmd = new MySqlCommand("UPDATE fpbooru.usrs SET session = @sess WHERE username = @uname", conn);
			cmd.Parameters.Clear();
			cmd.Parameters.AddWithValue("@uname", user);
			cmd.Parameters.AddWithValue("@sess", cookie);
			cmd.ExecuteNonQuery();

			return cookie;
		}
	}
}
