using System.Text.Json.Serialization;

namespace UT4MasterServer.Models.Requests
{
	public class GrantXP
	{
		[JsonPropertyName("xpAmount")]
		public int XPAmount { get; set; }
	}
}
