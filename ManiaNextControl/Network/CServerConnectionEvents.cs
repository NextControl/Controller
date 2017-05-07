using ManiaNextControl.Classes;
using ManiaNextControl.Debug;
using ManiaNextControl.FileManagement;
using ManiaNextControl.Manialink;
using ManiaNextControl.Utils;
using ManiaplanetXMLRPC.Connector;
using ManiaplanetXMLRPC.Connector.Gbx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManiaplanetXMLRPC.Attributes;
using ManiaNextControl.Controller;
using System.Collections;
using System.Reflection;

namespace ManiaNextControl.Network
{
    public class CServerConnectionEvents
    {
        internal CServerConnection con;

        public async Task ChatSendServerMessage(string message)
        {
            await con.Manager.AsyncSendCall(GbxParam.Create("ChatSendServerMessage", message));
        }

        public async Task<GbxParam> ChooseNextMap(string mapFileName)
        {
            return await con.Manager.AsyncSendCall(GbxParam.Create("ChooseNextMap", mapFileName));
        }

        public async Task<GbxParam> ChooseNextMapList(List<string> mapFilesName)
        {
            var gbx = GbxParam.LegacyCreate("ChooseNextMapList", mapFilesName.ToArray());
            return await con.Manager.AsyncSendCall(gbx);
        }

        public async Task Maniaplanet_UI_SetAltScoresTableVisibility(bool state)
        {
            await con.Manager.AsyncSendCall(GbxParam.Create("Maniaplanet.UI.SetAltScoresTableVisibility", state));
        }
    }
}
