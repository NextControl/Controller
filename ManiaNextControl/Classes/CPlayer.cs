using ManiaNextControl.Controller;
using ManiaNextControl.Network;
using ManiaplanetXMLRPC.Connector.Gbx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManiaNextControl.Classes
{
    public class CPlayer : HalfClass
    {

        public CUser User;
        public CServerConnection Server;

        public string NickName;
        public int PlayerId;
        public bool IsSpectator;
        public int TeamId;
        public int LadderRanking;

        public bool ControllerLoaded = false;

        // --------- Static
        // *
        public static Dictionary<string, CPlayer> AllPlayers = new Dictionary<string, CPlayer>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="login"></param>
        /// <param name="searcherHelper">If you set a server here, it will only search on this server!</param>
        /// <param name="allowToSearchOnOtherServers">If you have set <paramref name="searcherHelper"/> to True and this value to true, 
        /// it will search on another servers to see if this player exist</param>
        public static async Task<CPlayer> GetPlayerFromLogin(string login, CServerConnection[] searcherHelper = null, bool allowToSearchOnOtherServers = true)
        {
            if (!AllPlayers.TryGetValue(login, out var player))
            {
                retry:
                {
                    var serversToSearch = searcherHelper;
                    if ((serversToSearch == null
                        || serversToSearch.Length == 0)
                        && allowToSearchOnOtherServers)
                        serversToSearch = CManiaNextControl.XmlRPC_Clients.Values.ToArray();
                    foreach (var server in serversToSearch)
                    {
                        await server.RefreshPlayerList();
                        if (!AllPlayers.ContainsKey(login)
                            && allowToSearchOnOtherServers)
                        {
                            allowToSearchOnOtherServers = false; //< TODO: remove this dirty thing
                            searcherHelper = null;

                            goto retry;
                        }
                    }
                }
            }
            return AllPlayers[login];
        }
    }
}
