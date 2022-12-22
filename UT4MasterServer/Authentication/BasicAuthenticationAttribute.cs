
using Microsoft.AspNetCore.Authorization;

namespace UT4MasterServer.Authorization;

public class AuthorizeBasicAttribute : AuthorizeAttribute
{
	public AuthorizeBasicAttribute()
	{
		AuthenticationSchemes = "basic";
	}
}