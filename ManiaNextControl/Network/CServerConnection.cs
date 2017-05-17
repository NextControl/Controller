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
using ManiaNextControl.Services;
using System.Reflection;

namespace ManiaNextControl.Network
{
    public partial class CServerConnection : Client
    {
        public Dictionary<Type, CService> ServerServices = new Dictionary<Type, CService>();

        public CServerConnectionEvents Events;

        public Dictionary<string, CMap> MapList = new Dictionary<string, CMap>();
        public Dictionary<string, CPlayer> Players
            => CPlayer.AllPlayers.Where(player => player.Value.Server == this)
                .ToDictionary(k => k.Key, v => v.Value);

        public bool IsLoaded = false;

        public CServerInfo ServerInfo;

        public CServerConnection()
        {
            Events = new CServerConnectionEvents()
            {
                con = this
            };
        }

        public override void OnGbxCallback(CallbackContext cc)
        {
            ProcessCallback(cc);
        }

        public async Task Refresh()
        {
            await RefreshServerInfo();
            await RefreshPlayerList();
            await RefreshMapList();
            await ApplyInterfaceHelpers();
        }

        public async Task RefreshServerInfo()
        {
            var call = await Manager.AsyncSendCall(GbxParam.Create("GetMainServerPlayerInfo"));
            var hash = (Dictionary<string, object>)call.Parameters[0];

            ServerInfo = new CServerInfo();
            ServerInfo.Login = (string)hash["Login"];

            if (CManiaNextControl.XmlRPC_Clients.ContainsValue(this))
            {
                var keys = new List<string>();
                foreach (var kvp in CManiaNextControl.XmlRPC_Clients)
                {
                    if (ServerInfo.Login == kvp.Key
                        && this != kvp.Value)
                    {
                        CManiaNextControl.XmlRPC_Clients[kvp.Key] = this;
                    }
                    else if (kvp.Key != ServerInfo.Login
                        && this == kvp.Value)
                    {
                        keys.Add(kvp.Key);
                    }
                }
                for (int i = 0; i < keys.Count; i++)
                    CManiaNextControl.XmlRPC_Clients.Remove(keys[i]);
                CManiaNextControl.XmlRPC_Clients[ServerInfo.Login] = this;
            }
        }

        public async Task RefreshPlayerList()
        {
            var call = await Manager.AsyncSendCall(GbxParam.Create("GetPlayerList", 510, 0));
            foreach (Dictionary<string, object> dico in (List<object>)call.Parameters[0])
            {
                if (!CPlayer.AllPlayers.TryGetValue((string)dico["Login"], out var player))
                {
                    player = new CPlayer()
                    {
                        User = new CUser()
                        {
                            Login = (string)dico["Login"]
                        },
                        NickName = (string)dico["NickName"],
                        PlayerId = (int)dico["PlayerId"],
                        TeamId = (int)dico["TeamId"],
                        IsSpectator = (int)dico["TeamId"] > 0,
                        LadderRanking = (int)dico["LadderRanking"],
                        Server = this,
                    };
                }
                else
                {
                    player.NickName = (string)dico["NickName"];
                    player.PlayerId = (int)dico["PlayerId"];
                    player.TeamId = (int)dico["TeamId"];
                    player.IsSpectator = (int)dico["TeamId"] > 0;
                    player.LadderRanking = (int)dico["LadderRanking"];
                    player.Server = this;
                }

                player.LoadingState = HalfClass.CurrentState.PrimaryInfoFilled;

                CPlayer.AllPlayers[player.User.Login] = player;

                // Get detailed info
                call = await Manager.AsyncSendCall(GbxParam.Create("GetDetailedPlayerInfo", player.User.Login));

                var dicoDetail = (Dictionary<string, object>)call.Parameters[0];
                player.User.IPAddress = (string)dicoDetail["IPAddress"];
                player.User.DownloadRate = (int)dicoDetail["DownloadRate"];
                player.User.UploadRate = (int)dicoDetail["UploadRate"];
                player.User.Language = (string)dicoDetail["Language"];

                player.User.Player = player;

                player.LoadingState = HalfClass.CurrentState.AllInfoFilled;
            }
        }

        public async Task RefreshMapCompletely(string uid, string filemapname)
        {
            MapList.TryGetValue(uid, out var map);
            if (map == null)
                map = new CMap()
                {
                    UId = uid,
                    FileName = filemapname
                };

            var hashtable = (await Manager.AsyncSendCall(GbxParam.Create("GetMapInfo", map.FileName)))
                .Parameters[0]
                .TypeCast<Dictionary<string, object>>();

            map.Convert(hashtable);
            map.LoadingState = HalfClass.CurrentState.AllInfoFilled;

            CManiaNextControl.Events.OnGettingMapInformation_Invoke(this, map);
        }

        public async Task RefreshMapList()
        {
            var fileNames = new List<string>();
            (await Manager.AsyncSendCall(GbxParam.Create("GetMapList", 2000, 0)))
                .Parameters[0]
                .TypeCast<List<object>>()
                .ForEach((object value) =>
                {
                    var hashtable = value.TypeCast<Dictionary<string, object>>();
                    string fileName = (string)hashtable["FileName"];
                    string UId = (string)hashtable["UId"];

                    if (!MapList.ContainsKey(UId))
                        MapList[UId] = new CMap();

                    var map = MapList[UId];
                    map.UId = UId;
                    map.FileName = fileName;
                    map.Name = (string)hashtable["Name"];
                    map.Author = (string)hashtable["Author"];
                    map.Environnement = (string)hashtable["Environnement"];
                    map.LoadingState = HalfClass.CurrentState.PrimaryInfoFilled;

                    fileNames.Add(fileName);
                }
           );
        }

        public async Task ApplyFullInformationsToMaps()
        {
            foreach (var map in MapList.Values)
            {
                var hashtable = (await Manager.AsyncSendCall(GbxParam.Create("GetMapInfo", map.FileName)))
                    .Parameters[0]
                    .TypeCast<Dictionary<string, object>>();

                map.Convert(hashtable);
                map.LoadingState = HalfClass.CurrentState.AllInfoFilled;

                CManiaNextControl.Events.OnGettingMapInformation_Invoke(this, map);
            }
        }

        public async Task ApplyInterfaceHelpers(params CPlayer[] players)
        {
            CManialink.Build(@"
<script>
    <!--
  main()
  {
    declare MNC_ScoresTableVisible for LocalUser = False;
    while(True)
    {
      yield;
      MNC_ScoresTableVisible = PageIsVisible;
    }
  }
--></script>
")
            .SetName("VisibleScoresTable")
            .SetID("visiblescorestable")
            .SetLayerType(CManialink.EUILayerType.ScoresTable)
            .Send(new[] { Manager }, 0, players);
        }

        public Service GetService<Service>()
            where Service : CService
        {
            if (!typeof(Service).GetInterfaces().Contains(typeof(IServiceServer)))
                return null;

            if (ServerServices.ContainsKey(typeof(Service)))
                return (Service)ServerServices[typeof(Service)];

            if (typeof(Service).GetInterfaces().Contains(typeof(IServiceManualStart)))
                return (Service)CService.Service_NeedToBeStartedManually;
            else if (typeof(Service).GetInterfaces().Contains(typeof(IServiceStartOnDemand)))
            {
                var s = (IServiceServer)(ServerServices[typeof(Service)] = default(Service));
                s.Service_ServerConnection = this;
                s.ServerAdded();

                if (IsLoaded)
                    s.ServerLoaded();

                return (Service)s;
            }
            else
                return null;
        }

        public CService GetServiceByType(string type)
        {
            type = type.ToLower();

            foreach (var kvp in CManiaNextControl.servicesInformation)
            {
                if (kvp.Value.serviceType.ToLower() == type)
                {
                    return (CService)GetType().GetMethod("GetService").MakeGenericMethod(kvp.Key).Invoke(this, null);
                }
            }

            return null;
        }
    }
}
