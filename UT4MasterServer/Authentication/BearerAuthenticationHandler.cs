// Copyright (c) Barry Dorrans. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using UT4MasterServer.Authorization;
using System.Security.Claims;
using UT4MasterServer.Services;

namespace UT4MasterServer.Authentication;

public class BearerAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
	private readonly SessionService sessionService;

	public BearerAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger,
		UrlEncoder encoder, ISystemClock clock, SessionService sessionService) : base(options, logger, encoder, clock)
	{
		this.sessionService = sessionService;
	}

	protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		var authorizationHeader = Request.Headers["Authorization"];
		var authorization = new HttpAuthorization(authorizationHeader);
		if (!authorization.IsBearer)
		{
			const string errMessage = $"trying to handle a scheme that is not 'bearer' inside bearer scheme handler";
			Logger.LogInformation(errMessage);
			return AuthenticateResult.Fail(errMessage);
		}

		var session = await sessionService.GetSessionAsync(authorization.Value);
		if (session == null)
		{
			const string errMessage = $"invalid token";
			Logger.LogInformation(errMessage);
			return AuthenticateResult.Fail(errMessage);
		}

		var principal = new ClaimsPrincipal(new EpicUserIdentity(authorization.Value, session));
		var ticket = new AuthenticationTicket(principal, Scheme.Name);
		return AuthenticateResult.Success(ticket);
	}

	protected override Task HandleChallengeAsync(AuthenticationProperties properties)
	{
		// when the scheme is not present, return 401 Unauthorized
		Response.StatusCode = 401;
		return Task.CompletedTask;
	}
}