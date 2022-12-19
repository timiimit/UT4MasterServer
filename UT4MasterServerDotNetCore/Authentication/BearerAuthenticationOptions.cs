

using Microsoft.AspNetCore.Authentication;
using UT4MasterServer.Services;

namespace UT4MasterServer.Authentication
{
	public class BearerAuthenticationOptions : AuthenticationSchemeOptions
	{
		public SessionService SessionService { get; set; }

		public BearerAuthenticationOptions(SessionService sessionService)
		{
			SessionService = sessionService;
		}
	}
}