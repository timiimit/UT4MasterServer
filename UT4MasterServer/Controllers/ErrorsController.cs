using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UT4MasterServer.Common.Exceptions;
using UT4MasterServer.Models.DTO.Responses;

namespace UT4MasterServer.Controllers;

[ApiController, ApiExplorerSettings(IgnoreApi = true)]
[Route("api/errors")]
public sealed class ErrorsController : ControllerBase
{
	private const string NotFoundError = "Not found.";
	private const string BadRequestError = "Bad request.";
	private const string InternalServerError = "Internal server error occurred.";
	private const string UnauthorizedError = "Attempt to access resource without required authorization.";

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
					logger.LogWarning(exception, UnauthorizedError);
					return StatusCode(401, new ErrorResponse()
					{
						ErrorCode = "com.epicgames.errors.unauthorized",
						ErrorMessage = string.IsNullOrWhiteSpace(unauthorizedAccessException.Message) ? UnauthorizedError : unauthorizedAccessException.Message,
						NumericErrorCode = 401
					});
				}

			case AccountActivationException accountActivationException:
				{
					var err = new ErrorResponse()
					{
						ErrorCode = "ut4masterserver.errors.accountactivation",
						ErrorMessage = accountActivationException.Message,
						MessageVars = Array.Empty<string>(),
						NumericErrorCode = 404
					};

					logger.LogError(accountActivationException, "Account activation failed.");
					return StatusCode(404, err);
				}

			case AccountNotActiveException accountNotActiveException:
				{
					var err = new ErrorResponse()
					{
						ErrorCode = "ut4masterserver.errors.accountpendingactivation",
						ErrorMessage = accountNotActiveException.Message,
						MessageVars = Array.Empty<string>(),
						NumericErrorCode = 401
					};

					logger.LogError(accountNotActiveException, "Account pending activation.");
					return StatusCode(401, err);
				}

			case NotFoundException notFoundException:
				{
					var err = new ErrorResponse()
					{
						ErrorCode = "ut4masterserver.notfound",
						ErrorMessage = notFoundException.Message,
						MessageVars = Array.Empty<string>(),
						NumericErrorCode = 404
					};

					logger.LogWarning(notFoundException, NotFoundError);
					return StatusCode(404, err);
				}

			case RateLimitExceededException rateLimitExceededException:
				{
					var err = new ErrorResponse()
					{
						ErrorCode = "ut4masterserver.ratelimitexceeded",
						ErrorMessage = rateLimitExceededException.Message,
						MessageVars = Array.Empty<string>(),
						NumericErrorCode = 400
					};

					logger.LogWarning(rateLimitExceededException, BadRequestError);
					return StatusCode(400, err);
				}
		}

		logger.LogError(exception, InternalServerError);
		return StatusCode(statusCode, message);
	}
}
