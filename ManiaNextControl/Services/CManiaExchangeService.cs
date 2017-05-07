using System;
using System.Collections.Generic;
using System.Text;
using ManiaNextControl.Classes;

namespace ManiaNextControl.Services
{
    public class CManiaExchangeService : COnlineMapBrowser
    {
        public class CMX_Map : COnlineMap
        {

        }

        public CMX_Map GetFromUId(string guid)
        {
            return null;
        }

        public CMX_Map GetFromName(string name)
        {
            return null;
        }

        public override COnlineMap GetMapFromInternet(CMap map)
        {
            throw new NotImplementedException();
        }
    }
}
