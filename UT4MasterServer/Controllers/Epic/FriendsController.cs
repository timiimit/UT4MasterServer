﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using UT4MasterServer.Authentication;
using UT4MasterServer.Models.Database;
using UT4MasterServer.Common;
using UT4MasterServer.Services.Scoped;
using UT4MasterServer.Common.Helpers;

namespace UT4MasterServer.Controllers.Epic;

/// <summary>
/// friends-public-service-prod06.ol.epicgames.com
/// </summary>
[ApiController]
[Route("friends/api/public")]
[AuthorizeBearer]
[Produces("application/json")]
public sealed class FriendsController : JsonAPIController
{
    private readonly FriendService friendService;

    public FriendsController(ILogger<FriendsController> logger, FriendService friendService) : base(logger)
    {
        this.friendService = friendService;
    }

    #region friends

    [HttpGet("friends/{id}")]
    public async Task<IActionResult> GetFriends(string id, [FromQuery] bool? includePending)
    {
        if (User.Identity is not EpicUserIdentity authenticatedUser)
            return Unauthorized();

        var eid = EpicID.FromString(id);

        if (eid != authenticatedUser.Session.AccountID)
            return Json("[]", StatusCodes.Status401Unauthorized);

        // TODO: Check permission: "Sorry your login does not posses the permission 'friends:{id_from_parameter} READ' needed to perform the requested operation"

        var friends = await friendService.GetFriendsAsync(eid);

        JArray arr = new JArray();
        foreach (var friend in friends)
        {
            var other = friend.Sender == eid ? friend.Receiver : friend.Sender;
            var status = friend.Status == FriendStatus.Accepted ? "ACCEPTED" : "PENDING";
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

    [HttpPost("friends/{id}/{friendID}")]
    public async Task<ActionResult> SendFriendRequest(string id, string friendID)
    {
        if (User.Identity is not EpicUserIdentity authenticatedUser)
            return Unauthorized();

        var eid = EpicID.FromString(id);

        if (eid != authenticatedUser.Session.AccountID)
            return Json("[]", StatusCodes.Status401Unauthorized);

        await friendService.SendFriendRequestAsync(eid, EpicID.FromString(friendID));

        return NoContent();
    }

    [HttpDelete("friends/{id}/{friendID}")]
    public async Task<ActionResult> RemoveFriend(string id, string friendID)
    {
        if (User.Identity is not EpicUserIdentity authenticatedUser)
            return Unauthorized();

        var eid = EpicID.FromString(id);

        if (eid != authenticatedUser.Session.AccountID)
            return Json("[]", StatusCodes.Status401Unauthorized);

        await friendService.CancelFriendRequestAsync(eid, EpicID.FromString(friendID));

        return NoContent();
    }

    #endregion

    #region blocklist

    [HttpGet("blocklist/{id}")]
    public async Task<IActionResult> GetBlockedAccounts(string id)
    {
        if (User.Identity is not EpicUserIdentity authenticatedUser)
            return Json("[]", StatusCodes.Status401Unauthorized);

        var eid = EpicID.FromString(id);

        if (eid != authenticatedUser.Session.AccountID)
            return Json("[]", StatusCodes.Status401Unauthorized);

        // TODO: Check permission: "Sorry your login does not posses the permission 'blockList:{id_from_parameter} READ' needed to perform the requested operation"

        var blockedUsers = await friendService.GetBlockedUsersAsync(eid);

        JArray arr = new JArray();
        foreach (var blockedUser in blockedUsers)
        {
            arr.Add(blockedUser.Receiver.ToString());
        }
        return Json(arr);
    }

    [HttpPost("blocklist/{id}/{friendID}")]
    public async Task<ActionResult> BlockAccount(string id, string friendID)
    {
        if (User.Identity is not EpicUserIdentity authenticatedUser)
            return Unauthorized();

        var eid = EpicID.FromString(id);

        if (eid != authenticatedUser.Session.AccountID)
            return Json("[]", StatusCodes.Status401Unauthorized);

        await friendService.BlockAccountAsync(eid, EpicID.FromString(friendID));

        return NoContent();
    }

    [HttpDelete("blocklist/{id}/{friendID}")]
    public async Task<ActionResult> UnblockAccount(string id, string friendID)
    {
        if (User.Identity is not EpicUserIdentity authenticatedUser)
            return Unauthorized();

        var eid = EpicID.FromString(id);

        if (eid != authenticatedUser.Session.AccountID)
            return Json("[]", StatusCodes.Status401Unauthorized);

        await friendService.UnblockAccountAsync(eid, EpicID.FromString(friendID));

        return NoContent();
    }

    #endregion
}
