
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
            return Json("{}");
        }
    }
}
