using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UT4MasterServer.Models
{
	/// <summary>
	/// class common for all types of codes. these basically exchange identity of user between applications.
	/// they expire after certain amount of time.
	/// </summary>
	[BsonNoId]
	public class CommonCode
	{
		public EpicID AccountID { get; set; }
		public EpicID CreatingClientID { get; set; }
		public Token Token { get; set; }

		public CommonCode(EpicID accountID, EpicID creatingClientID, Token token)
		{
			AccountID = accountID;
			CreatingClientID = creatingClientID;
			Token = token;
		}
	}
}
