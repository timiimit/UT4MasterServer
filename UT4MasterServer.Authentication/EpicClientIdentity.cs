using System.Security.Claims;
using UT4MasterServer.Models.Database;

namespace UT4MasterServer.Authentication;

public class EpicClientIdentity : ClaimsIdentity
{
	public Client Client { get; private set; }

	public EpicClientIdentity(Client client) : base(HttpAuthorization.BasicScheme)
	{
		Client = client;

		AddClaim(new Claim("id", client.ID.ToString(), "user"));
		AddClaim(new Claim("secret", client.Secret.ToString(), "user"));
	}
}
