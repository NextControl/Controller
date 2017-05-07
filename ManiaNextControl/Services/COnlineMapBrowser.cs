using ManiaNextControl.Classes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ManiaNextControl.Services
{
    public abstract class COnlineMapBrowser : COnlineService
    {
        public class COnlineMap
        {
            public string UId;
            public string Name;
            public string AuthorName;
            public string Comments;
            public string MapType;
        }

        public abstract COnlineMap GetMapFromInternet(CMap map);
    }
}
