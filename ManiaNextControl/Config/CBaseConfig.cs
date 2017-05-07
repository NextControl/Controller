using ManiaNextControl.FileManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ManiaNextControl.Config
{
    public class CBaseConfig
    {
        private XmlDocument gc_XmlDocument;

        public XmlDocument XmlDocument => gc_XmlDocument;

        public string ConfigPath { get; }    


        public CBaseConfig(string path)
        {
            ConfigPath = path;
        }

        public async Task<string> GetLiveValue(string key)
        {
            return await GetLiveValue(key, string.Empty);
        }

        public async Task<string> GetLiveValue(string key, string defaultValue)
        {
            if (gc_XmlDocument == null)
                gc_XmlDocument = new XmlDocument();
            gc_XmlDocument.LoadXml(await CFileIO.Default.ReadTextAsync(ConfigPath));

            if (gc_XmlDocument.GetElementsByTagName(key).Count == 0
                && !ReferenceEquals(defaultValue, string.Empty))
                return defaultValue;
            return gc_XmlDocument.GetElementsByTagName(key)[0].InnerText;
        }

        public async Task RefreshXmlDoc()
        {
            if (gc_XmlDocument == null)
                gc_XmlDocument = new XmlDocument();
            gc_XmlDocument.LoadXml(File.ReadAllText(await CFileIO.Default.ReadTextAsync(ConfigPath)));
        }
    }
}
