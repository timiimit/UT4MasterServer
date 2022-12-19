using System.Security.Claims;
using System.Security.Principal;
using UT4MasterServer.Models;

namespace UT4MasterServer.Authorization
{
	public class EpicUserIdentity : ClaimsIdentity
	{
		public Session Session { get; set; }
		public string AccessToken { get; set; }

		public EpicUserIdentity(string accessToken, Session session) : base("bearer")
		{
			AccessToken = accessToken;
			Session = session;

			AddClaim(new Claim("access_token", accessToken, ClaimValueTypes.String, "user"));
			AddClaim(new Claim("session_id", session.ID.ToString(), "server"));
		}
	}
}