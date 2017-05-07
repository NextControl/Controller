using System;
using System.Collections.Generic;
using System.Text;

namespace ManiaNextControl.Config
{
    public class CServerConfig : CBaseConfig
    {
        public string ServerLogin;
        public string SuperAdminLogin = "SuperAdmin";
        public string SuperAdminPassword = "SuperAdmin";
        public string IPAddress = "127.0.0.1";
        public int Port = -1;

        public Dictionary<string, string> PluginsConfig = new Dictionary<string, string>();

        public CServerConfig(string path) : base(path)
        { }


    }
}
