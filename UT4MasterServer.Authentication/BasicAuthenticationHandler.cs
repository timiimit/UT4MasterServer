using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using UT4MasterServer.Models;
using UT4MasterServer.Services.Scoped;

namespace UT4MasterServer.Authentication;

public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
	private readonly ClientService clientService;

	public BasicAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger,
		UrlEncoder encoder, ISystemClock clock,
		ClientService clientService) : base(options, logger, encoder, clock)
	{
		this.clientService = clientService;
	}

	protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		return await Task.Run(async () =>
		{
			var authorizationHeader = Request.Headers[HttpAuthorization.AuthorizationHeader];
			var authorization = new HttpAuthorization(authorizationHeader);
			if (!authorization.IsBasic)
			{
				return AuthenticateResult.Fail("unexpected scheme");
			}

			var clientParser = new ClientIdentification(authorization.Value);
			if (clientParser.ID.IsEmpty || string.IsNullOrEmpty(clientParser.Secret))
			{
				return AuthenticateResult.Fail("unexpected value in authorization header");
			}

			var client = await clientService.GetAsync(clientParser.ID);
			if (client == null)
			{
				return AuthenticateResult.Fail("unknown client tried to authenticate");
			}
			if (client.Secret != clientParser.Secret)
			{
				// TODO: since client login was invalid, it means that someone is trying to impersonate
				//       this client or some human error is involved.
				//
				//       someone should be notified that this is happening!
				return AuthenticateResult.Fail("client ID:Secret pair is invalid");
			}

			var principal = new ClaimsPrincipal(new EpicClientIdentity(client));
			var ticket = new AuthenticationTicket(principal, Scheme.Name);
			return AuthenticateResult.Success(ticket);
		});
	}

	protected override Task HandleChallengeAsync(AuthenticationProperties properties)
	{
		// when the scheme is not present, return 401 Unauthorized
		Response.StatusCode = 401;
		return Task.CompletedTask;
	}
}
