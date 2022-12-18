
using Newtonsoft.Json;

namespace UT4MasterServer.Models
{
	public class ErrorResponse
	{
		[JsonProperty("errorCode")]
		public string ErrorCode { get; set; } // errors.com.epicgames.account.oauth.authorization_code_not_found

		[JsonProperty("errorMessage")]
		public string ErrorMessage { get; set; } // Sorry the authorization code you supplied was not found. It is possible that it was no longer valid

		[JsonProperty("messageVars")]
		public string[] MessageVars { get; set; } // unknown type of objects in array

		[JsonProperty("numericErrorCode")]
		public int NumericErrorCode { get; set; } // 18059

		[JsonProperty("originatingService")]
		public string OriginatingService { get; set; } // com.epicgames.account.public

		[JsonProperty("intent")]
		public string Intent { get; set; } // prod

		[JsonProperty("error_description")]
		public string ErrorDescription { get; set; } // Sorry the authorization code you supplied was not found. It is possible that it was no longer valid

		[JsonProperty("error")]
		public string Error { get; set; } // invalid_grant
	}
}