
using Newtonsoft.Json;
using System.Web.Http;

namespace UT4MasterServer.Models
{
	public class ErrorResponse : IHttpActionResult
	{
		[JsonProperty("errorCode")]
		public string ErrorCode { get; set; }

		[JsonProperty("errorMessage")]
		public string ErrorMessage { get; set; }

		[JsonProperty("messageVars")]
		public string[] MessageVars { get; set; } // any value inside errorMessage is listed in this array

		[JsonProperty("numericErrorCode")]
		public int NumericErrorCode { get; set; }

		[JsonProperty("originatingService")]
		public string OriginatingService { get; set; }

		[JsonProperty("intent")]
		public string Intent { get; set; }

		[JsonProperty("error_description")]
		public string ErrorDescription { get; set; }

		[JsonProperty("error")]
		public string Error { get; set; }

		public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
		{
			return new Task<HttpResponseMessage>(() => new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest));
		}



		/*
		some examples:
		{"errorCode":"errors.com.epicgames.account.auth_token.invalid_refresh_token","errorMessage":"Sorry the refresh token '971A38DCCBBA60E51F4AB04A09BE7B3F3D9D983C8814D279CF41321C8D3906B5' is invalid","messageVars":["971A38DCCBBA60E51F4AB04A09BE7B3F3D9D983C8814D279CF41321C8D3906B5"],"numericErrorCode":18036,"originatingService":"com.epicgames.account.public","intent":"prod","error_description":"Sorry the refresh token '971A38DCCBBA60E51F4AB04A09BE7B3F3D9D983C8814D279CF41321C8D3906B5' is invalid","error":"invalid_grant"}

		*/
	}
}