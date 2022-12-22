using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UT4MasterServer
{
	public class ExchangeCode
	{
		public Session AssociatedSession { get; set; }
		public Token Token { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="associatedSession">session this exchange code was created with</param>
		/// <param name="token"></param>
		/// <param name="creatingClientId">Client that created this exchange code</param>
		public ExchangeCode(Session associatedSession, Token token)
		{
			AssociatedSession = associatedSession;
			Token = token;
		}

		public string ToJson()
		{
			JObject obj = new JObject();
			obj.Add("expiresInSeconds", Token.ExpirySeconds);
			obj.Add("code", Token.Value);
			obj.Add("creatingClientId", AssociatedSession.ClientID);
			return obj.ToString(Newtonsoft.Json.Formatting.None);
		}
	}
}
