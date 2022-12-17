using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UT4MasterServer.Models;
using UT4MasterServer.Services;

namespace UT4MasterServer.Controllers
{
    [Route("account/api/oauth/token")]
    [ApiController]
    public class TokenResponseController : ControllerBase
    {
        private readonly ILogger<TokenResponseController> _logger;
        public TokenResponseController(ILogger<TokenResponseController> logger) 
        { 
            _logger= logger;
        }

        [HttpPost]
        public ActionResult<TokenResponse> Post()
        {
            _logger.LogInformation(@"POST request for {path}", Request.Path.Value);
            var tokenResponse = new TokenResponse();
            return tokenResponse;
        }
    }
}
