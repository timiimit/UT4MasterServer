using Microsoft.AspNetCore.Mvc;
using UT4MasterServer.Authorization;
using UT4MasterServer.Services;

namespace UT4MasterServer.Controllers
{
    [ApiController]
    [Route("ut/api/cloudstorage/user/")]
    [AuthorizeBearer]
    [Produces("application/json")]
    public class CloudStorageController : JsonAPIController
	{
        private readonly ILogger<SessionController> logger;
        private readonly AccountService accountService;
        private readonly SessionService sessionService;

        public CloudStorageController(SessionService sessionService, AccountService accountService, AccountDataService accountDataService, ILogger<SessionController> logger)
        {
            this.sessionService = sessionService;
            this.accountService = accountService;
            this.logger = logger;
        }

        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            return new NoContentResult();
        }

        [HttpGet("{id}/user_profile_2")]
        [Produces("application/octet-stream")]
        public async Task<IActionResult> QueryProfile(string id)
        {
            // Temp data
            string path = "user_profile_2.local";
            byte[] data = await System.IO.File.ReadAllBytesAsync(path);
            return new FileContentResult(data, "application/octet-stream");
        }

        [HttpPut("{id}/user_profile_2")]
        [Produces("application/octet-stream")]
        public IActionResult UpdateProfile(string id)
        {
            return new NoContentResult();  
        }

        // TODO: user_progression_1
    }
}
