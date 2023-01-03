using System.Text.Json.Serialization;

namespace UT4MasterServer.Models.Requests
{
	public class AuthenticateRequest
	{
		[JsonPropertyName("grant_type")]
		public string? GrantType { get; set; }
		[JsonPropertyName("includePerms")]
		public bool? IncludePerms { get; set; }
		[JsonPropertyName("code")]
		public string? Code { get; set; }
		[JsonPropertyName("exchangeCode")]
		public string? ExchangeCode { get; set; }
		[JsonPropertyName("refreshToken")]
		public string? RefreshToken { get; set; }
		[JsonPropertyName("username")]
		public string? Username { get; set; }
		[JsonPropertyName("password")]
		public string? Password { get; set; }
	}
}
