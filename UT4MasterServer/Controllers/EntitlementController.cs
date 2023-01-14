using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using UT4MasterServer.Authentication;
using UT4MasterServer.Other;

namespace UT4MasterServer.Controllers;

/// <summary>
/// entitlement-public-service-prod08.ol.epicgames.com
/// </summary>
[ApiController]
[Route("entitlement/api/account/{id}/entitlements")]
[AuthorizeBearer]
[Produces("application/json")]
public class EntitlementController : JsonAPIController
{
	private static readonly List<string> mapEntitlementIDs;

	static EntitlementController()
	{
		// UT entitlement name: UnrealTournament
		// UT entitlement id:   b8538c739273426aa35a98220e258d55

		// the following ids are found in UT4's source code in file UnrealTournament.cpp
		mapEntitlementIDs = new List<string>();
		mapEntitlementIDs.Add("0d5e275ca99d4cf0b03c518a6b279e26"); // DM-Lea
		mapEntitlementIDs.Add("48d281f487154bb29dd75bd7bb95ac8e"); // CTF-Pistola
		mapEntitlementIDs.Add("d8ac8a7ce06d44ab8e6b7284184e556e"); // DM-Batrankus
		mapEntitlementIDs.Add("08af4962353443058766998d6b881707"); // DM-Backspace
		mapEntitlementIDs.Add("27f36270a1ec44509e72687c4ba6845a"); // DM-Salt
		mapEntitlementIDs.Add("a99f379bfb9b41c69ddf0bfbc4a48860"); // CTF-Polaris
		mapEntitlementIDs.Add("65fb5029cddb4de7b5fa155b6992e6a3"); // DM-Unsaved
	}

	public EntitlementController(ILogger<EntitlementController> logger) : base(logger)
	{

	}

	[HttpGet]
	public IActionResult QueryProfile(string id)
	{
		// TODO: Permission: "Sorry your login does not posses the permissions 'entitlement:account:{id_from_param}:entitlements READ' needed to perform the requested operation"

		var commonDate = new DateTime(2023, 1, 1).ToStringISO();

		JArray arr = new JArray();
		foreach (var mapEntitlementID in mapEntitlementIDs)
		{
			JObject obj = new JObject();
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
			arr.Add(obj);
		}

		return Json(arr);
	}
}
