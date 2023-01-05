using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace UT4MasterServer.Models.Requests
{
	public class AuthenticateRequest
	{
		[FromForm(Name = "grant_type")]
		public string? GrantType { get; set; }
		[FromForm(Name = "includePerms")]
		public bool? IncludePerms { get; set; }
		[FromForm(Name = "code")]
		public string? Code { get; set; }
		[FromForm(Name = "exchange_code")]
		public string? ExchangeCode { get; set; }
		[FromForm(Name = "refresh_token")]
		public string? RefreshToken { get; set; }
		[FromForm(Name = "username")]
		public string? Username { get; set; }
		[FromForm(Name = "password")]
		public string? Password { get; set; }
	}
}
