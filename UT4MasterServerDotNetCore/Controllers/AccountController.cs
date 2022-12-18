using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UT4MasterServer.Models;
using UT4MasterServer.Services;

namespace UT4MasterServer.Controllers
{

    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AccountService _accountService;
        private readonly ILogger<AccountController> _logger;
        public AccountController(AccountService accountService, ILogger<AccountController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        [Route("account/api/public/account/{id}")]
        [HttpGet]
        public async Task<ActionResult<Account?>> GetAccount(string id)
        {
            _logger.Log(LogLevel.Information, @"Looking for {id}", id);
            var account = await _accountService.GetAsync(id);
            if (account is null)
            {
                return NotFound();
            }

            return account;
        }

        [Route("account/api/public/account")]
        [HttpGet]
        public async Task<ActionResult<List<Account>>> GetAccounts([FromQuery(Name = "accountId")] List<string> accountIds)
        {
            _logger.LogInformation(@"Looking for {id} from a query parameter", string.Join(", ", accountIds));
            var accounts = await _accountService.GetAsync(accountIds);
            if (accounts is null || accounts.Count == 0)
            {
                return NotFound();
            }
            return accounts;
        }

        // Paths that are either unused or untested will return 200 and an empty response
        [Route("account/api/accounts/{id?}/metadata")]
        [Route("account/api/public/account/{id?}/externalAuths")]
        [HttpGet]
        public IActionResult GetEmpty()
        {
            _logger.LogInformation(@"GET request for {path}", Request.Path.Value);
            return new EmptyResult();
        }

        [Route("account/api/epicdomains/ssodomains")]
        [HttpGet]
        public ActionResult<String> GetSSODomains()
        {
            _logger.LogInformation(@"GET request for SSO domains");
            return "[]"; // TODO: Is this the proper way to return an empty JSON array??
        }

        [Route("account/api/oauth/sessions/kill/{id?}")]
        [HttpDelete]
        public NoContentResult Delete()
        {
            _logger.LogInformation(@"DELETE request for {path}", Request.Path.Value);
            return new NoContentResult();
        }
    }
}
