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
using ManiaNextControl.Services;

namespace ManiaNextControl.DefaultPlugins
{
    public partial class MapInfo : CRolePlugin,
        ManiaPlanetCallbacks.PlayerManialinkPageAnswer,
        ManiaPlanetCallbacks.PlayerConnect
    {
        async void ManiaPlanetCallbacks.PlayerManialinkPageAnswer.Callback(Client con, int PlayerUid, string Login, string Answer)
        {

            var server = con as Network.CServerConnection;
            var splitMessage = Answer.Split('|');

            CDebug.Log(splitMessage.Length + " : " + splitMessage[0] + ", " + splitMessage[1] + ", " + splitMessage[2]);
            if (splitMessage.Length == 3
                && splitMessage[0] == "mnc_mapinfo")
            {
                if (splitMessage[1] == "selectmapuid")
                {
                    string mapUid = splitMessage[2];

                    if (server.MapList.ContainsKey(mapUid))
                    {
                        var map = server.MapList[mapUid];
                        var gc_Dico = map.GetStringHashtable();

                        GS_MapInfo._wPlayers = new List<CPlayer>() { await CPlayer.GetPlayerFromLogin(Login, new[] { server }) };
                        GS_MapInfo.Set(gc_Dico);
                    }
                    else
                    {
                        GS_MapInfo.Set(mapinfoError);
                    }
                }
                if (splitMessage[1] == "juke")
                {
                    string mapUId = splitMessage[2];

                    if (server.MapList.ContainsKey(mapUId))
                        await CJukeboxService.Add(server, server.MapList[mapUId]);
                }
            }
        }

        async void ManiaPlanetCallbacks.PlayerConnect.Callback(Client con, string Login, bool isSpectator)
        {
            var server = con as CServerConnection;
            var player = await CPlayer.GetPlayerFromLogin(Login, new[] { server }, true);
            ApplyManialinksToPlayer(server.ServerInfo.Login, player);
        }
    }
}