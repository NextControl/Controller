using ManiaNextControl.Config;
using ManiaNextControl.Controller;
using ManiaNextControl.Debug;
using ManiaNextControl.Manialink;
using ManiaNextControl.Network;
using ManiaplanetXMLRPC.Connector;
using ManiaplanetXMLRPC.Connector.Gbx;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

class CProgramMNC
{
    static void Main(string[] args)
    {
        LoadEverything().Wait();

        CManiaNextControl.Singleton = new CManiaNextControl();

        foreach (var serverConf in CManiaNextControl.ServersConfig)
        {
            CManiaNextControl.AddServer(serverConf.Value).Wait();
            CManiaNextControl.InitServer(serverConf.Value.ServerLogin).Wait();
        }

        bool createdNew;
        var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, (Guid.NewGuid().ToString() + Guid.NewGuid().ToString()), out createdNew);
        var signaled = false;
        var currentHandle = 0;
        int loopHandle = 0;

        do
        {
            if (loopHandle >= 10)
            {
                loopHandle = 0;
                CManialink.Loop();
            }

            loopHandle++;

            signaled = waitHandle.WaitOne(TimeSpan.FromSeconds(1 / 100f));

            //PluginManager.Plugins.ForEach(o => o.Update());
        }
        while (!signaled);

        while (true)
        {
            
        }
    }

    static async Task LoadEverything()
    {
        ForceCulture();
        await ReloadAllConfigs();
        LoadAllPlugins();
    }

    static void ForceCulture()
    {
        string CultureName = CultureInfo.CurrentCulture.Name;
        CultureInfo ci = new CultureInfo(CultureName);
        if (ci.NumberFormat.NumberDecimalSeparator != ".")
        {
            // Forcing use of decimal separator for numerical values
            ci.NumberFormat.NumberDecimalSeparator = ".";
            CultureInfo.CurrentCulture = ci;
        }
    }

    static async Task ReloadAllConfigs()
    {
        var servers_confFolder = CManiaNextControl.RunningPath + "configs";
        var global_confFile = CManiaNextControl.RunningPath + "nextcontrol.conf";

        // Load global conf first
        {

        }

        CManiaNextControl.ServersConfig = new Dictionary<string, CServerConfig>();
        {
            CDebug.Log(servers_confFolder);

            if (!Directory.Exists(servers_confFolder))
                Directory.CreateDirectory(servers_confFolder);

            foreach (var file in Directory.GetFiles(servers_confFolder))
            {
                if (file.EndsWith(".conf"))
                {
                    CDebug.Log(file.LastIndexOf('.'));

                    string serverLogin = file.Substring(file.LastIndexOf("\\"), file.Length - file.LastIndexOf('.'));
                    var config = new CServerConfig(file);

                    if ((await config.GetLiveValue("Enabled", "false")).ToLower() == "false"
                        || await config.GetLiveValue("Enabled", "false") == "0")
                    {
                        continue;
                    }

                    config.ServerLogin = serverLogin;
                    config.SuperAdminLogin = await config.GetLiveValue("SuperAdminLogin", "SuperAdmin");
                    config.SuperAdminPassword = await config.GetLiveValue("SuperAdminPassword", "SuperAdmin");
                    config.Port = await config.GetLiveValue("Port", "") == "" ? CNetUtils.TryGetPort(serverLogin) : int.Parse(await config.GetLiveValue("Port"));

                    CManiaNextControl.ServersConfig[serverLogin] = config;
                }
            }
        }
    }

    static async void LoadAllPlugins()
    {
        string plugin_folder = CManiaNextControl.RunningPath + "plugins";

        if (!Directory.Exists(plugin_folder))
            Directory.CreateDirectory(plugin_folder);

        foreach (var file in Directory.GetFiles(plugin_folder))
        {
            if (file.EndsWith(".mncplugin"))
            {
                await CManiaNextControl.LoadPlugins(File.ReadAllText(file));
            }
        }
    }
}