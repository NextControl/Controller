using ManiaNextControl.Classes;
using ManiaNextControl.FileManagement;
using ManiaNextControl.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ManiaNextControl.Services
{
    public class COnlineMapBrowser : COnlineService
    {
        public class COnlineMap
        {
            public string UId;
            public string Name;
            public string AuthorName;
            public string Comments;
            public string MapType;
        }

        public virtual COnlineMap GetMapFromInternet(CMap map) { return null; }

        public static async Task AddMapFromUrl(CServerConnection con, string mapUrl)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(mapUrl);
            // If required by the server, set the credentials.
            request.Headers["UserAgent"] = "Mozilla/5.0";
            // Get the response.
            WebResponse response = await request.GetResponseAsync();
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream, CodePagesEncodingProvider.Instance.GetEncoding(1252));
            // Read the content.
            string responseFromServer = reader.ReadToEnd();

            //var responseFromServer = await DownloadFileAsync(mapUrl);

            /// Create the map
            await CFileIO.Default.WriteTextAsync("test.Map.Gbx", responseFromServer);
            var tick = Environment.TickCount;
            await con.Events.WriteFile("mx/" + tick + ".Map.Gbx", responseFromServer);
            await con.Events.InsertMap("mx/" + tick + ".Map.Gbx");

            await con.RefreshMapCompletely("null", "mx/" + tick + ".Map.Gbx");
        }

        public static async Task<string> DownloadFileAsync(string url)
        {
            using (System.Net.Http.HttpClient HC = new System.Net.Http.HttpClient())
            {
                return await HC.GetStringAsync(url);
            }
        }
    }
}
