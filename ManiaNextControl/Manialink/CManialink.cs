using ManiaNextControl.Classes;
using ManiaNextControl.Controller;
using ManiaNextControl.Network;
using ManiaNextControl.Plugin;
using ManiaNextControl.Utils;
using ManiaplanetXMLRPC.Connector;
using ManiaplanetXMLRPC.Connector.Gbx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ManiaNextControl.Manialink
{
    public class CManialink
    {
        public enum EUILayerType
        {
            Default,
            ScoresTable
        }

        #region Public Fields

        public static List<CNCGetterSetter> Getters = new List<CNCGetterSetter>();
        public static List<CNCGetterSetter> Setters = new List<CNCGetterSetter>();
        public static List<CNCEvent> Events = new List<CNCEvent>();

        public bool hasCorrectId = false;
        public bool hasCorrectName = false;
        public object Caller = null;

        #endregion Public Fields

        #region Internal Constructors

        internal CManialink(string contents, object caller)
        {
            Caller = caller == null ? this : caller;
            this.contents = contents;

            hasCorrectId = false;

            if (Caller != this && typeof(CPlugin).IsAssignableFrom(Caller.GetType()))
                ReplaceGS();

            SetVersion(3);
        }

        public void ReplaceGS()
        {
            foreach (var field in Caller.GetType().GetFields())
            {
                if (field.Name.StartsWith("GS_"))
                {
                    contents = contents.Replace($"@{field.Name}", $"NC_{((CPlugin)Caller).manialinkPrefix}_{field.Name} for LocalUser");
                    contents = contents.Replace($"%{field.Name}", $"NC_{((CPlugin)Caller).manialinkPrefix}_{field.Name}");
                }
            }
        }

        public CManialink SetID(string id)
        {
            string idFormat = $"MNC:gc({Caller.GetType().FullName}):id:({id})";

            contents = contents
                .Replace("%mlId", idFormat);

            hasCorrectId = true;

            return this;
        }

        public CManialink SetName(string id)
        {
            string idFormat = $"MNC:gc({Caller.GetType().FullName}):id:({id})";

            hasCorrectName = true;

            contents = contents.
                Replace("%mlName", idFormat);

            return this;
        }

        public CManialink SetParameter(string parameterName, object newValue)
        {
            contents = contents.Replace("%" + parameterName, newValue.ToString());
            Console.WriteLine(contents);
            return this;
        }

        public CManialink SetVersion(int version)
        {
            contents = contents.
                Replace("%mlVersion", version.ToString());

            return this;
        }

        public CManialink SetLayerType(EUILayerType layerType)
        {

            contents = contents
                .Replace("%mlLayerType", layerType.ToString().Replace("EUILayerType.", ""));

            return this;
        }

        #endregion Internal Constructors

        #region Public Properties

        public string contents { get; private set; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Build a manialink
        /// </summary>
        /// <param name="contents">Content to put in</param>
        /// <param name="caller">Optional, replace every % by a GS_ variable of the caller</param>
        /// <returns></returns>
        public static CManialink Build(string contents, object caller = null)
        {
            string finalContent = $@"
<?xml version={"\"1.0\""} encoding={"\"utf-8\""} standalone={"\"yes\""} ?>
<manialink version={"\"%mlVersion\""} name={"\"%mlName\""} id={$"\"%mlId\""} layer={$"\"%mlLayerType\""}> 
{contents}
</manialink>
";
            return new CManialink(finalContent, caller);
        }

        public static string ToCType(Type type)
        {
            if (type == typeof(bool))
                return "Boolean";
            if (type == typeof(int))
                return "Integer";
            if (type == typeof(string))
                return "Text";
            if (type.GetInterfaces().Where(I => I == typeof(IDictionary)).Count() > 0)
            {
                var gt1 = type.GetGenericArguments()[1];
                var gt2 = type.GetGenericArguments()[0];

                return $"{ToCType(gt2)}[{ToCType(gt1)}]";
            }
            return "CNod";
        }

        public static string ToCValue(object value)
        {
            if (value is IDictionary)
            {
                Dictionary<object, object> newDictionary = CastDict((IDictionary)value)
                                           .ToDictionary(entry => (object)entry.Key,
                                                         entry => (object)entry.Value);
                Debug.CDebug.ErrorLog(((IDictionary)value).Count);

                int currIndex = 0;
                StringBuilder builder = new StringBuilder("[");
                foreach (var kvp in newDictionary)
                {

                    currIndex++;
                    builder.Append($"{ToCValue(kvp.Key)} => {ToCValue(kvp.Value)}");
                    if (currIndex < newDictionary.Count())
                        builder.Append(",");
                }
                builder.Append("]");

                return builder.ToString();
            }
            if (!(value is string))
            {
                return value.ToString();
            }
            else
            {
                return "\""+ ((string)value).Replace("\\", "\\\\") +"\"";
            }
        }

        private static IEnumerable<DictionaryEntry> CastDict(IDictionary dictionary)
        {
            foreach (DictionaryEntry entry in dictionary)
            {
                yield return entry;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="server">Parameter ignored if players is not empty. If null, this will be sent to all servers</param>
        /// <param name="timeBeforeHide"></param>
        /// <param name="players"></param>
        public async void Send(XmlRPCManager[] servers = null, int timeBeforeHide = 0, params CPlayer[] players)
        {
            if (!hasCorrectId)
                throw new IncorrectIDException("No ID was put.");
            if (!hasCorrectName)
                throw new IncorrectNameException("No name was put.");

            if (players.Count() == 0)
            {
                var sendToServers = servers;
                if (sendToServers == null
                    || sendToServers.Length == 0)
                    sendToServers = CManiaNextControl.XmlRPC_Clients.Select(s => s.Value.Manager).ToArray();

                foreach (var server in sendToServers)
                    await server.AsyncSendCall(GbxParam.Create("SendDisplayManialinkPage", contents ?? "", timeBeforeHide, false), (gbx) =>
                    {
                        Console.WriteLine($"result: hasError:{gbx.isError} errorCode:{gbx.ErrorCode} errorString:{gbx.ErrorString}");
                    });
            }
            else
            {
                foreach (var player in players)
                {
                    await player.Server.Manager.AsyncSendCall(GbxParam.Create("SendDisplayManialinkPageToLogin", player.User.Login, contents ?? "", timeBeforeHide, false), (gbx) =>
                    {
                        Console.WriteLine($"result: hasError:{gbx.isError} errorCode:{gbx.ErrorCode} errorString:{gbx.ErrorString}");
                    });
                }
            }
        }

        #endregion Public Methods

        #region Internal Methods

        internal static void Set_SendNow(CNCGetterSetter setter)
        {
            string scriptContent = "declare Integer NC_UpdateTick for LocalUser = Now; NC_UpdateTick = Now;";
            scriptContent += $"declare Integer NC_EnvironnementTick for LocalUser = Now; NC_EnvironnementTick = {Environment.TickCount};";
            scriptContent += $"declare NC_{setter.varPrefix}_{setter.varName} for LocalUser = {CManialink.ToCValue(setter.Value)};";
            scriptContent += $"NC_{setter.varPrefix}_{setter.varName} = {CManialink.ToCValue(setter.Value)};";
            scriptContent += $"log(NC_{setter.varPrefix}_{setter.varName});";

            string content = $@"
<?xml version={"\"1.0\""} encoding={"\"utf-8\""} standalone={"\"yes\""} ?>
<manialink version={"\"2\""} name={"\"NC2-Var-Updater\""} id={$"\"NC2:gc:({setter.varPrefix}{setter.varName}):time{Environment.TickCount}\""}>
<script><!--
main() {"{"} {scriptContent} {"}"}
--></script>
</manialink>
";

            CManialink manialink = new CManialink(content, null) { hasCorrectId = true, hasCorrectName = true };
            if (setter.Servers == null
                || setter.Servers.Count() == 0)
                manialink.Send(null, 100, setter.Players);
            else
            {
                manialink.Send(setter.Servers.Select(s => s.Manager).ToArray(), 100, setter.Players);
            }
        }

        internal static void Loop()
        {
            try
            {
                foreach (var setter in Setters)
                {
                    Set_SendNow(setter);
                }
                foreach (var _event in Events)
                {
                    string scriptContent = "declare Integer NC_UpdateTick for LocalUser = Now; NC_UpdateTick = Now;";

                    var eventVars = new StringBuilder("[");
                    int index = 0;
                    foreach (var toAdd in _event.Data)
                    {
                        eventVars.Append('"');
                        eventVars.Append(toAdd);
                        eventVars.Append('"');
                        if (index < _event.Data.Length - 1)
                            eventVars.Append(',');
                        index++;
                    }
                    eventVars.Append(']');

                    scriptContent += $"\n// Events part\n";
                    scriptContent += $@"SendCustomEvent(""MNC"", {eventVars.ToString()}); log(""event sent!"");";
                    string content = $@"
<?xml version={"\"1.0\""} encoding={"\"utf-8\""} standalone={"\"yes\""} ?>
<manialink version={"\"2\""} name={"\"NC2-Events\""} id={$"\"NC2:ev:({_event.varPrefix}{_event.varName}):time{Environment.TickCount}\""}>
<script><!--
main() {"{"} {scriptContent} {"}"}
--></script>
</manialink>
";

                    CManialink manialink = new CManialink(content, null) { hasCorrectId = true, hasCorrectName = true };
                    manialink.Send(null, 15000, _event.Players);
                }

                Setters = new List<CNCGetterSetter>();
                Getters = new List<CNCGetterSetter>();
                Events = new List<CNCEvent>();
            }
            catch (Exception ex)
            {
                bool safe = true;
                if (ex is NullReferenceException)
                {
                    Setters.RemoveAll(o => o == null || o.Value == null || o.varName == null || o.varPrefix == null);
                }
                if (safe)
                    Loop();
            }
        }

        #endregion Internal Methods

        #region Public Classes

        public class CNCGetterSetter
        {
            #region Public Fields

            public CPlayer[] Players;
            public object Value;

            public CServerConnection[] Servers;

            #endregion Public Fields

            #region Internal Fields

            internal string varName;
            internal string varPrefix;

            #endregion Internal Fields
        }

        public class CNCEvent
        {
            #region Public Fields

            public CPlayer[] Players;
            public string[] Data;

            public CServerConnection Server;

            #endregion Public Fields

            #region Internal Fields

            internal string varName;
            internal string varPrefix;

            #endregion Internal Fields
        }

        public class AnswerTree<T>
        {
            public static AnswerTree<object> Zero = new AnswerTree<object>(null);

            internal struct wR
            {
                public enum EWa
                {
                    Is,
                    Result
                }

                public EWa wantedAction;
                public string name;
            }

            public string main;
            public string response;
            internal List<wR> waitedResult;
            public Type wantedType;

            public AnswerTree(Action<AnswerTree<T>> creator)
            {
                if (creator == null)
                    return;
                creator(this);
            }

            public AnswerTree<T> Is(string nameOf)
            {
                waitedResult.Add(new wR() { wantedAction = wR.EWa.Is, name = nameOf });
                return this;
            }

            public void Result()
            {
                waitedResult.Add(new wR() { wantedAction = wR.EWa.Result, name = "_result" });
                wantedType = typeof(T);
            }
        }

        public class PlayerAnswerAttribute : Attribute
        {
            public PlayerAnswerAttribute(bool fullVersion)
            {

            }
        }

        public class SharerAction<T>
        {
            #region Public Fields

            public readonly string varName;
            public readonly string varPrefix;

            #endregion Public Fields

            #region Private Fields

            public T _currValue;

            #endregion Private Fields

            #region Public Constructors

            public SharerAction(string variableName, string variablePrefix)
            {
                varName = variableName;
                varPrefix = variablePrefix;
            }

            #endregion Public Constructors

            #region Public Methods

            // TODO : CManialink.Getter
            public async Task<T> Get()
            {
                /*if (_wPlayers.Count() == 1)
                {
                    _currValue = await GetFromPlayer(_wPlayers[0]);
                }*/
                return _currValue;
            }

            public async Task<T> GetFromPlayer(params CPlayer[] pls)
            {
                CManialink.Getters.Add(new CNCGetterSetter() { Players = pls });
                return default(T);
            }

            /// <summary>
            /// Variables will be send every 0.1 ms
            /// </summary>
            /// <param name="value"></param>
            /// <param name="pls"></param>
            public void SetForPlayers(T value, params CPlayer[] pls)
            {
                CManialink.Setters.Add(new CNCGetterSetter() { Value = value, Players = pls, varName = varName, varPrefix = varPrefix });
            }

            /// <summary>
            /// Variables will be send every 0.1
            /// </summary>
            /// <param name="value"></param>
            /// <param name="srvs"></param>
            public void SetForServers(T value, params CServerConnection[] srvs)
            {
                CManialink.Setters.Add(new CNCGetterSetter() { Value = value, Servers = srvs, varName = varName, varPrefix = varPrefix });
            }

            /// <summary>
            /// Variable will be sent instantly to the players
            /// </summary>
            /// <param name="value"></param>
            /// <param name="pls"></param>
            public void SetNowForPlayers(T value, params CPlayer[] pls)
            {
                CManialink.Set_SendNow(new CNCGetterSetter() { Value = value, Players = pls, varName = varName, varPrefix = varPrefix });
            }

            /// <summary>
            /// Variable will be sent instantly to the servers
            /// </summary>
            /// <param name="value"></param>
            /// <param name="pls"></param>
            public void SetNowForServers(T value, params CServerConnection[] srvs)
            {
                CManialink.Set_SendNow(new CNCGetterSetter() { Value = value, Servers = srvs, varName = varName, varPrefix = varPrefix });
            }

            public void SendEventForPlayers(string[] values, params CPlayer[] pls)
            {
                CManialink.Events.Add(new CNCEvent() { Data = values, Players = pls, varName = varName, varPrefix = varPrefix });
            }

            #endregion Public Methods
        }

        public class IncorrectIDException : Exception { public IncorrectIDException(string msg) : base(msg) { } }
        public class IncorrectNameException : Exception { public IncorrectNameException(string msg) : base(msg) { } }

        #endregion Public Classes
    }
}
