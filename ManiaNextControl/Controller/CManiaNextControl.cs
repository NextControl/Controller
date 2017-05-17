using ManiaNextControl.Classes;
using ManiaNextControl.Config;
using ManiaNextControl.Debug;
using ManiaNextControl.Network;
using ManiaNextControl.Plugin;
using ManiaNextControl.Services;
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
        public static Dictionary<string, CService> GlobalServices { get; internal set; } = new Dictionary<string, CService>();

        internal static List<Type> servicesTypeFound = new List<Type>();
        internal static Dictionary<Type, CXmlPluginInformation> servicesInformation = new Dictionary<Type, CXmlPluginInformation>();

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

            for (int i = 0; i < servicesTypeFound.Count; i++)
            {
                var value = servicesTypeFound[i];
                if (value.GetInterfaces().Contains(typeof(IServiceServer))
                    && value.GetInterfaces().Contains(typeof(IServiceStartAutomatically)))
                {
                    var s = client.ServerServices[value] = Activator.CreateInstance(value) as CService;
                    ((IServiceServer)s).Service_ServerConnection = client;
                    await ((IServiceServer)s).ServerAdded();
                }
            }
            for (int i = 0; i < Plugins.Count; i++)
                await Plugins[i].OnServerAdded(serverConfig.ServerLogin);
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

            client.IsLoaded = true;

            foreach (var service in client.ServerServices)
            {
                await ((IServiceServer)service.Value).ServerLoaded();
            }
            for (int i = 0; i < Plugins.Count; i++)
                await Plugins[i].OnServerLoaded(client.ServerInfo.Login);

            await client.ApplyFullInformationsToMaps();
        }

        public static async Task LoadPlugins(string xmlText)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xmlText);

            var pluginsInformation = new List<CXmlPluginInformation>();
            foreach (XmlElement entryList in doc.GetElementsByTagName("*"))
            {
                if (entryList.Name.ToLower() == "plugin"
                    || entryList.Name.ToLower() == "service")
                {
                    CXmlPluginInformation conf = new CXmlPluginInformation();
                    foreach (XmlElement element in entryList)
                    {
                        if (element.Name.ToLower() == "entry")
                            conf.entryPath = element.InnerText;
                        if (element.Name.ToLower() == "serviceclassname")
                            conf.className = element.InnerText;
                        if (element.Name.ToLower() == "isservice"
                            && (element.InnerText.ToLower() == "true"
                            || element.InnerText.ToLower() == "1"))
                            conf.isService = true;
                        if (element.Name.ToLower() == "servicetype")
                            conf.serviceType = element.InnerText;
                    }

                    pluginsInformation.Add(conf);
                }
            }

            var alreadyLoadedAssemblies = new Dictionary<string, Assembly>();
            foreach (var pluginInfo in pluginsInformation)
            {
                if (!alreadyLoadedAssemblies.TryGetValue($"{(pluginInfo.isService ? "s" : "p")}" + pluginInfo.entryPath, out var assembly))
                    assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(RunningPath + $"{(pluginInfo.isService ? "services" : "plugins")}/" + pluginInfo.entryPath);
                alreadyLoadedAssemblies[$"{(pluginInfo.isService ? "s" : "p")}" + pluginInfo.entryPath] = assembly;

                if (!pluginInfo.isService)
                {
                    var pluginTypes = assembly.GetTypes().Where(t => typeof(CPlugin).IsAssignableFrom(t));
                    foreach (var pluginType in pluginTypes)
                    {
                        var pl = Activator.CreateInstance(pluginType) as CPlugin;
                        await pl.Init();

                        Plugins.Add(pl);
                    }
                }
                else
                {
                    if (GlobalServices.ContainsKey(pluginInfo.serviceType))
                    {
                        throw new DoubleServiceTypeException();
                    }

                    var serviceTypes = assembly.GetTypes().Where(t => typeof(CService).IsAssignableFrom(t) && t.Name == pluginInfo.className);
                    foreach (var serviceType in serviceTypes)
                    {
                        if (serviceType.GetInterfaces().Contains(typeof(IServiceStartAutomatically)))
                        {
                            if (serviceType.GetInterfaces().Contains(typeof(IServiceGlobalSingleton)))
                            {
                                var s = Activator.CreateInstance(serviceType) as IServiceGlobalSingleton;
                                await s.ControllerLoad();

                                GlobalServices[pluginInfo.serviceType] = s as CService;
                            }
                        }

                        servicesTypeFound.Add(serviceType);
                        servicesInformation[serviceType] = pluginInfo;
                    }
                }
            }
        }
    }
}
