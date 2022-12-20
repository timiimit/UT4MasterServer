
using Microsoft.AspNetCore.Mvc;
using UT4MasterServer.Authorization;

namespace UT4MasterServer.Controllers
{
    [ApiController]
    [Route("entitlement/api/account/{id}/entitlements")]
    [AuthorizeBearer]
    [Produces("application/json")]
    public class EntitlementController : JsonAPIController
	{
        private readonly ILogger<SessionController> logger;

        public EntitlementController(ILogger<SessionController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        public ActionResult<IActionResult> QueryProfile(string id)
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

            */
			return Json("[]");
        }
    }
}
