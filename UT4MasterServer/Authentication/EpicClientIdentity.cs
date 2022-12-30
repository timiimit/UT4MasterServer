using System.Security.Claims;

namespace UT4MasterServer.Authentication;

public class EpicClientIdentity : ClaimsIdentity
{
	public ClientIdentification Client { get; private set; }

	public EpicClientIdentity(ClientIdentification client) : base(HttpAuthorization.BasicScheme)
	{
		Client = client;

		AddClaim(new Claim("id", client.ID.ToString(), "user"));
		AddClaim(new Claim("secret", client.Secret.ToString(), "user"));
	}
}
