using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UT4MasterServer
{
	public enum SessionType
	{
		AuthorizationCode,
		ExchangeCode,
		ClientCredentials
	}

	public class Session
	{
		public User User { get; set; }
		public string ClientID { get; set; }

		public Token AccessToken { get; set; }
		public Token RefreshToken { get; set; }

		public Session(User user, string clientID, SessionType sessionType)
		{
			User = user;
			ClientID = clientID;
			switch (sessionType)
			{
				case SessionType.AuthorizationCode:
					AccessToken = Token.Generate(TimeSpan.FromDays(1)); // TODO: unsure if same as epic
					RefreshToken = Token.Generate(TimeSpan.FromDays(20)); // TODO: unsure if same as epic
					break;
				case SessionType.ExchangeCode:
					AccessToken = Token.Generate(TimeSpan.FromHours(6)); // same as epic
					RefreshToken = Token.Generate(TimeSpan.FromHours(24)); // same as epic
					break;
				case SessionType.ClientCredentials:
					AccessToken = Token.Generate(TimeSpan.FromHours(4)); // same as epic
					RefreshToken = Token.Generate(TimeSpan.FromHours(24)); // same as epic
					break;
				default:
					throw new ArgumentException("invalid sessionType");
			}
		}

		public void Refresh()
		{

		}

		public string ToJson()
		{
			JObject obj = new JObject();
			obj.Add("access_token", new JValue(AccessToken.Value));
			obj.Add("expires_in", new JValue(AccessToken.ExpirySeconds));
			obj.Add("expires_at", new JValue(AccessToken.Expiration.ToStringEpic()));
			obj.Add("token_type", new JValue("bearer"));
			if (!User.ID.IsSystem)
			{
				obj.Add("refresh_token", new JValue(RefreshToken.Value));
				obj.Add("refresh_expires", new JValue(RefreshToken.ExpirySeconds));
				obj.Add("refresh_expires_at", new JValue(RefreshToken.Expiration.ToStringEpic()));
				obj.Add("account_id", new JValue(User.ID.ToString()));
			}
			obj.Add("client_id", new JValue(ClientID));
			obj.Add("internal_client", new JValue(false));
			obj.Add("client_service", new JValue("ut"));
			if (!User.ID.IsSystem)
			{
				obj.Add("displayName", new JValue(User.Username));
				obj.Add("app", new JValue("ut"));
				obj.Add("in_app_id", new JValue(User.ID.ToString()));
				obj.Add("device_id", new JValue("465a117c2b144b5c8222ee71b9bc8da2")); // unsure about this, probably some ip tracking feature
			}
			return obj.ToString(Newtonsoft.Json.Formatting.None);
		}
	}
}
