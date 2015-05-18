using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using PluginInterface;

namespace FPBooru {
	public class PluginManager : IPluginManager {
		private MySqlConnection mysqlconn;
		internal List<IPlugin> loadedplugins = new List<IPlugin>();
		internal bool hasAuthPlugin = false;

		public PluginManager(MySqlConnection conn) {
			mysqlconn = conn;
		}

		internal void AddPlugin(IPlugin plugin) {
			if (!hasAuthPlugin)
				hasAuthPlugin = ((plugin.GetInformation().Permissions & Permission.HandleAuth) == Permission.HandleAuth);
		}

		#region IPluginManager implementation
		public string GetUserAppData(string UserName, IPlugin plugin) {
			throw new NotImplementedException();
		}

		public bool SetUserAppData(string UserName, IPlugin plugin, string AppData) {
			throw new NotImplementedException();
		}

		public string GetAppData(IPlugin plugin) {
			throw new NotImplementedException();
		}

		public bool SetAppData(IPlugin plugin, string AppData) {
			throw new NotImplementedException();
		}
		#endregion
    }
}

