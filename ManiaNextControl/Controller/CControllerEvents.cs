using ManiaNextControl.Classes;
using ManiaNextControl.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace ManiaNextControl.Controller
{
    public class CControllerEvents
    {
        public delegate void delegate_OnGettingMapInformation(CServerConnection con, CMap map);
        public event delegate_OnGettingMapInformation OnGettingMapInformation;
        public void OnGettingMapInformation_Invoke(CServerConnection con, CMap map)
        {
            OnGettingMapInformation?.Invoke(con, map);
        }
    }
}
