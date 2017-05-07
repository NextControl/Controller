using ManiaNextControl.Classes;
using ManiaNextControl.Config;
using ManiaNextControl.Debug;
using ManiaNextControl.Network;
using ManiaNextControl.Plugin;
using ManiaplanetXMLRPC.Callbacks;
using ManiaplanetXMLRPC.Connector;
using ManiaplanetXMLRPC.Connector.Gbx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ManiaNextControl.Controller
{
    public class CManiaNextControl
    {
        public static CManiaNextControl Singleton;
        public static CControllerEvents Events = new CControllerEvents();

        public static CNextConfig GlobalConfig;
        public static Dictionary<string, CServerConfig> ServersConfig { get; internal set; } = new Dictionary<string, CServerConfig>();

        public static Dictionary<string, CServerConnection> XmlRPC_Clients { get; internal set; } = new Dictionary<string, CServerConnection>();
        public static List<CPlugin> Plugins { get; internal set; } = new List<CPlugin>();

        public static string RunningPath
            => Assembly.GetEntryAssembly().Location
                .Replace(Assembly.GetEntryAssembly().GetName().Name + ".dll", "");

        public static async Task AddServer(CServerConfig serverConfig)
        {
            var client = new CServerConnection();
            XmlRPC_Clients[serverConfig.ServerLogin] = client;
            {
                var connected = await client.CreateConnection(IPAddress.Parse(serverConfig.IPAddress), serverConfig.Port);
                if (connected)
                    CDebug.Log("Connected!");
                else
                {
                    CDebug.ErrorLog("Couldn't connect to : " + serverConfig.IPAddress + ":" + serverConfig.Port);
                }
            }

            for (int i = 0; i < Plugins.Count; i++)
                Plugins[i].OnServerAdded(serverConfig.ServerLogin);
        }

        public static async Task InitServer(string serverLogin)
        {
            var client = XmlRPC_Clients[serverLogin];

            await client.Manager.AsyncSendCall(GbxParam.Create("Authenticate",
                ServersConfig[serverLogin].SuperAdminLogin,
                ServersConfig[serverLogin].SuperAdminPassword));
            await client.Manager.AsyncSendCall(GbxParam.Create("EnableCallbacks", true));
            await client.Manager.AsyncSendCall(GbxParam.Create("SetApiVersion", "2013-04-16"));
            await client.Manager.AsyncSendCall(GbxParam.Create("ChatSendServerMessage", "The controller successfuly made a link to this server!"));
            await client.Refresh();

            for (int i = 0; i < Plugins.Count; i++)
                Plugins[i].OnServerLoaded(client.ServerInfo.Login);

            await client.ApplyFullInformationsToMaps();
        }

        public static async Task LoadPlugins(string xmlText)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xmlText);

            string entryPluginPath = string.Empty;
            foreach (XmlElement element in doc)
            {
                if (element.Name.ToLower() == "entry")
                    entryPluginPath = element.InnerText;
            }

            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(RunningPath + "plugins/" + entryPluginPath);
            var pluginsType = assembly.GetTypes().Where(t => typeof(CPlugin).IsAssignableFrom(t));
            foreach (var pluginType in pluginsType)
            {
                var pl = Activator.CreateInstance(pluginType) as CPlugin;
                pl.Init();

                Plugins.Add(pl);
            }
        }
    }
}
