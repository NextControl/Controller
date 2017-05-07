using ManiaplanetXMLRPC.Attributes;
using ManiaplanetXMLRPC.Connector;
using ManiaplanetXMLRPC.Interfaces;
using ManiaplanetXMLRPC.Structure;

namespace ManiaplanetXMLRPC.Callbacks
{
    public class ManiaPlanetCallbacks
    {
        // TODO: Add mising codes, and missing callbacks

        // Players
        public const string Code_PlayerConnect = "ManiaPlanet.PlayerConnect";
        public interface PlayerConnect : ICallback {[CallbackContext] void Callback(Client con, string Login, bool isSpectator); }
        public const string Code_PlayerDisconnect = "ManiaPlanet.PlayerDisconnect";
        public interface PlayerDisconnect : ICallback {[CallbackContext] void Callback(Client con, string Login, string DisconnectionReason); }
        public const string Code_PlayerChat = "ManiaPlanet.PlayerChat";
        public interface PlayerChat : ICallback {[CallbackContext] void Callback(Client con, int PlayerUid, string Login, string Text, bool IsRegistredCmd); }

        // Map
        public const string Code_BeginMap = "ManiaPlanet.BeginMap";
        public interface BeginMap : ICallback {[CallbackContext] void Callback(Client con, SMapInfo Map); }
		public const string Code_EndMap = "ManiaPlanet.EndMap";
		public interface EndMap : ICallback {[CallbackContext] void Callback(Client con, SMapInfo Map); }
		public const string Code_MapListModified = "ManiaPlanet.MapListModified";
        public interface MapListModified : ICallback {[CallbackContext] void Callback(Client con, int curMapIndex, int NextMapIndex, bool IsListModified); }

		// Manialink
		public const string Code_PlayerManialinkPageAnswer = "ManiaPlanet.PlayerManialinkPageAnswer";
		public interface PlayerManialinkPageAnswer : ICallback {[CallbackContext] void Callback(Client con, int PlayerUid, string Login, string Answer); }
	}
}