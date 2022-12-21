

using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace UT4MasterServer.Controllers;

/// <summary>
/// Should be used as a base for all controllers that handle json responses.
/// The goal of this class is to provide an easy way to guarantee a properly
/// formed response and proper Content-Type header when dealing with json.
/// </summary>
public class JsonAPIController : ControllerBase
{
	private static readonly string mimeJson = "application/json";

	[NonAction]
	public ContentResult Json(string content)
	{
		// i cant find a better way than to do this.
		return Content(content, mimeJson);
	}

	[NonAction]
	public ContentResult Json(string content, int status)
	{
		// i cant find a better way than to do this.
		var r = Content(content, mimeJson);
		r.StatusCode = status;
		return r;
	}

	[NonAction]
	public ContentResult Json(JToken content)
	{
		return Json(content.ToString(Newtonsoft.Json.Formatting.None));
	}

	[NonAction]
	public ContentResult Json(JToken content, int status)
	{
		var r = Json(content.ToString(Newtonsoft.Json.Formatting.None));
		r.StatusCode = status;
		return r;
	}

	[NonAction]
	public ContentResult Json(JToken content, bool humanReadable)
	{
		var formatting = humanReadable ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None;
		return Json(content.ToString(formatting));
	}

	[NonAction]
	public ContentResult Json(JToken content, int status, bool humanReadable)
	{
		var formatting = humanReadable ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None;
		var r = Json(content.ToString(formatting));
		r.StatusCode = status;
		return r;
	}

	[NonAction]
	public JsonResult Json(object? content)
	{
		return new JsonResult(content, new JsonSerializerOptions() { Converters = { new EpicIDJsonConverter() } });
	}

	[NonAction]
	public JsonResult Json(object? content, int status)
	{
		return new JsonResult(content, new JsonSerializerOptions() { Converters = { new EpicIDJsonConverter() } }) { StatusCode = status };
	}
}