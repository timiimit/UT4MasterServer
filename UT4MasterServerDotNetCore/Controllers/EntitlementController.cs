
using Microsoft.AspNetCore.Mvc;
using UT4MasterServer.Authorization;

namespace UT4MasterServer.Controllers
{
    [ApiController]
    [Route("entitlement/api/account/{id}/entitlements")]
    [AuthorizeBearer]
    [Produces("application/json")]
    public class EntitlementController : ControllerBase
    {
        private readonly ILogger<SessionController> logger;

        public EntitlementController(ILogger<SessionController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        public ActionResult<string> QueryProfile(string id)
        {
            return "{}";
        }
    }
}
