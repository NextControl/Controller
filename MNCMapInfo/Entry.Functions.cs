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
        ManiaPlanetCallbacks.PlayerConnect
    {
        public async void ApplyManialinksToPlayer(string serverLogin, params CPlayer[] players)
        {
            var server = CManiaNextControl.XmlRPC_Clients[serverLogin];

            foreach (var player in players)
            {
                await Task.Factory.StartNew(() =>
                {
                    while (player.LoadingState != HalfClass.CurrentState.AllInfoFilled
                        || !player.ControllerLoaded);
                });
            }

            Manialink_MapList.Send(new[] { CManiaNextControl.XmlRPC_Clients[serverLogin].Manager }, 0, players);
            Manialink_MapWidget.Send(new[] { CManiaNextControl.XmlRPC_Clients[serverLogin].Manager }, 0, players);
            Manialink_MapSideBarTools.Send(new[] { CManiaNextControl.XmlRPC_Clients[serverLogin].Manager }, 0, players);
            Manialink_MapInfo.Send(new[] { CManiaNextControl.XmlRPC_Clients[serverLogin].Manager }, 0, players);
            Manialink_ManiaExchangeList.Send(new[] { CManiaNextControl.XmlRPC_Clients[serverLogin].Manager }, 0, players);

            var gc_Dico = new Dictionary<int, Dictionary<string, string>>();

            int i = 0;
            foreach (var map in server.MapList.Values)
            {
                var dico = gc_Dico[i] = new Dictionary<string, string>();
                dico["UId"] = map.UId;
                dico["Name"] = map.Name;

                i++;
            }

            GS_MapCount.SetNowForPlayers(server.MapList.Where(m => m.Value.LoadingState == HalfClass.CurrentState.AllInfoFilled).Count(), players);
            GS_Maps.SetNowForPlayers(gc_Dico, players);
            GS_RefreshMapList.SetNowForPlayers(true, players);
        }
    }
}