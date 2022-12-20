using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using UT4MasterServer.Authorization;
using UT4MasterServer.Models;
using UT4MasterServer.Services;

namespace UT4MasterServer.Controllers
{
    [ApiController]
    [Route("ut/api/game/v2")]
    [AuthorizeBearer]
    [Produces("application/json")]
    public class UnrealTournamentController : JsonAPIController
	{
        private readonly ILogger<SessionController> logger;
		private readonly AccountService accountService;

		public UnrealTournamentController(ILogger<SessionController> logger, AccountService accountService)
        {
            this.logger = logger;
			this.accountService = accountService;

		}

        [HttpPost]
        [Route("profile/{id}/client/QueryProfile")]
        public async Task<IActionResult> QueryProfile(string id, [FromQuery] string profileId, [FromQuery] string rvn, [FromBody] string body)
        {
            // game sends empty json object as body
            if (profileId != "profile0" || rvn != "-1" || body != "{}")
            {
                logger.LogWarning($"QueryProfile received unexpected data! p:\"{profileId}\" rvn:\"{rvn}\" body:\"{body}\"");
            }

			var account = await accountService.GetAccountAsync(EpicID.FromString(id));
			if (account == null)
				return NotFound();

			// actual response example is in <repo_root>/UT4MasterServer/Server.cs line 750

			JObject obj = new JObject();
			obj.Add("profileRevision", 7152);
			obj.Add("profileId", profileId);
			obj.Add("profileChangesBaseRevision", 7152);
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
			stats.Add("CountryFlag", account.CountryFlag);
			stats.Add("GoldStars", account.GoldStars);
			JObject login_rewards = new JObject();
			login_rewards.Add("nextClaimTime", null);
			login_rewards.Add("level", 0);
			login_rewards.Add("totalDays", 0);
			stats.Add("login_rewards", login_rewards);
			stats.Add("Avatar", account.Avatar);
			stats.Add("inventory_limit_bonus", 0);
			stats.Add("daily_purchases", new JObject());
			stats.Add("in_app_purchases", new JObject());
			stats.Add("LastXPTime", account.XPLastMatchAt.ToUnixTimestamp()); // probably unix timestamp at last received xp
			stats.Add("XP", account.XP);
			stats.Add("Level", account.LevelStockLimited); // TODO: try values over 50
			stats.Add("BlueStars", account.BlueStars);
			stats.Add("RecentXP", account.XPLastMatch); // probably xp from last finished match
			stats.Add("boosts", new JArray());
			stats.Add("new_items", new JObject());
			profile.Add("stats", stats);
			profileChange.Add("profile", profile);
			profileChange.Add("commandRevision", 7043);
			// }
			profileChanges.Add(profileChange);
			obj.Add("profileChanges", profileChanges);
			obj.Add("profileCommandRevision", 7043);
			obj.Add("serverTime", DateTime.UtcNow.ToStringISO());
			obj.Add("responseVersion", 1);
			obj.Add("command", "QueryProfile");

			return Json(obj);
        }

        [HttpPost]
        [Route("ratings/account/{id}/mmrbulk")]
        public IActionResult MmrBulk(string id, [FromBody] MMRBulk ratings)
        {
            for (int i = 0; i < ratings.RatingTypes.Count; i++)
            {
                ratings.Ratings.Add(1500);
                ratings.PlayCount.Add(0);
            }
			
			return Json(ratings);
        }

        [HttpGet]
        [Route("ratings/account/{id}/league/{leagueName}")]
        public IActionResult LeagueRating(string id, string leagueName)
        {
            var league = new League();
            // for now we just send default/empty values
            return Json(league);
        }
    }
}
