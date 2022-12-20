

using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;

namespace UT4MasterServer.Controllers;

/// <summary>
/// Should be used as a base for all controllers that handle json responses.
/// The goal of this class is to provide an easy way to guarantee a properly
/// formed response and proper Content-Type header when dealing with json.
/// </summary>
public class JsonAPIController : ControllerBase
{
	private static readonly string mimeJson = "application/json";

	public ContentResult Json(string content)
	{
		// i cant find a better way than to do this.
		return Content(content, mimeJson);
	}

	public JsonResult Json(JObject content)
	{
		return new JsonResult(content.ToString(Newtonsoft.Json.Formatting.None));
	}

	public JsonResult Json(object? content)
	{
		return new JsonResult(content);
	}
}