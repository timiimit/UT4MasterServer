using Microsoft.AspNetCore.Authorization;

namespace UT4MasterServer.Authentication;

public class AuthorizeBearerAttribute : AuthorizeAttribute
{
	public AuthorizeBearerAttribute()
	{
		AuthenticationSchemes = HttpAuthorization.BearerScheme;
	}
}
