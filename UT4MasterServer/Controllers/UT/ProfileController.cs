using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using UT4MasterServer.Authentication;
using UT4MasterServer.Controllers.Epic;
using UT4MasterServer.Models.DTO.Requests;
using UT4MasterServer.Common;
using UT4MasterServer.Services.Scoped;
using UT4MasterServer.Common.Helpers;
using UT4MasterServer.Models.Database;

namespace UT4MasterServer.Controllers.UT;

/// <summary>
/// ut-public-service-prod10.ol.epicgames.com
/// </summary>
[ApiController]
[Route("ut/api/game/v2/profile")]
[AuthorizeBearer]
[Produces("application/json")]
public sealed class ProfileController : JsonAPIController
{
	private static readonly List<(string item, int requiredLevel)> profileItems;

	static ProfileController()
	{
		profileItems = new()
		{
			("BeanieBlack", 2),
			("Sunglasses", 3),
			("HockeyMask", 4),
			("ThundercrashMale05", 5),
			("NecrisMale01", 7),
			("ThundercrashMale03", 8),
			("NecrisHelm01", 10),
			("ThundercrashBeanieGreen", 12),
			("HockeyMask02", 14),
			("ThundercrashMale02", 17),
			("NecrisFemale02", 20),
			("BeanieWhite", 23),
			("NecrisHelm02", 26),
			("SkaarjMale01", 30),
			("BeanieGrey", 34),
			("ThundercrashBeanieRed", 39),
			("SkaarjMale02", 40),
			("ThundercrashBeret", 45),
			("NecrisMale04", 50),

			// TODO: figure out how profile items below should be unlocked
			//("ThundercrashSunglasses", 0),
			//("ThundercrashMale01", 0),
			//("ThundercrashBeanieWhite", 0),
			//("PhayderStealthHelm", 0),
			//("NanoblackHelmGreen", 0),
			//("NanoblackHelmBlack", 0),
			//("Infiltrator", 0),
			//("EnergyHelm", 0),
			//("EliteAssassinHelm", 0),
		};
	}

	private readonly AccountService accountService;
	private readonly MatchmakingService matchmakingService;

	public ProfileController(ILogger<SessionController> logger, AccountService accountService, MatchmakingService matchmakingService) : base(logger)
	{
		this.accountService = accountService;
		this.matchmakingService = matchmakingService;
	}

	[HttpPost("{id}/{clientKind}/QueryProfile")]
	public async Task<IActionResult> QueryProfile(string id,
		string clientKind,
		[FromQuery] string profileId,
		[FromQuery] int rvn)
	{
		if (User.Identity is not EpicUserIdentity user)
		{
			return Unauthorized();
		}

		var isRequestSentFromClient = clientKind.ToLower() == "client";
		var isRequestSentFromServer = clientKind.ToLower() == "dedicated_server";

		// TODO: I think "rvn" is revision number and it represents index of profile change entry.
		// negative values probably mean index from back-to-front in array.

		var body = await Request.Body.ReadAsStringAsync(1024);
		var jsonBody = JObject.Parse(body);

		if (rvn == -1)
		{
			rvn = 1;
		}

		// game sends empty json object as body
		if (!(isRequestSentFromClient || isRequestSentFromServer) | profileId != "profile0" || !JToken.DeepEquals(jsonBody, JObject.Parse("{}")))
		{
			logger.LogWarning($"QueryProfile received unexpected data! k:\"{clientKind}\" p:\"{profileId}\" rvn:\"{rvn}\" body:\"{body}\"");
		}

		Account? account = await accountService.GetAccountAsync(EpicID.FromString(id));
		if (account == null)
		{
			return NotFound();
		}

		// actual response example is in <repo_root>/OldReferenceCode/Server.cs line 750

		var revisionNumber = rvn + 1;
		var commandRevision = rvn - 1;

		JObject obj = new();
		obj.Add("profileRevision", revisionNumber);
		obj.Add("profileId", profileId);
		obj.Add("profileChangesBaseRevision", revisionNumber);
		var profileChanges = new JArray();
		// foreach {
		JObject profileChange = new();
		profileChange.Add("changeType", "fullProfileUpdate");
		JObject profile = new();
		{
			profile.Add("_id", account.ID.ToString());
			profile.Add("created", account.CreatedAt.ToStringISO());
			profile.Add("updated", (account.LastLoginAt - TimeSpan.FromSeconds(10)).ToStringISO()); // we don't store this info, send an arbitrary one
			profile.Add("rvn", revisionNumber);
			profile.Add("wipeNumber", 0);
			profile.Add("accountId", account.ID.ToString());
			profile.Add("commandRevision", commandRevision);
			profile.Add("profileId", profileId);
			profile.Add("version", "ut_base");
		}
		JObject items = new();
		foreach ((string item, int requiredLevel) profileItem in profileItems)
		{
#if (!DEBUG) || (DEBUG && true)
			if (account.Level < profileItem.requiredLevel)
			{
				continue;
			}
#endif

			// guid probably represents the id of profile item
			// we don't really store obtained items, so we generate new id
			// each time
			var profileItemGuid = Guid.NewGuid().ToString();
			items.Add(profileItemGuid, new JObject()
			{
				{ "templateId", "Item." + profileItem.item },
				{ "attributes", new JObject()
					{
						{ "tradable", false }
					}
				},
				{ "quantity", 1 }
			});
		}
		profile.Add("items", items);
		JObject stats = new();
		{
			stats.Add("templateId", "profile_v2");
			JObject attributes = new();
			attributes.Add("CountryFlag", account.CountryFlag);
			attributes.Add("GoldStars", account.GoldStars);
			JObject loginRewards = new();
			loginRewards.Add("nextClaimTime", null);
			loginRewards.Add("level", 0);
			loginRewards.Add("totalDays", 0);
			attributes.Add("login_rewards", loginRewards);
			attributes.Add("Avatar", account.Avatar);
			attributes.Add("inventory_limit_bonus", 0);
			attributes.Add("daily_purchases", new JObject());
			attributes.Add("in_app_purchases", new JObject());
			attributes.Add("LastXPTime", (account.LastLoginAt - TimeSpan.FromSeconds(10)).ToUnixTimestamp()); // we don't store this info, send an arbitrary one
			attributes.Add("XP", account.XP);
			attributes.Add("Level", account.LevelStockLimited); // TODO: try values over 50
			attributes.Add("BlueStars", account.BlueStars);
			attributes.Add("RecentXP", 0);//account.XPLastMatch); // probably xp from last finished match
			attributes.Add("boosts", new JArray());
			attributes.Add("new_items", new JObject());
			stats.Add("attributes", attributes);
		}
		profile.Add("stats", stats);
		profileChange.Add("profile", profile);
		// }
		profileChanges.Add(profileChange);
		obj.Add("profileChanges", profileChanges);
		obj.Add("profileCommandRevision", commandRevision);
		obj.Add("serverTime", DateTime.UtcNow.ToStringISO());
		obj.Add("responseVersion", 1);
		obj.Add("command", "QueryProfile");

		return Json(obj);
	}

	[HttpPost("{id}/{clientKind}/SetAvatarAndFlag")]
	public async Task<IActionResult> SetAvatarAndFlag(string id, string clientKind, [FromQuery] string profileId, [FromQuery] int rvn)
	{
		if (User.Identity is not EpicUserIdentity user)
		{
			return Unauthorized();
		}

		if (user.Session.AccountID != EpicID.FromString(id))
		{
			return Unauthorized();
		}

		// TODO: Permission: "Sorry your login does not posses the permissions 'ut:profile:{id_from_params}:commands ALL' needed to perform the requested operation"

		if (rvn == -1)
		{
			rvn = 1;
		}

		var revisionNumber = rvn;

		var obj = JObject.Parse(await Request.Body.ReadAsStringAsync(1024));
		var avatar = obj["newAvatar"]?.ToObject<string>();
		var flag = obj["newFlag"]?.ToObject<string>();

		obj = new JObject();
		obj.Add("profileRevision", revisionNumber);
		obj.Add("profileId", "profile0");
		obj.Add("profileChangesBaseRevision", revisionNumber - 1);
		var profileChanges = new JArray();
		if (avatar != null || flag != null)
		{
			Account? acc = await accountService.GetAccountAsync(user.Session.AccountID);
			if (acc == null)
			{
				logger.LogError("Account is null");
				return StatusCode(StatusCodes.Status500InternalServerError); // should never happen
			}

			if (avatar != null)
			{
				acc.Avatar = avatar;
				JObject profileChange = new()
				{
					{ "changeType", "statModified" },
					{ "name", "Avatar" },
					{ "value", acc.Avatar }
				};
				profileChanges.Add(profileChange);
			}

			if (flag != null)
			{
				acc.CountryFlag = flag;
				JObject profileChange = new()
				{
					{ "changeType", "statModified" },
					{ "name", "CountryFlag" },
					{ "value", acc.CountryFlag }
				};
				profileChanges.Add(profileChange);
			}

			await accountService.UpdateAccountAsync(acc);
		}
		obj.Add("profileChanges", profileChanges);
		obj.Add("profileCommandRevision", revisionNumber - 1);
		obj.Add("serverTime", DateTime.UtcNow.ToStringISO());
		obj.Add("responseVersion", 1);
		obj.Add("command", "SetAvatarAndFlag");

		return Json(obj);
	}

	[HttpPost("{id}/{clientKind}/GrantXP")]
	public async Task<IActionResult> GrantXP(string id, string clientKind, [FromQuery] string profileId, [FromQuery] int rvn, [FromBody] GrantXPRequest body)
	{
		if (User.Identity is not EpicUserIdentity user)
		{
			return Unauthorized();
		}

		var eid = EpicID.FromString(id);

		// only known to be sent by dedicated_server so far
		var isRequestSentFromClient = clientKind.ToLower() == "client";
		var isRequestSentFromServer = clientKind.ToLower() == "dedicated_server";

		if (isRequestSentFromServer && user.Session.AccountID.IsEmpty)
		{
			// it is "okay" to let any server handle anyone's XP since we have a limit on how much xp/h one can earn
		}
		else if (isRequestSentFromClient && user.Session.AccountID == eid)
		{
			// it is "okay" to let client modify his own XP since we have a limit on how much xp/h one can earn
		}
		else
		{
			return Unauthorized();
		}

		Account? acc = await accountService.GetAccountAsync(eid);
		if (acc == null)
		{
			return NotFound();
		}

		const double maxXPPerHour = 500.0;
		var hoursSinceLastMatch = (DateTime.UtcNow - acc.LastMatchAt).TotalHours;

		// this is just some hard limit on max xp allowed per request/match
		if (body.XPAmount > 300)
		{
			logger.LogWarning("{User} supposedly earned {XP} XP in a single match.", acc.ToString(), body.XPAmount);
			body.XPAmount = 300;
		}

		var maxEarnableXP = maxXPPerHour * hoursSinceLastMatch;
		if (body.XPAmount > maxEarnableXP)
		{
			logger.LogWarning("{User} supposedly earned {XP} XP in {Hours} hours. Limiting to {AdjustedXP} XP which is max allowed XP within this timespan.",
				acc.ToString(), body.XPAmount, hoursSinceLastMatch, maxEarnableXP);
			body.XPAmount = (int)maxEarnableXP;
		}

		var prevXP = acc.XP;
		var prevLevel = acc.LevelStockLimited;

		if (rvn == -1)
		{
			rvn = 1;
		}

		var revisionNumber = rvn;

		var obj = new JObject();
		obj.Add("profileRevision", revisionNumber);
		obj.Add("profileId", "profile0");
		obj.Add("profileChangesBaseRevision", revisionNumber - 1);
		var profileChanges = new JArray();
		{
			acc.LastMatchAt = DateTime.UtcNow;
			profileChanges.Add(new JObject()
			{
				{ "changeType", "statModified" },
				{ "name", "LastXPTime" },
				{ "value", acc.LastMatchAt.ToUnixTimestamp() }
			});

			//acc.XPLastMatch = body.XPAmount;
			profileChanges.Add(new JObject()
			{
				{ "changeType", "statModified" },
				{ "name", "RecentXP" },
				{ "value", 0 } //acc.XPLastMatch }
			});

			acc.XP += body.XPAmount;
			profileChanges.Add(new JObject()
			{
				{ "changeType", "statModified" },
				{ "name", "XP" },
				{ "value", acc.XP }
			});

			profileChanges.Add(new JObject()
			{
				{ "changeType", "statModified" },
				{ "name", "Level" },
				{ "value", acc.LevelStockLimited }
			});
		}
		obj.Add("profileChanges", profileChanges);

		obj.Add("notifications", new JArray()
		{
			{
				new JObject()
				{
					{ "type", "XPProgress" },
					{ "primary", false },
					{ "prevXP", prevXP },
					{ "XP", acc.XP },
					{ "prevLevel", prevLevel },
					{ "level", acc.LevelStockLimited }
				}
			}
		});

		obj.Add("profileCommandRevision", revisionNumber - 1);
		obj.Add("serverTime", DateTime.UtcNow.ToStringISO());
		obj.Add("responseVersion", 1);
		obj.Add("command", "GrantXP");

		await accountService.UpdateAccountAsync(acc);

		return Json(obj);
	}

	[HttpPost("{id}/{clientKind}/SetStars")]
	public async Task<IActionResult> SetStars(string id, string clientKind, [FromQuery] string profileId, [FromQuery] string rvn, [FromBody] SetStarsRequest body)
	{
		if (User.Identity is not EpicUserIdentity user)
		{
			return Unauthorized();
		}

		if (user.Session.AccountID != EpicID.FromString(id))
		{
			return Unauthorized();
		}

		// only known to be sent by client so far
		//var isRequestSentFromClient = clientKind.ToLower() == "client";
		//var isRequestSentFromServer = clientKind.ToLower() == "dedicated_server";

		// this endpoint is kind of pointless. the actual stars are stored in cloudstorage progression file.
		// then whenever it is changed, it sends an update to master server.

		Account? account = await accountService.GetAccountAsync(user.Session.AccountID);
		if (account == null)
		{
			return BadRequest();
		}

		account.GoldStars = body.NewGoldStars;
		account.BlueStars = body.NewBlueStars;

		await accountService.UpdateAccountAsync(account);

		// TODO: send out a proper response which is similar to QueryProfile

		return Ok();
	}
}
