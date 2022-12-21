
using Microsoft.AspNetCore.Mvc;
using UT4MasterServer.Authorization;
using UT4MasterServer.Services;

namespace UT4MasterServer.Controllers
{
	[ApiController]
	[Route("friends/api/public")]
	[AuthorizeBearer]
	[Produces("application/json")]
	public class FriendsController : JsonAPIController
	{
		private readonly ILogger<SessionController> logger;
		private readonly AccountService accountService;

		public FriendsController(AccountService accountService, ILogger<SessionController> logger)
		{
			this.logger = logger;
			this.accountService = accountService;
		}

		[HttpGet("friends/{id}")]
		public IActionResult GetFriends(string id, [FromQuery] bool? includePending)
		{
			if (User.Identity is not EpicUserIdentity authenticatedUser)
				return Unauthorized();

			var eid = EpicID.FromString(id);

			// idk if epic allows anyone to view this, but we wont
			if (eid != authenticatedUser.Session.AccountID)
				return Json("[]", StatusCodes.Status401Unauthorized);

			// TODO
			// [{"accountId":"0b0f09b400854b9b98932dd9e5abe7c5","status":"PENDING",
			// "direction":"INBOUND","created":"2022-10-16T15:14:32.356Z","favorite":false}]
			return Json("[]");
		}

		[HttpGet("blocklist/{id}")]
		public async Task<IActionResult> GetBlockedAccounts(string id)
		{
			if (User.Identity is not EpicUserIdentity authenticatedUser)
				return Json("[]", StatusCodes.Status401Unauthorized);

			var eid = EpicID.FromString(id);

			// idk if epic allows anyone to view this, but we wont
			if (eid != authenticatedUser.Session.AccountID)
				return Json("[]", StatusCodes.Status401Unauthorized);
			
			var account = await accountService.GetAccountAsync(eid);
			if (account == null)
				return Json("[]", StatusCodes.Status500InternalServerError); // this should never happen since session with this account exists, internal error

			// TODO: make sure this sends expected json array of strings
			return Json(account.BlockedUsers);
		}


	}
}
