using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManiaNextControl.Classes
{
    public class CMap : HalfClass
    {
        private Dictionary<string, object> hash;
        private Dictionary<string, string> stringHash;

        public string Author;
        public int AuthorTime;
        public int BronzeTime;
        public int CopperPrice;
        public string Environnement;
        public string FileName;
        public int GoldTime;
        public int LapRace;
        public string MapStyle;
        public string MapType;
        public string Mood;
        public string Name;
        public int SilverTime;
        public string UId;

        public CMap Convert(Dictionary<string, object> hash)
        {
            this.hash = hash;
            this.stringHash = hash
                .ToDictionary(k => k.Key, v => v.Value.ToString());

            Name = (string)hash["Name"];
            UId = (string)hash["UId"];
            FileName = (string)hash["FileName"];
            Author = (string)hash["Author"];
            Environnement = (string)hash["Environnement"];
            Mood = (string)hash["Mood"];
            AuthorTime = (int)hash["AuthorTime"];
            GoldTime = (int)hash["GoldTime"];
            SilverTime = (int)hash["SilverTime"];
            BronzeTime = (int)hash["BronzeTime"];
            MapStyle = (string)hash["MapStyle"];
            MapType = (string)hash["MapType"];

            return this;
        }

        public Dictionary<string, string> GetStringHashtable()
            => stringHash;

        public Dictionary<string, object> GetHashtable()
            => hash;
    }
}
