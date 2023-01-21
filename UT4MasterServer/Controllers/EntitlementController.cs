using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using UT4MasterServer.Authentication;
using UT4MasterServer.Models.Requests;
using UT4MasterServer.Other;

namespace UT4MasterServer.Controllers;

/// <summary>
/// entitlement-public-service-prod08.ol.epicgames.com
/// </summary>
[ApiController]
[Route("entitlement/api/account/{id}/entitlements")]
//[AuthorizeBearer]
[Produces("application/json")]
public class EntitlementController : JsonAPIController
{
	private static readonly List<string> mapEntitlementIDs;
	private static readonly List<string> cosmeticEntitlementIDs;
	private const string UTEntitlementID = "b8538c739273426aa35a98220e258d55";

	static EntitlementController()
	{
		// the following ids are found in UT4's source code in file UnrealTournament.cpp
		mapEntitlementIDs = new List<string>();
		mapEntitlementIDs.Add("0d5e275ca99d4cf0b03c518a6b279e26"); // DM-Lea
		mapEntitlementIDs.Add("48d281f487154bb29dd75bd7bb95ac8e"); // CTF-Pistola
		mapEntitlementIDs.Add("d8ac8a7ce06d44ab8e6b7284184e556e"); // DM-Batrankus
		mapEntitlementIDs.Add("08af4962353443058766998d6b881707"); // DM-Backspace
		mapEntitlementIDs.Add("27f36270a1ec44509e72687c4ba6845a"); // DM-Salt
		mapEntitlementIDs.Add("a99f379bfb9b41c69ddf0bfbc4a48860"); // CTF-Polaris
		mapEntitlementIDs.Add("65fb5029cddb4de7b5fa155b6992e6a3"); // DM-Unsaved

		// list of these is also partially in UnrealTournament.cpp,
		// but this was retrieved by looping over all cosmetics and calling
		// GetRequiredEntitlementFromAsset.
		cosmeticEntitlementIDs = new List<string>();
		cosmeticEntitlementIDs.Add("a18ab3a3eb6644b7842750fc7613ec01"); // TC_ArmorNewV
		cosmeticEntitlementIDs.Add("606862e8a0ec4f5190f67c6df9d4ea81"); // BP_SkullHornsMask & BP_SkullMask
		cosmeticEntitlementIDs.Add("91afa66fbf744726af33dba391657296"); // BP_Round_HelmetLeader & BP_Round_HelmetGoggles
		cosmeticEntitlementIDs.Add("9a1ad6c3c10e438f9602c14ad1b67bfa"); // BP_CardboardHat_Leader & BP_CardboardHat
		cosmeticEntitlementIDs.Add("8747335f79dd4bec8ddc03214c307950"); // BP_BaseballHat_Leader & BP_BaseballHat
		//cosmeticEntitlementIDS.Add("527E7E209F4142F8835BA696919E2BEC"); // BP_Char_Oct2015, broken character that we don't want people to have

		// TODO: find a way to include halloween cosmetics, they seem to be handled differently in the game
	}

	public EntitlementController(ILogger<EntitlementController> logger) : base(logger)
	{

	}

	[HttpGet]
	public IActionResult ListEntitlements(string id)
	{
		// TODO: Permission: "Sorry your login does not posses the permissions 'entitlement:account:{id_from_param}:entitlements READ' needed to perform the requested operation"

		EpicID eid = EpicID.FromString(id);

		List<Entitlement> entitlements = new List<Entitlement>();
		entitlements.Add(new Entitlement("UnrealTournament", UTEntitlementID, eid));
		foreach (var entitlementID in mapEntitlementIDs)
		{
			entitlements.Add(new Entitlement(entitlementID, entitlementID, eid));
		}
		// TODO: decide how players unlock these special cosmetics
		//foreach (var entitlementID in cosmeticEntitlementIDs)
		//{
		//	entitlements.Add(new Entitlement(entitlementID, entitlementID, eid));
		//}

		return new JsonResult(entitlements);
	}
}
