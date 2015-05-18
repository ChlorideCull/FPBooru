using System;

namespace PluginInterface {
	// MAIN PLUGIN INTERFACE
	public interface IPlugin {
		bool SetPluginManager(IPluginManager PlugMan);
		PluginInfo GetInformation();

		string LoadPage(string title);
		UserPin[] LoadUserPins(string UserName);
	}

	// DATA DEFINITIONS
	public interface IPluginManager {
		void RegisterPage(string title, IPlugin plugin);
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
		SetUserPins = 1 << 1
	}
}

