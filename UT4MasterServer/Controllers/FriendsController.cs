
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
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
		private readonly FriendService friendService;

		public FriendsController(FriendService friendService, ILogger<SessionController> logger)
		{
			this.logger = logger;
			this.friendService = friendService;
		}

		[HttpGet("friends/{id}")]
		public async Task<IActionResult> GetFriends(string id, [FromQuery] bool? includePending)
		{
			if (User.Identity is not EpicUserIdentity authenticatedUser)
				return Unauthorized();

			var eid = EpicID.FromString(id);

			// idk if epic allows anyone to view this, but we wont
			if (eid != authenticatedUser.Session.AccountID)
				return Json("[]", StatusCodes.Status401Unauthorized);

			var friends = await friendService.GetFriendsAsync(eid);

			JArray arr = new JArray();
			foreach (var friend in friends)
			{
				var other = friend.Sender == eid ? friend.Receiver : friend.Sender;
				var status = friend.Status == Models.FriendStatus.Active ? "ACTIVE" : "PENDING";
				var direction = friend.Sender == eid ? "OUTBOUND" : "INBOUND";

				JObject obj = new JObject();
				obj.Add("accountId", other.ToString());
				obj.Add("status", status);
				obj.Add("direction", direction);
				obj.Add("created", DateTime.UtcNow.ToStringISO()); // should we care?
				obj.Add("favourite", false); // TODO: figure out if it's possible to set to true normally
				arr.Add(obj);
			}

			return Json(arr);
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
			
			var blockedUsers = await friendService.GetBlockedUsersAsync(eid);

			JArray arr = new JArray();
			foreach (var blockedUser in blockedUsers)
			{
				arr.Add(blockedUser.Receiver.ToString());
			}
			return Json(arr);
		}


	}
}
