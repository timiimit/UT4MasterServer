using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UT4MasterServer.Exceptions;

namespace UT4MasterServer.Controllers;

[ApiController, ApiExplorerSettings(IgnoreApi = true)]
[Route("api/errors")]
public class ErrorsController : Controller
{
	[HttpGet]
	public IActionResult Index()
	{
		var message = "Internal server error occurred.";
		var statusCode = 500;

		var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

		if (exceptionHandlerFeature?.Error is { } ex)
		{
			switch (ex)
			{
				case InvalidEpicIDException:
					message = ex.Message;
					statusCode = 400;
					break;
			}
		}

		return StatusCode(statusCode, message);
	}
}
