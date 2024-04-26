using System.Net;

namespace UT4MasterServer.Authentication;

public class HttpAuthorization
{
	public string Type { get; set; } = string.Empty;
	public string Value { get; set; } = string.Empty;

	public const string AuthorizationHeader = "Authorization";
	public const string BearerScheme = "Bearer";
	public const string BasicScheme = "Basic";

	public bool IsBearer => Type.ToLower() == BearerScheme.ToLower();
	public bool IsBasic => Type.ToLower() == BasicScheme.ToLower();

	public HttpAuthorization(HttpListenerRequest request) : this(request.Headers[AuthorizationHeader])
	{
	}

	public HttpAuthorization(string? authorizationHeader)
	{
		if (authorizationHeader == null)
		{
			return;
		}

		int space = authorizationHeader.IndexOf(' ');
		if (space == -1)
		{
			Value = authorizationHeader;
			return;
		}

		Type = authorizationHeader.Substring(0, space);
		Value = authorizationHeader.Substring(space + 1);
	}
}
