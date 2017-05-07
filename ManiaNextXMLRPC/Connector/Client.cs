using ManiaplanetXMLRPC.Attributes;
using ManiaplanetXMLRPC.Connector.Gbx;
using ManiaplanetXMLRPC.Structure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using static ManiaplanetXMLRPC.Callbacks.ManiaPlanetCallbacks;

namespace ManiaplanetXMLRPC.Connector
{
	public class Client : Structures.RegisterEvent
	{
		#region Public Fields

		public XmlRPCManager Manager;

		#endregion Public Fields

		#region Private Fields

		private SMapInfo MapInformation = new SMapInfo();

		#endregion Private Fields

		#region Public Methods

		public async Task<bool> CreateConnection(IPAddress address, int port)
		{
			Manager = new XmlRPCManager();
			Manager.Client = this;
			await Manager.Connect(address, port);

			Manager.OnNewCallback += OnGbxCallback;

			return true;
		}

		#endregion Public Methods

		#region Private Methods

		public virtual void OnGbxCallback(CallbackContext cc)
		{
			var gbx = cc.gbx;
			var op = gbx.Parameters;

			switch (gbx.EventName)
			{
				case Code_PlayerConnect:
					TriggerListeners<PlayerConnect>(this, o => o.Callback(this, (string)op[0], (bool)op[1]));
					break;

				case Code_PlayerDisconnect:
					TriggerListeners<PlayerDisconnect>(this, o => o.Callback(this, (string)op[0], (string)op[1]));
					break;

				case Code_PlayerChat:
					if (op.Length == 4)
						TriggerListeners<PlayerChat>(this, o => o.Callback(this, (int)op[0], (string)op[1], (string)op[2], (bool)op[3]));
					break;

				case Code_BeginMap:
					if (op.Length == 1)
						TriggerListeners<BeginMap>(this, o => o.Callback(this, SMapInfo.Convert((Dictionary<string, object>)op[0])));
					break;

				case Code_EndMap:
					if (op.Length == 1)
						TriggerListeners<EndMap>(this, o => o.Callback(this, SMapInfo.Convert((Dictionary<string, object>)op[0])));
					break;

				case Code_MapListModified:
					TriggerListeners<MapListModified>(this, o => o.Callback(this, 0, 0, true));
					break;
				case Code_PlayerManialinkPageAnswer:
					TriggerListeners<PlayerManialinkPageAnswer>(this, o => o.Callback(this, (int)op[0], (string)op[1], (string)op[2]));
					break;
			}
		}

		#endregion Private Methods
	}
}