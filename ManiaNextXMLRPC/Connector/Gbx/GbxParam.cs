using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Linq;
using System.Globalization;

namespace ManiaplanetXMLRPC.Connector.Gbx
{
    public class GbxParam
    {
        #region Public Fields

        public int ErrorCode;
        public string ErrorString;
        public string EventName;
        public int Handle;
        public bool isError;
        public object[] Parameters;
        public string Result;
        public MessageType Type;

        #endregion Public Fields

        #region Public Enums

        public enum MessageType
        {
            None,
            Response,
            Request,
            Callback
        }

        #endregion Public Enums

        #region Public Methods

        public static GbxParam Create(string eventName)
        {
            return Create(eventName, new object[0]);
        }

        public static GbxParam LegacyCreate(string eventName, object param)
        {
            return Instantiate(eventName, new object[] { param });
        }

        public static GbxParam Create(string eventName, params object[] oParams) => Instantiate(eventName, oParams);

        public static GbxParam Instantiate(string eventName, object[] oParams)
        {
            var gbx = new GbxParam();
            gbx.Parameters = oParams;
            gbx.EventName = eventName;

            StringBuilder xml = new StringBuilder("<?xml version=\"1.0\" ?><methodCall><methodName>" + eventName + "</methodName><params>");
            foreach (var obj in gbx.Parameters)
            {
                // Initialize
                Console.WriteLine(obj.GetType());
                xml.Append("<param>" + ParseObject(obj) + "</param>\n");
            }
            xml.Append("</params></methodCall>");
            gbx.Result = xml.ToString();
            return gbx;
        }

        public static string ParseObject(object obj)
        {
            // open parameter ...
            string xml = "<value>";

            if (!obj.GetType().IsArray)
            {
                if (obj.GetType() == typeof(string)) // parse type string ...
                {
                    xml += "<string>" + WebUtility.HtmlEncode((string)obj) + "</string>";
                }
                else if (obj.GetType() == typeof(int)) // parse type int32 ...
                {
                    xml += "<int>" + (int)obj + "</int>";
                }
                else if (obj.GetType() == typeof(double)) // parse type double ...
                {
                    xml += "<double>" + (double)obj + "</double>";
                }
                else if (obj.GetType() == typeof(bool))  // parse type bool ...
                {
                    if ((bool)obj)
                        xml += "<boolean>1</boolean>";
                    else
                        xml += "<boolean>0</boolean>";
                }
            }
            else if (obj.GetType().IsArray)
            {
                xml += "<array><data>";
                foreach (object element in ((Array)obj))
                {
                    xml += ParseObject(element);
                }
                xml += "</data></array>";
            }
            else if (obj.GetType().GetInterfaces().Contains(typeof(IEnumerable))) // parse type array ...
            {
                xml += "<array><data>";
                foreach (object element in ((IEnumerable)obj))
                {
                    xml += ParseObject(element);
                }
                xml += "</data></array>";
            }
            else if (obj.GetType() == typeof(Dictionary<string, object>)) // parse type struct ...
            {
                xml += "<struct>";
                foreach (var key in ((Dictionary<string, object>)obj))
                {
                    xml += "<member>";
                    xml += "<name>" + key.ToString() + "</name>";
                    xml += ParseObject(key.Value);
                    xml += "</member>";
                }
                xml += "</struct>";
            }
            else if (obj.GetType() == typeof(byte[])) // parse type of byte[] into base64
            {
                xml += "<base64>";
                xml += Convert.ToBase64String((byte[])obj);
                xml += "</base64>";
            }

            // close parameter ...
            return xml + "</value>\n";
        }

        static public object ParseXml(XmlElement inParam)
        {
            XmlElement val;
            if (inParam["value"] == null)
            {
                val = inParam;
            }
            else
            {
                val = inParam["value"];
            }

            if (val["string"] != null) // param of type string ...
            {
                return val["string"].InnerText;
            }
            else if (val["int"] != null) // param of type int32 ...
            {
                return Int32.Parse(val["int"].InnerText);
            }
            else if (val["i4"] != null) // param of type int32 (alternative) ...
            {
                return Int32.Parse(val["i4"].InnerText);
            }
            else if (val["double"] != null) // param of type double ...
            {
                return double.Parse(val["double"].InnerText.Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator)
                                                           .Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator));
            }
            else if (val["boolean"] != null) // param of type boolean ...
            {
                if (val["boolean"].InnerText == "1")
                    return true;
                else
                    return false;
            }
            else if (val["struct"] != null) // param of type struct ...
            {
                var structure = new Dictionary<string, object>();
                foreach (XmlElement member in val["struct"])
                {
                    // parse each member ...
                    structure.Add(member["name"].InnerText, ParseXml(member));
                }
                return structure;
            }
            else if (val["array"] != null) // param of type array ...
            {
                var array = new List<object>();
                foreach (XmlElement data in val["array"]["data"])
                {
                    // parse each data field ...
                    array.Add(ParseXml(data));
                }
                return array;
            }
            else if (val["base64"] != null) // param of type base64 ...
            {
                byte[] data = Convert.FromBase64String(val["base64"].InnerText);
                return data;
            }

            return null;
        }

        #endregion Public Methods

        #region Internal Methods

        internal static GbxParam ParseResponse(int handle, byte[] data)
        {
            GbxParam gbx = new GbxParam();

            gbx.Type = MessageType.None;
            gbx.Handle = handle;
            gbx.Result = Encoding.UTF8.GetString(data);
            gbx.ErrorCode = 0;
            gbx.ErrorString = "";

            Console.WriteLine(gbx.Result);

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(gbx.Result);
            XmlElement methodParams = null;

            // message is of type request ...
            if (xmlDoc["methodCall"] != null)
            {
                // check message type ...
                if (handle > 0)
                    gbx.Type = MessageType.Callback;
                else
                    gbx.Type = MessageType.Request;

                // try to get the method name ...
                if (xmlDoc["methodCall"]["methodName"] != null)
                {
                    gbx.EventName = xmlDoc["methodCall"]["methodName"].InnerText;
                }
                else
                    gbx.isError = true;

                // try to get the mehtod's parameters ...
                if (xmlDoc["methodCall"]["params"] != null)
                {
                    gbx.isError = false;
                    methodParams = xmlDoc["methodCall"]["params"];
                }
                else
                    gbx.isError = true;
            }
            else if (xmlDoc["methodResponse"] != null) // message is of type response ...
            {
                // check message type ...
                gbx.Type = MessageType.Response;

                if (xmlDoc["methodResponse"]["fault"] != null)
                {
                    var errStruct = (Dictionary<string, object>)ParseXml(xmlDoc["methodResponse"]["fault"]);
                    gbx.ErrorCode = (int)errStruct["faultCode"];
                    gbx.ErrorString = (string)errStruct["faultString"];
                    gbx.isError = true;
                }
                else if (xmlDoc["methodResponse"]["params"] != null)
                {
                    gbx.isError = false;
                    methodParams = xmlDoc["methodResponse"]["params"];
                }
                else
                {
                    gbx.isError = true;
                }
            }
            else
            {
                gbx.isError = true;
            }

            // parse each parameter of the message, if there are any ...
            List<object> parameters = new List<object>();
            if (methodParams != null)
            {
                foreach (XmlElement param in methodParams)
                {
                    parameters.Add(GbxParam.ParseXml(param));
                }
            }
            gbx.Parameters = parameters.ToArray();

            return gbx;
        }

        #endregion Internal Methods
    }
}