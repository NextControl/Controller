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
            /*GS_LoadedMaps._server = con;
            GS_MapCount._server = con;*/
            GS_MapCount._wPlayers = GS_LoadedMaps._wPlayers = con.Players.Values.ToList();

            GS_LoadedMaps.SetNow(con.MapList.Where(m => m.Value.LoadingState == HalfClass.CurrentState.AllInfoFilled).Count());
            GS_MapCount.SetNow(con.MapList.Count);

            CDebug.Log("Received informations for " + map.Name);
        }
    }
}