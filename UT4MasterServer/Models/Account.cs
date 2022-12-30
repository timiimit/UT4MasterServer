using MongoDB.Bson.Serialization.Attributes;

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

	[BsonElement("CreatedAt")]
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	[BsonElement("LastLoginAt")]
	public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;

	/**** Not required for proper operation ****/

	[BsonIgnoreIfNull]
	[BsonElement("Email")]
	public string? Email { get; set; } = null;

	[BsonIgnoreIfDefault]
	[BsonDefaultValue(new string[0])]
	[BsonElement("DeviceIDs")]
	public string[] DeviceIDs { get; set; } = Array.Empty<string>();

	/************** Game Specific **************/

	[BsonIgnoreIfDefault]
	[BsonDefaultValue("Unreal")]
	[BsonElement("CountryFlag")]
	public string CountryFlag { get; set; } = "Unreal";

	[BsonIgnoreIfDefault]
	[BsonDefaultValue("UT.Avatar.0")]
	[BsonElement("Avatar")]
	public string Avatar { get; set; } = "UT.Avatar.0";

	[BsonIgnoreIfDefault]
	[BsonDefaultValue(0)]
	[BsonElement("GoldStars")]
	public int GoldStars { get; set; } = 0;

	[BsonIgnoreIfDefault]
	[BsonDefaultValue(0)]
	[BsonElement("BlueStars")]
	public int BlueStars { get; set; } = 0;

	[BsonIgnoreIfDefault]
	[BsonDefaultValue(0)]
	[BsonElement("XP")]
	public int XP { get; set; } = 0;

	[BsonIgnoreIfDefault]
	[BsonDefaultValue(0)]
	[BsonElement("XPLastMatch")]
	public int XPLastMatch { get; set; } = 0;

	//[BsonElement("XPLastMatchAt")]
	//public DateTime XPLastMatchAt { get; set; } = DateTimeExtension.UnixTimestampStartOfTime;


	[BsonIgnore]
	public float Level
	{
		get
		{
			// calculation for levels over 50 from UT4UU - port from c++ to c# is untested
			// find required xp per certain level here: https://docs.google.com/spreadsheets/d/1gvoxW2UMk8_O1E1emObkQNy1kzPOQ1Wmu0YvslMAwyE

			ulong xp_in = (ulong)XP;
			if (xp_in < 50)
				return 1;
			if (xp_in < 150)
				return 2;
			// note: req to next level, so element 0 is XP required for level 1
			ulong xp = 0;
			ulong Increment = 50;
			ulong Step = 50;
			xp = Step;

			ulong incrementBoost = 0;
			ulong incrementBoostLevelsStep = 10;
			ulong incrementBoostLevelChange = 10;

			ulong level = 3;
			while (xp <= xp_in)
			{
				Increment += incrementBoost;
				if (level == incrementBoostLevelChange)
				{
					incrementBoost += 5;
					incrementBoostLevelsStep += 10;
					incrementBoostLevelChange += incrementBoostLevelsStep;
				}
				Step += Increment;
				xp += Step;
				level++;
			}
			return level - 2;
		}
	}

	[BsonIgnore]
	public int LevelStockLimited => Math.Min(50, (int)Level);



	public override string ToString()
	{
		// just some way of representing account as a string
		return $"[{ID}] {Username}";
	}

}
