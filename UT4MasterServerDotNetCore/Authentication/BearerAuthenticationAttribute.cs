using Microsoft.AspNetCore.Authorization;

namespace UT4MasterServer.Authorization
{
	public class AuthorizeBearerAttribute : AuthorizeAttribute
	{
		public AuthorizeBearerAttribute()
		{
			AuthenticationSchemes = "bearer";
		}
	}
}