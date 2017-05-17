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

                        GS_MapInfo.SetNowForPlayers(gc_Dico, await CPlayer.GetPlayerFromLogin(Login, new[] { server }));

                        gc_Dico.Clear();
                    }
                    else
                    {
                        GS_MapInfo.SetNowForPlayers(mapinfoError, await CPlayer.GetPlayerFromLogin(Login, new[] { server }));
                    }
                }
                if (splitMessage[1] == "juke")
                {
                    string mapUId = splitMessage[2];

                    if (server.MapList.ContainsKey(mapUId))
                        await server.GetService<MNCJukebox>().Add(server.MapList[mapUId]);
                }
            }
        }

        async void ManiaPlanetCallbacks.PlayerConnect.Callback(Client con, string Login, bool isSpectator)
        {
            var server = con as CServerConnection;
            var player = await CPlayer.GetPlayerFromLogin(Login, new[] { server }, true);
            ApplyManialinksToPlayer(server.ServerInfo.Login, player);
        }

        async void ManiaPlanetCallbacks.PlayerChat.Callback(Client con, int PlayerUid, string Login, string Text, bool IsRegistredCmd)
        {
            if (Text.StartsWith("/addmap "))
            {
                var url = Text.Replace("/addmap ", string.Empty);
                try
                {
                    await COnlineMapBrowser.AddMapFromUrl((CServerConnection)con, url);
                }
                catch(Exception ex)
                {
                    CDebug.ErrorLog(ex.Message, con as CServerConnection);
                }
            }
        }
    }
}