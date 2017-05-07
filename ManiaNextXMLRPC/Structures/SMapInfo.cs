using System.Collections;
using System.Collections.Generic;

namespace ManiaplanetXMLRPC.Structure
{
	public struct SMapInfo
	{
		#region Public Fields

		public string Author;
		public int AuthorTime;
		public int BronzeTime;
		public int CopperPrice;
		public string Environnement;
		public string FileName;
		public int GoldTime;
		public bool LapRace;
		public string MapStyle;
		public string MapType;
		public string Mood;
		public string Name;
		public int NbCheckpoints;
		public int NbLaps;
		public int SilverTime;
		public string UId;

		#endregion Public Fields

		#region Public Methods

		public static SMapInfo Convert(Dictionary<string, object> hash)
		{
			var mi = new SMapInfo();

			mi.Name = (string)hash["Name"];
			mi.UId = (string)hash["UId"];
			mi.FileName = (string)hash["FileName"];
			mi.Author = (string)hash["Author"];
			mi.Environnement = (string)hash["Environnement"];
			mi.Mood = (string)hash["Mood"];

			return mi;
		}

		#endregion Public Methods
	}
}