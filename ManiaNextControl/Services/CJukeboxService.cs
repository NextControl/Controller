using ManiaNextControl.Classes;
using ManiaNextControl.Controller;
using ManiaNextControl.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManiaNextControl.Services
{
    public class CJukeboxService : CService
    {
        public static List<CMap> JukedMaps = new List<CMap>();


        public static void Reset()
        {
            JukedMaps.Clear();
        }

        public static async Task Add(CServerConnection con, CMap map)
        {
            if (!JukedMaps.Contains(map))
            {
                JukedMaps.Add(map);
                await con.Events.ChatSendServerMessage("$69f $fff|| Juked $<" + map.Name + "$z$s$>!$");
                await UpdateList(con);
            }
            else
            {
                await con.Events.ChatSendServerMessage("$69f $fff|| Map already juked.");
            }
        }

        public static async Task JukeAsNext(CServerConnection con, CMap map)
        {

            await con.Events.ChatSendServerMessage("$69f $fff|| Juked $<" + map.Name + "$z$s$>!$");
            var gbx = await con.Events.ChooseNextMap(map.FileName);
            await con.Events.ChatSendServerMessage(gbx.isError ? "$f00 ✘Error" : " $0f0✔Done");
        }

        public static async Task Insert(CServerConnection con, CMap map, int index)
        {
            JukedMaps.Insert(index, map);
            await con.Events.ChatSendServerMessage("$69f $fff|| Inserting $<" + map.Name + "$z$s$> at index " + index + " to the jukebox!$");
            await UpdateList(con);
        }

        private static async Task UpdateList(CServerConnection con)
        {
            var reversedList = JukedMaps.Reverse<CMap>();

            int errorCount = 0;
            var gbx = await con.Events.ChooseNextMapList(JukedMaps.Select(m => m.FileName).ToList());

            await con.Events.ChatSendServerMessage(gbx.isError
                ? ("$f00 ✘Error\n" + gbx.ErrorString)
                : "$0f0 ✔Done");
        }

        internal async void OnMapLoaded(string serverLogin, CMap map)
        {
            if (JukedMaps.Contains(map))
            {
                if (await CManiaNextControl.ServersConfig[serverLogin].GetLiveValue("jukebox_rmp", await CManiaNextControl.GlobalConfig.GetLiveValue("jukebox_rmp", "1"))
                    == "1"
                    && JukedMaps[0] != map)
                    return;
                JukedMaps.Remove(map);
            }
        }
    }
}
