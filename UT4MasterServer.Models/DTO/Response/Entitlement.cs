using System.Text.Json.Serialization;
using UT4MasterServer.Common;

namespace UT4MasterServer.Models.DTO.Responses;

public class Entitlement
{
	/*

				obj.Add("id", EpicID.GenerateNew().ToString()); // does not matter to us
			obj.Add("entitlementName", mapEntitlementID); // usually same as map entitlement id
			obj.Add("namespace", "ut");
			obj.Add("catalogItemId", mapEntitlementID); // this is what is checked in the game
			obj.Add("accountId", id);
			obj.Add("identityId", id);
			obj.Add("entitlementType", "EXECUTABLE"); // EXECUTABLE is the only known value
			obj.Add("grantDate", commonDate);
			obj.Add("consumable", false);
			obj.Add("status", "ACTIVE"); // ACTIVE is the only known value
			obj.Add("active", true);
			obj.Add("useCount", 0);
			obj.Add("originalUseCount", 0);
			obj.Add("platformType", "EPIC");
			obj.Add("created", commonDate);
			obj.Add("updated", commonDate);
			obj.Add("groupEntitlement", false);
			obj.Add("country", "US"); // does not matter to us
	 
	 */

	[JsonPropertyName("id")]
	public EpicID ID { get; set; } = EpicID.GenerateNew();

	[JsonPropertyName("entitlementName")]
	public string EntitlementName { get; set; }

	[JsonPropertyName("namespace")]
	public string Namespace { get; set; } = "ut";

	[JsonPropertyName("catalogItemId")]
	public string CatalogItemID { get; set; }

	[JsonPropertyName("accountId")]
	public EpicID AccountID { get; set; }

	[JsonPropertyName("identityId")]
	public EpicID IdentityID { get; set; }

	[JsonPropertyName("grantDate")]
	public DateTime GrantDate { get; set; }

	[JsonPropertyName("consumable")]
	public bool Consumable { get; set; } = false;

	[JsonPropertyName("status")]
	public string Status { get; set; } = "ACTIVE";

	[JsonPropertyName("active")]
	public bool Active { get; set; } = true;

	[JsonPropertyName("useCount")]
	public int UseCount { get; set; } = 0;

	[JsonPropertyName("originalUseCount")]
	public int OriginalUseCount { get; set; } = 0;

	[JsonPropertyName("platformType")]
	public string PlatformType { get; set; } = "EPIC";

	[JsonPropertyName("created")]
	public DateTime Created { get; set; }

	[JsonPropertyName("updated")]
	public DateTime Updated { get; set; }

	[JsonPropertyName("groupEntitlement")]
	public bool GroupEntitlement { get; set; } = false;

	[JsonPropertyName("country")]
	public string Country { get; set; } = "US";

	public Entitlement(string name, string id, EpicID accountID)
	{
		var commonDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		EntitlementName = name;
		CatalogItemID = id;
		AccountID = accountID;
		IdentityID = accountID;
		GrantDate = commonDate;
		Created = commonDate;
		Updated = commonDate;
	}
}
