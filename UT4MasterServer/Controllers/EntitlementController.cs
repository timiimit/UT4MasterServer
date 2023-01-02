using Microsoft.AspNetCore.Mvc;
using UT4MasterServer.Authentication;

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
	[HttpGet]
	public IActionResult QueryProfile(string id)
	{
		// TODO: we should at least return entitlement for those 3 or so community made maps
		//       that are in the game and need to normally be redeemed in store. it is important
		//       that we find the id's of those items before its too late.

		/*
		Here is one for DM-Unsaved, the only one i own.

		[{
			"id": "87e81495602a42bfb8f11066886f7276",
			"entitlementName": "Unsaved",
			"namespace": "ut",
			"catalogItemId": "65fb5029cddb4de7b5fa155b6992e6a3",
			"accountId": "0b0f09b400854b9b98932dd9e5abe7c5",
			"identityId": "0b0f09b400854b9b98932dd9e5abe7c5",
			"entitlementType": "EXECUTABLE",
			"grantDate": "2017-10-12T16:20:24.736Z",
			"startDate": "2016-11-03T00:00:00.000Z",
			"consumable": false,
			"status": "ACTIVE",
			"active": true,
			"useCount": 0,
			"created": "2017-10-12T16:20:24.740Z",
			"updated": "2017-10-12T16:20:24.740Z",
			"groupEntitlement": false
			}]

		[
			{
				"id": "47573a6c0df04f81908e652ffcab5f26",
				"entitlementName": "UnrealTournament",
				"namespace": "ut",
				"catalogItemId": "b8538c739273426aa35a98220e258d55",
				"accountId": "64bf8c6d81004e88823d577abe157373",
				"identityId": "64bf8c6d81004e88823d577abe157373",
				"entitlementType": "EXECUTABLE",
				"grantDate": "2020-06-02T13:21:18.729Z",
				"consumable": false,
				"status": "ACTIVE",
				"active": true,
				"useCount": 0,
				"originalUseCount": 0,
				"platformType": "EPIC",
				"created": "2020-06-02T13:21:18.732Z",
				"updated": "2020-06-02T13:21:18.732Z",
				"groupEntitlement": false,
				"country": "SI"
			}
		]
		*/

		// TODO: Permission: "Sorry your login does not posses the permissions 'entitlement:account:{id_from_param}:entitlements READ' needed to perform the requested operation"

		return Json("[]");
	}
}
