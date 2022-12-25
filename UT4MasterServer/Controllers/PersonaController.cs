
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using UT4MasterServer.Authorization;
using UT4MasterServer.Services;

namespace UT4MasterServer.Controllers;

[ApiController]
[Route("persona/api/public")]
[AuthorizeBearer]
[Produces("application/json")]
// persona-public-service-prod06.ol.epicgames.com
public class PersonaController : JsonAPIController
{
	private readonly AccountService accountService;

	public PersonaController(AccountService accountService)
	{
		this.accountService = accountService;
	}

	[HttpGet("account/lookup")]
	public async Task<IActionResult> AccountLookup([FromQuery(Name = "q")] string query)
	{
		if (User.Identity is not EpicUserIdentity authenticatedUser)
			return Unauthorized();

		var account = await accountService.GetAccountAsync(query);
		if (account == null)
			return BadRequest();

		var obj = new JObject();
		obj.Add("id", account.ID.ToString());
		obj.Add("displayName", account.Username);
		obj.Add("extenalAuths", new JObject());

		return Json(obj);
	}
}