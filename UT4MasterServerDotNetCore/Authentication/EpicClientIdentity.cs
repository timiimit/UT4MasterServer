using System.Security.Claims;
using System.Security.Principal;

namespace UT4MasterServer.Authorization
{
	public class EpicClientIdentity : ClaimsIdentity
	{
		public ClientIdentification Client { get; private set; }

		public EpicClientIdentity(ClientIdentification client) : base("basic")
		{
			Client = client;

			AddClaim(new Claim("id", client.ID.ToString(), "user"));
			AddClaim(new Claim("secret", client.Secret.ToString(), "user"));
		}
	}
}