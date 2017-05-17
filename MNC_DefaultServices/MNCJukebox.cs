using ManiaNextControl.Classes;
using ManiaNextControl.Controller;
using ManiaNextControl.Debug;
using ManiaNextControl.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManiaNextControl.Services
{
    public class MNCJukebox : 
        CService,
        IServiceServer,
        IServiceStartAutomatically
    {
        public CServerConnection Service_ServerConnection { get; set; }

        public List<CMap> JukedMaps = new List<CMap>();

        public async Task ServerAdded()
        {

        }

        public async Task ServerLoaded()
        {

        }

        public void Reset()
        {
            JukedMaps.Clear();
        }

        public async Task Add(CMap map)
        {
            CDebug.Log("something");
            if (!JukedMaps.Contains(map))
            {
                JukedMaps.Add(map);
                await Service_ServerConnection.Events.ChatSendServerMessage("$69f $fff|| Juked $<" + map.Name + "$z$s$>!$");
                await UpdateList();
            }
            else
            {
                await Service_ServerConnection.Events.ChatSendServerMessage("$888 $fff|| Map already juked.");
            }
        }

        public async Task JukeAsNext(CMap map)
        {

            await Service_ServerConnection.Events.ChatSendServerMessage("$69f $fff|| Juked $<" + map.Name + "$z$s$>!$");
            var gbx = await Service_ServerConnection.Events.ChooseNextMap(map.FileName);
            await Service_ServerConnection.Events.ChatSendServerMessage(gbx.isError ? "$f00 ✘Error" : " $0f0✔Done");
        }

        public async Task Insert(CMap map, int index)
        {
            JukedMaps.Insert(index, map);
            await Service_ServerConnection.Events.ChatSendServerMessage("$69f $fff|| Inserting $<" + map.Name + "$z$s$> at index " + index + " to the jukebox!$");
            await UpdateList();
        }

        private async Task UpdateList()
        {
            var reversedList = JukedMaps.Reverse<CMap>();

            int errorCount = 0;
            var gbx = await Service_ServerConnection.Events.ChooseNextMapList(JukedMaps.Select(m => m.FileName).ToList());

            await Service_ServerConnection.Events.ChatSendServerMessage(gbx.isError
                ? ("$f00 ✘Error\n" + gbx.ErrorString)
                : "$0f0 ✔Done");
        }

        internal async void OnMapLoaded(CMap map)
        {
            if (JukedMaps.Contains(map))
            {
                try
                {
                    if (await CManiaNextControl.ServersConfig[""].GetLiveValue("jukebox_rmp", await CManiaNextControl.GlobalConfig.GetLiveValue("jukebox_rmp", "1"))
                        == "1"
                        && JukedMaps[0] != map)
                        return;
                }
                catch { }
                await Service_ServerConnection.Events.ChatSendServerMessage("$f20 $fff|AUTO| Removing $<" + map.Name + "$z$s$>");
                JukedMaps.Remove(map);
            }
        }
    }
}
