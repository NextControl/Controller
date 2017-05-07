using ManiaNextControl.Classes;
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

                        await ApplyInterfaceHelpers(player);
                        player.LoadingState = HalfClass.CurrentState.AllInfoFilled;
                        player.ControllerLoaded = true;

                        await Manager.AsyncSendCall(GbxParam.Create("ChatSend", $"hello {player.User.Login}!"));

                        TriggerListeners<PlayerConnect>(this, o => o.Callback(this, (string)op[0], (bool)op[1]));
                        break;
                    }

                case Code_PlayerDisconnect:
                    TriggerListeners<PlayerDisconnect>(this, o => o.Callback(this, (string)op[0], (string)op[1]));
                    break;

                case Code_PlayerChat:
                    if (op.Length == 4)
                        TriggerListeners<PlayerChat>(this, o => o.Callback(this, (int)op[0], (string)op[1], (string)op[2], (bool)op[3]));
                    break;

                case Code_BeginMap:
                    if (op.Length == 1)
                        TriggerListeners<BeginMap>(this, o => o.Callback(this, SMapInfo.Convert((Dictionary<string, object>)op[0])));
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
