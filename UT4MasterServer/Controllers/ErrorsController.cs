using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UT4MasterServer.Exceptions;
using UT4MasterServer.Models;

namespace UT4MasterServer.Controllers;

[ApiController, ApiExplorerSettings(IgnoreApi = true)]
[Route("api/errors")]
public class ErrorsController : ControllerBase
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
				case InvalidEpicIDException invalidEpicIDException:
					return StatusCode(400, new ErrorResponse()
					{
						ErrorCode = invalidEpicIDException.ErrorCode,
						ErrorMessage = invalidEpicIDException.Message,
						MessageVars = new string[] { invalidEpicIDException.ID },
						NumericErrorCode = invalidEpicIDException.NumericErrorCode
					});
			}
		}

		return StatusCode(statusCode, message);
	}
}
