using System.Text.Json.Serialization;

namespace UT4MasterServer.Models.Requests
{
	public class ChangePasswordRequest
	{
		private string username = "";
		private string currentPassword = "";
		private string newPassword = "";

		[JsonPropertyName("username")]
		public string Username { get => username; set => username = value; }

		[JsonPropertyName("currentPassword")]
		public string CurrentPassword { get => currentPassword; set => currentPassword = value; }

		[JsonPropertyName("newPassword")]
		public string NewPassword { get => newPassword; set => newPassword = value; }
	}
}
