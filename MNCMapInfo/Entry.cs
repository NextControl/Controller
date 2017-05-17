using ManiaNextControl.Classes;
using ManiaNextControl.Controller;
using ManiaNextControl.Debug;
using ManiaNextControl.Manialink;
using ManiaNextControl.Plugin;
using ManiaplanetXMLRPC.Callbacks;
using System;
using System.Collections.Generic;
using System.Linq;
using static ManiaNextControl.Manialink.CManialink;
using ManiaplanetXMLRPC.Connector;
using System.Threading.Tasks;

namespace ManiaNextControl.DefaultPlugins
{
    public partial class MapInfo : CRolePlugin,
        ManiaPlanetCallbacks.PlayerManialinkPageAnswer,
        ManiaPlanetCallbacks.PlayerConnect,
        ManiaPlanetCallbacks.PlayerChat
    {
        CManialink Manialink_MapInfo;
        CManialink Manialink_MapList;
        CManialink Manialink_MapWidget;
        CManialink Manialink_MapSideBarTools;
        CManialink Manialink_ManiaExchangeList;

        /// <summary>
        /// Return all maps with some of their informations
        /// <para>Usage: Share with all players</para>
        /// </summary>
        public SharerAction<Dictionary<int, Dictionary<string, string>>> GS_Maps;
        /// <summary>
        /// Return all informations of a map
        /// <para>Usage: Share with one player</para>
        /// </summary>
        public SharerAction<Dictionary<string, string>> GS_MapInfo;
        public SharerAction<bool> GS_RefreshMapList;
        public SharerAction<int> GS_LoadedMaps;
        public SharerAction<int> GS_MapCount;

        public Dictionary<string, string> mapinfoError { get; private set; }

        public override async Task Init()
        {
            await base.Init();

            mapinfoError = new Dictionary<string, string>()
            {
                { "UId", "error" }
            };

            Manialink_MapList = CManialink.Build(await FileIO.ReadTextAsync("maplist.xml"), this)
                .SetID("maplist")
                .SetName("MapList")
                .SetVersion(3);
            string frameInstance = "";
            for (int i = 0; i < 50; i++)
            {
                frameInstance += $"<frameinstance hidden='1' modelid='view' id='view_{i}' pos='0 {50 - (i * 10)}' z-index='1' />";
            }
            Manialink_MapList.SetParameter("FrameInstances", frameInstance);

            GC.Collect(); //< beaucoup de strings là

            Manialink_MapWidget = CManialink.Build(await FileIO.ReadTextAsync("mapwidget.xml"), this)
                .SetID("mapwidget")
                .SetName("MapWidget")
                .SetVersion(3);
            Manialink_MapSideBarTools = CManialink.Build(await FileIO.ReadTextAsync("mapsidebartools.xml"), this)
                .SetID("mapsidebartools")
                .SetName("MapSideBar tools")
                .SetVersion(3);
            Manialink_MapInfo = CManialink.Build(await FileIO.ReadTextAsync("mapinfo.xml"), this)
                .SetID("mapinfo")
                .SetName("MapInfo")
                .SetVersion(3);
            Manialink_ManiaExchangeList = CManialink.Build(await FileIO.ReadTextAsync("mapmxlist.xml"), this)
                .SetID("mxlist")
                .SetName("ManiaExchange L")
                .SetVersion(3);
        }

        public override Task OnServerAdded(string serverLogin)
        {
            CDebug.Log(serverLogin + " added!");

            CManiaNextControl.XmlRPC_Clients[serverLogin].RegisterListener<ManiaPlanetCallbacks.PlayerManialinkPageAnswer>(this);
            CManiaNextControl.XmlRPC_Clients[serverLogin].RegisterListener<ManiaPlanetCallbacks.PlayerChat>(this);
            CManiaNextControl.Events.OnGettingMapInformation += OnGettingMapInformation;

            return Task.CompletedTask;
        }

        public override async Task OnServerLoaded(string serverLogin)
        {
            CDebug.Log(serverLogin + " was loaded!");

            ApplyManialinksToPlayer(serverLogin);

            var server = CManiaNextControl.XmlRPC_Clients[serverLogin];
            await server.Events.Maniaplanet_UI_SetAltScoresTableVisibility(false);
        }
    }
}