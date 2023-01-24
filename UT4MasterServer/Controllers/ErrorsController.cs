using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UT4MasterServer.Exceptions;
using UT4MasterServer.Models;

namespace UT4MasterServer.Controllers;

[ApiController, ApiExplorerSettings(IgnoreApi = true)]
[Route("api/errors")]
public sealed class ErrorsController : ControllerBase
{
	private const string InternalServerError = "Internal server error occurred.";

	private readonly ILogger<ErrorsController> logger;

	public ErrorsController(ILogger<ErrorsController> logger)
	{
		this.logger = logger;
	}

	public IActionResult Index()
	{
		var message = InternalServerError;
		var statusCode = 500;

		var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
		var exception = exceptionHandlerFeature?.Error;

		if (exception is null)
		{
			logger.LogError(InternalServerError);
			return StatusCode(statusCode, message);
		}
		
		switch (exception)
		{
			case InvalidEpicIDException invalidEpicIDException:
			{
				var err = new ErrorResponse()
				{
					ErrorCode = invalidEpicIDException.ErrorCode,
					ErrorMessage = invalidEpicIDException.Message,
					MessageVars = new string[] { invalidEpicIDException.ID },
					NumericErrorCode = invalidEpicIDException.NumericErrorCode
				};

				logger.LogError(exception, "Tried using {ID} as EpicID.", invalidEpicIDException.ID);
				return StatusCode(400, err);
			}

			case UnauthorizedAccessException unauthorizedAccessException:
			{
				logger.LogWarning(exception, "Attempt to access resource without required authorization");
				return StatusCode(401, new ErrorResponse()
				{
					ErrorCode = "com.epicgames.errors.unauthorized",
					ErrorMessage = unauthorizedAccessException.Message,
					NumericErrorCode = 401
				});
			}
		}

		logger.LogError(exception, InternalServerError);
		return StatusCode(statusCode, message);
	}
}
