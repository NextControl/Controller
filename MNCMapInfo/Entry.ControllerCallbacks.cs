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
using ManiaNextControl.Network;

namespace ManiaNextControl.DefaultPlugins
{
    public partial class MapInfo : CRolePlugin,
        ManiaPlanetCallbacks.PlayerManialinkPageAnswer,
        ManiaPlanetCallbacks.PlayerConnect
    {
        void OnGettingMapInformation(CServerConnection con, CMap map)
        {
            GS_LoadedMaps.SetNowForServers(con.MapList.Where(m => m.Value.LoadingState == HalfClass.CurrentState.AllInfoFilled).Count(), con);
            GS_MapCount.SetNowForServers(con.MapList.Count, con);

            CDebug.Log("Received informations for " + map.Name);
        }
    }
}