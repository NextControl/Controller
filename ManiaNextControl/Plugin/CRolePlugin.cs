using ManiaNextControl.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace ManiaNextControl.Plugin
{
    public class CRolePlugin : CPlugin
    {
        public CContractResult Request(CContract contract)
        {
            return OnRequestReceived(contract);
        }

        public void SendEvent(string eventName, object[] parameters)
        {
            OnEventReceived(eventName, parameters);
        }

        public virtual CContractResult OnRequestReceived(CContract contract)
        {
            return new CContractResult() { IsError = true, ErrorCode = 0 };
        }

        public virtual void OnEventReceived(string eventName, object[] parameters)
        {

        }

        public virtual void OnGlobalEventReceived(string eventName, object[] parameters, CPlugin caller)
        {

        }
    }
}
