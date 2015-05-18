using System;

namespace PluginInterface {
	// MAIN PLUGIN INTERFACE
	public interface IPlugin {
		/// <summary>
		/// Sets the governing plugin manager, which handles permissions and data management.
		/// </summary>
		/// <returns><c>true</c>, if the governing plugin manager was set, <c>false</c> otherwise.</returns>
		/// <param name="PlugMan">Governing Plugin Manager</param>
		bool SetPluginManager(IPluginManager PlugMan);

		/// <summary>
		/// Gets the plugin information, specifing permissions, license, name and more.
		/// </summary>
		/// <returns>This plugins information.</returns>
		PluginInfo GetInformation();

		#region HandleAuth
		/// <summary>
		/// Attempt to Authenticate an User.
		/// </summary>
		/// <param name="UserName">Provided username</param>
		/// <param name="Password">Provided password</param>
		/// <returns>Cookie that can be used to automatically authenticate an user, or <c>null</c> if <paramref name="UserName"/>
		/// or <param name="Password"/> is incorrect.
		/// </returns>
		/// <remarks>Will never get called unless the Permission.HandleAuth is <c>or</c>d into the Permissions in the plugin info</remarks>
		string Authenticate(string UserName, string Password);

		/// <summary>
		/// Get the username of an already authenticated user from the cookie.
		/// </summary>
		/// <param name="cookie">Cookie, as provided by Authenticate</param>
		/// <returns>Username of an user, or <c>null</c> if <paramref name="cookie"/> is incorrect.</returns>
		/// <remarks>Will never get called unless the Permission.HandleAuth is <c>or</c>d into the Permissions in the plugin info</remarks>
		string GetAuthenticatedUser(string cookie);
		#endregion

		#region CustomPages
		/// <summary>
		/// Gets the page titles provided by this Plugin
		/// </summary>
		/// <returns>The page titles.</returns>
		/// <remarks>Will never get called unless the Permission.CustomPages is <c>or</c>d into the Permissions in the plugin info</remarks>
		string[] GetPageTitles();

		/// <summary>
		/// Loads the titled page provided by this Plugin
		/// </summary>
		/// <returns>Page HTML, inserted into a div</returns>
		/// <param name="title">Title of requested page.</param>
		/// <remarks>Will never get called unless the Permission.CustomPages is <c>or</c>d into the Permissions in the plugin info</remarks>
		string LoadPage(string title);
		#endregion

		#region UserPins
		/// <summary>
		/// Loads the user pins provided by this plugin for a certain user
		/// </summary>
		/// <returns>User Pins to be shown on the profile</returns>
		/// <param name="UserName">Requesting username.</param>
		/// <remarks>Will never get called unless the Permission.UserPins is <c>or</c>d into the Permissions in the plugin info</remarks>
		UserPin[] LoadUserPins(string UserName);
		#endregion
	}

	// DATA DEFINITIONS
	public interface IPluginManager {
		string GetUserAppData(string UserName, IPlugin plugin);
		bool SetUserAppData(string UserName, IPlugin plugin, string AppData);
		string GetAppData(IPlugin plugin);
		bool SetAppData(IPlugin plugin, string AppData);
	}

	public struct UserPin {
		public string ImageURL;
		public string Title;
		public string FlavorText;
		public string Description;
	}

	public struct PluginInfo {
		public string Name;
		public string VersionString;
		public string URL;
		public LicenseInfo License;
		public Int32 Permissions;
	}

	public enum LicenseInfo {
		MIT,
		AGPL
	}

	public enum Permission {
		HandleAuth = 1,
		UserPins = 1 << 1,
		CustomPages = 1 << 2
	}
}

