using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace UT4MasterServer.Models;

public class Account
{
	//        {
	//	"id": "fd83abe496ca401497b5adf4e412bf2c",
	//	"displayName": "dc!",
	//	"name": "dc",
	//	"email": "email@email.email",
	//	"affiliationType": "Programmer",
	//	"failedLoginAttempts": 0,
	//	"lastLogin": "2022-12-14T23:39:48.417Z",
	//	"numberOfDisplayNameChanges": 0,
	//	"ageGroup": "UNKNOWN",
	//	"headless": false,
	//	"country": "CA",
	//	"lastName": "dc",
	//	"phoneNumber": "123",
	//	"preferredLanguage": "en",
	//	"canUpdateDisplayName": true,
	//	"tfaEnabled": true,
	//	"emailVerified": true,
	//	"minorVerified": false,
	//	"minorExpected": false,
	//	"minorStatus": "NOT_MINOR",
	//	"cabinedMode": false,
	//	"hasHashedEmail": false
	//}

	// TODO: Figure out what fields ^^^^ are actually needed in this model.

	[BsonId]
	public EpicID ID { get; set; } = EpicID.Empty;

	[BsonElement("Username")]
	public string Username { get; set; } = string.Empty;

	[BsonElement("Password")]
	public string Password { get; set; } = string.Empty;

	[BsonElement("LastLogin")]
	public DateTime LastLogin { get; set; } = DateTime.UtcNow;

	[BsonElement("XP")]
	public int XP { get; set; } = 0;

	[BsonElement("Friends")]
	public List<EpicID> Friends { get; set; } = new List<EpicID>();

	[BsonElement("BlockedUsers")]
	public List<EpicID> BlockedUsers { get; set; } = new List<EpicID>();



	public override string ToString()
	{
		// just some way of representing account as a string
		return $"[{ID}] {Username}";
	}

}
