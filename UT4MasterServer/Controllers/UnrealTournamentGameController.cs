using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
using System.Buffers;
using System.Text;
using UT4MasterServer.Authorization;
using UT4MasterServer.Models;
using UT4MasterServer.Services;

namespace UT4MasterServer.Controllers
{
	[ApiController]
	[Route("ut/api/game/v2")]
	[AuthorizeBearer]
	[Produces("application/json")]
	public class UnrealTournamentGameController : JsonAPIController
	{
		private readonly ILogger<SessionController> logger;
		private readonly AccountService accountService;

		public UnrealTournamentGameController(ILogger<SessionController> logger, AccountService accountService)
		{
			this.logger = logger;
			this.accountService = accountService;

		}

		[HttpPost("profile/{id}/{clientKind}/QueryProfile")]
		public async Task<IActionResult> QueryProfile(string id,
			string clientKind,
			[FromQuery] string profileId,
			[FromQuery] string rvn)
		{
			bool isRequestSentFromClient = clientKind.ToLower() == "client";
			bool isRequestSentFromServer = clientKind.ToLower() == "dedicated_server";


			// i think "rvn" is revision number and it represents index of profile change entry.
			// negative values probably mean index from back-to-front in array.

			var body = await Request.BodyReader.ReadAsStringAsync(1024);
			var jsonBody = JObject.Parse(body);

			// game sends empty json object as body
			if (!(isRequestSentFromClient || isRequestSentFromServer) | profileId != "profile0" || rvn != "-1" || jsonBody != JObject.Parse("{}"))
			{
				logger.LogWarning($"QueryProfile received unexpected data! k:\"{clientKind}\" p:\"{profileId}\" rvn:\"{rvn}\" body:\"{body}\"");
			}

			var account = await accountService.GetAccountAsync(EpicID.FromString(id));
			if (account == null)
				return NotFound();

			// actual response example is in <repo_root>/OldReferenceCode/Server.cs line 750

			int revisionNumber = 3;

			JObject obj = new JObject();
			obj.Add("profileRevision", revisionNumber);
			obj.Add("profileId", profileId);
			obj.Add("profileChangesBaseRevision", revisionNumber);
			JArray profileChanges = new JArray();
			// foreach {
			JObject profileChange = new JObject();
			profileChange.Add("changeType", "fullProfileUpdate");
			JObject profile = new JObject();
			profile.Add("_id", account.ID.ToString());
			profile.Add("created", account.CreatedAt.ToStringISO());
			profile.Add("updated", account.UpdatedAt.ToStringISO());
			profile.Add("rvn", 7152);
			profile.Add("wipeNumber", 4);
			profile.Add("accountId", account.ID.ToString());
			profile.Add("profileId", profileId);
			profile.Add("version", "ut_base");
			JObject items = new JObject();
			// TODO !!!
			profile.Add("items", items);
			JObject stats = new JObject();
			stats.Add("templateId", "profile_v2");
			JObject attributes = new JObject();
			attributes.Add("CountryFlag", account.CountryFlag);
			attributes.Add("GoldStars", account.GoldStars);
			JObject login_rewards = new JObject();
			login_rewards.Add("nextClaimTime", null);
			login_rewards.Add("level", 0);
			login_rewards.Add("totalDays", 0);
			attributes.Add("login_rewards", login_rewards);
			attributes.Add("Avatar", account.Avatar);
			attributes.Add("inventory_limit_bonus", 0);
			attributes.Add("daily_purchases", new JObject());
			attributes.Add("in_app_purchases", new JObject());
			attributes.Add("LastXPTime", account.XPLastMatchAt.ToUnixTimestamp()); // probably unix timestamp at last received xp
			attributes.Add("XP", account.XP);
			attributes.Add("Level", account.LevelStockLimited); // TODO: try values over 50
			attributes.Add("BlueStars", account.BlueStars);
			attributes.Add("RecentXP", account.XPLastMatch); // probably xp from last finished match
			attributes.Add("boosts", new JArray());
			attributes.Add("new_items", new JObject());
			stats.Add("attributes", attributes);
			profile.Add("stats", stats);
			profileChange.Add("profile", profile);
			profileChange.Add("commandRevision", revisionNumber + 1);
			// }
			profileChanges.Add(profileChange);
			obj.Add("profileChanges", profileChanges);
			obj.Add("profileCommandRevision", revisionNumber + 1);
			obj.Add("serverTime", DateTime.UtcNow.ToStringISO());
			obj.Add("responseVersion", 1);
			obj.Add("command", "QueryProfile");

			return Json(obj);
		}

		[HttpGet("profile/{id}/client/SetAvatarAndFlag")]
		public IActionResult SetAvatarAndFlag(string id, [FromQuery] string profileId, [FromQuery] string rvn, [FromBody] string body)
		{
			/* input: 
			{
				"newAvatar": "UT.Avatar.0",
				"newFlag": "Algeria"
			}

			*/
			// response: {"profileRevision":3,"profileId":"profile0","profileChangesBaseRevision":2,"profileChanges":[{"changeType":"statModified","name":"Avatar","value":"UT.Avatar.0"},{"changeType":"statModified","name":"CountryFlag","value":"Algeria"}],"profileCommandRevision":2,"serverTime":"2022-12-20T18:31:46.948Z","responseVersion":1,"command":"SetAvatarAndFlag"}
			return Ok();
		}

		[HttpPost("ratings/account/{id}/mmrbulk")]
		public IActionResult MmrBulk(string id, [FromBody] MMRBulk ratings)
		{
			for (int i = 0; i < ratings.RatingTypes.Count; i++)
			{
				ratings.Ratings.Add(1500);
				ratings.NumGamesPlayed.Add(0);
			}

			return Json(ratings);
		}

		[HttpPost("ratings/account/{id}/mmr/{ratingType}")]
		public IActionResult Mmr(string id, string ratingType, [FromBody] MMRBulk ratings)
		{
			throw new NotImplementedException();

			// TODO: return only one type of rating

			// proper response: {"rating":1844,"numGamesPlayed":182}
			for (int i = 0; i < ratings.RatingTypes.Count; i++)
			{
				ratings.Ratings.Add(1500);
				ratings.NumGamesPlayed.Add(0);
			}

			return Json(ratings);
		}
		[HttpGet("ratings/account/{id}/league/{leagueName}")]
		public IActionResult LeagueRating(string id, string leagueName)
		{
			var league = new League();
			// for now we just send default/empty values
			return Json(league);
		}

		[HttpPost("ratings/team/elo/{ratingType}")]
		public IActionResult JoinQuickplay(string ratingType, [FromBody] string body)
		{
			/*
			INPUT body:

			{
				"members": [
					{
						"accountId": "64bf8c6d81004e88823d577abe157373",
						"score": 0,
						"isBot": false
					}
				],
				"socialPartySize": 1
			}

			*/

			// Response: {"rating":1500}

			return Ok();
		}

		[HttpPost("wait_times/estimate")]
		public IActionResult QuickplayWaitEstimate()
		{
			// Response: [{"ratingType":"DMSkillRating","averageWaitTimeSecs":15.833333333333334,"numSamples":6},{"ratingType":"FlagRunSkillRating","averageWaitTimeSecs":15.0,"numSamples":7}]
			return Ok();
		}

		[HttpPost("wait_times/report/{ratingType}/{unkownNumber}")]
		public IActionResult QuickplayWaitReport()
		{
			return NoContent();
		}

	}
}
