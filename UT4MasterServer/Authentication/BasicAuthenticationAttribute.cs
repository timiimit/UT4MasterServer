using Microsoft.AspNetCore.Authorization;

namespace UT4MasterServer.Authentication;

public class AuthorizeBasicAttribute : AuthorizeAttribute
{
	public AuthorizeBasicAttribute()
	{
		AuthenticationSchemes = HttpAuthorization.BasicScheme;
	}
}
