using ManiaplanetXMLRPC.Connector;
using ManiaplanetXMLRPC.Connector.Gbx;
using ManiaplanetXMLRPC.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ManiaplanetXMLRPC.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class CallbackContext : Attribute
    {
        #region Internal Fields

        public Client callerClient;
        public string eventName;
        public bool fromManialink;
        public GbxParam gbx;
        public int operationNumber;
        public object[] result;
        public bool realCall { get; internal set; }

        #endregion Internal Fields

        #region Public Constructors

        public CallbackContext()
        {
            callBackTime = Environment.TickCount;
            result = new object[0];
        }

        #endregion Public Constructors

        #region Internal Properties

        internal double callBackTime { get; }

        #endregion Internal Properties

        #region Public Methods

        public static CallbackContext GetContext<CallbackName>(object caller, [CallerMemberName] string callerName = "") where CallbackName : ICallback
        {
            /* TODO: MethodBase method = typeof(CallbackName).GetMethod(callerName);

            foreach (var m in caller.GetType().GetInterfaceMap(typeof(CallbackName)).TargetMethods)
            {
                method = m;
            }

            IEnumerable<CallbackContext> attr = method.GetCustomAttributes(typeof(CallbackContext), true).Cast<CallbackContext>();
            if (attr == null || attr.Count() == 0)
                throw new Exception("Method don't got the attribute 'CallbackContext'!");

            CallbackContext context = (CallbackContext)attr.ToArray()[0].MemberwiseClone();
            return context;*/
            return null;
        }

        #endregion Public Methods
    }
}