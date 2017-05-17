using ManiaNextControl.Classes;
using ManiaNextControl.Services;
using ManiaplanetXMLRPC.Attributes;
using ManiaplanetXMLRPC.Connector;
using ManiaplanetXMLRPC.Connector.Gbx;
using ManiaplanetXMLRPC.Structure;
using System;
using System.Collections.Generic;
using System.Text;
using static ManiaplanetXMLRPC.Callbacks.ManiaPlanetCallbacks;

namespace ManiaNextControl.Network
{
    public partial class CServerConnection : Client
    {
        public async void ProcessCallback(CallbackContext cc)
        {
            var gbx = cc.gbx;
            var op = gbx.Parameters;

            switch (gbx.EventName)
            {
                case Code_PlayerConnect:
                    {
                        var player = await CPlayer.GetPlayerFromLogin((string)op[0], new[] { this }, false);
                        player.LoadingState = HalfClass.CurrentState.PrimaryInfoFilled;

                        CPlayer.AllPlayers[player.User.Login] = player;

                        // Get detailed info
                        var call = await Manager.AsyncSendCall(GbxParam.Create("GetDetailedPlayerInfo", player.User.Login));

                        var dicoDetail = (Dictionary<string, object>)call.Parameters[0];
                        player.User = new CUser();
                        player.User.IPAddress = (string)dicoDetail["IPAddress"];
                        player.User.DownloadRate = (int)dicoDetail["DownloadRate"];
                        player.User.UploadRate = (int)dicoDetail["UploadRate"];
                        player.User.Language = (string)dicoDetail["Language"];

                        player.User.Player = player;

                        player.LoadingState = HalfClass.CurrentState.AllInfoFilled;

                        await ApplyInterfaceHelpers(player);
                        player.LoadingState = HalfClass.CurrentState.AllInfoFilled;
                        player.ControllerLoaded = true;

                        await Manager.AsyncSendCall(GbxParam.Create("ChatSendServerMessage", $"$0f0$555⏵ $fffWelcome $<{player.NickName}$z$s$> $fffto the server!"));

                        TriggerListeners<PlayerConnect>(this, o => o.Callback(this, (string)op[0], (bool)op[1]));
                        break;
                    }

                case Code_PlayerDisconnect:
                    {
                        CPlayer.AllPlayers.TryGetValue((string)op[0], out var player);
                        await Manager.AsyncSendCall(GbxParam.Create("ChatSendServerMessage", $"$0f0$555⏵ $fffBye bye $<{(player == null || player.NickName == null ? (string)op[0] : player.NickName)}$z$s$> $fff:("));

                        TriggerListeners<PlayerDisconnect>(this, o => o.Callback(this, (string)op[0], (string)op[1]));
                        break;
                    }
                case Code_PlayerChat:
                    if (op.Length == 4)
                        TriggerListeners<PlayerChat>(this, o => o.Callback(this, (int)op[0], (string)op[1], (string)op[2], (bool)op[3]));
                    break;

                case Code_BeginMap:
                    var smap = SMapInfo.Convert((Dictionary<string, object>)op[0]);
                    await RefreshMapCompletely(smap.UId, smap.FileName);

                    if (op.Length == 1)
                        TriggerListeners<BeginMap>(this, o => o.Callback(this, smap));
                    break;

                case Code_EndMap:
                    if (op.Length == 1)
                        TriggerListeners<EndMap>(this, o => o.Callback(this, SMapInfo.Convert((Dictionary<string, object>)op[0])));
                    break;

                case Code_MapListModified:
                    TriggerListeners<MapListModified>(this, o => o.Callback(this, 0, 0, true));
                    break;
                case Code_PlayerManialinkPageAnswer:
                    TriggerListeners<PlayerManialinkPageAnswer>(this, o => o.Callback(this, (int)op[0], (string)op[1], (string)op[2]));
                    break;
            }
        }
    }
}
