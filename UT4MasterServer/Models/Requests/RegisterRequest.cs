using System.Text.Json.Serialization;

namespace UT4MasterServer.Models.Requests
{
	public class RegisterRequest
	{
		private string username = "";
		private string password = "";
		private string email = "";

		[JsonPropertyName("username")]
		public string Username { get => username; set => username = value; }
		[JsonPropertyName("password")]
		public string Password { get => password; set => password = value; }
		[JsonPropertyName("email")]
		public string Email { get => email; set => email = value; }
	}
}
