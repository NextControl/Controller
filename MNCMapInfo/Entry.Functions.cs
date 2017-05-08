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
            foreach (var map in CManiaNextControl.XmlRPC_Clients[serverLogin].MapList.Values)
            {
                var dico = gc_Dico[i] = new Dictionary<string, string>();
                dico["UId"] = map.UId;
                dico["Name"] = map.Name;

                i++;
            }

            GS_Maps._all = true;
            GS_Maps._wPlayers = players.ToList();
            GS_Maps.Set(gc_Dico);

            GS_RefreshMapList._all = true;
            GS_RefreshMapList._wPlayers = GS_Maps._wPlayers;
            GS_RefreshMapList.Set(true);
        }
    }
}