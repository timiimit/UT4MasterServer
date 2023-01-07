using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using UT4MasterServer.Other;

namespace UT4MasterServer.Controllers;

/// <summary>
/// Should be used as a base for all controllers that handle json responses.
/// The goal of this class is to provide an easy way to guarantee a properly
/// formed response and proper Content-Type header when dealing with json.
/// </summary>
public class JsonAPIController : ControllerBase
{
	private const string MimeJson = "application/json";

	[NonAction]
	public ContentResult Json(string content)
	{
		// i cant find a better way than to do this.
		return Content(content, MimeJson);
	}

	[NonAction]
	public ContentResult Json(string content, int status)
	{
		// i cant find a better way than to do this.
		var r = Content(content, MimeJson);
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



	[NonAction]
	protected string GetClientIPString()
	{
		string? tmp = null;

		// try to locate any known proxy related headers

		// standard cloudflare header
		tmp = HttpContext.Request.Headers["CF-Client-IP"]
				.GroupBy(s => s)
				.OrderByDescending(x => x.Count())
				.Select(x => x.Key)
				.FirstOrDefault();

		if (tmp != null)
			return tmp;

		// normal http header
		tmp = HttpContext.Request.Headers["X-Forwarded-For"]
				.FirstOrDefault();

		if (tmp != null)
			return tmp;

		// enterprise cloudflare header
		tmp = HttpContext.Request.Headers["True-Client-IP"]
				.GroupBy(s => s)
				.OrderByDescending(x => x.Count())
				.Select(x => x.Key)
				.FirstOrDefault();

		if (tmp != null)
			return tmp;


		// when connection is not proxied, we have no choice
		// but to take the actual IP address of remote

		var ipAddress = HttpContext.Connection.RemoteIpAddress;
		if (ipAddress == null)
		{
			// for some reason we were unable to determine remote IP...
			logger.LogError($"Could not determine ip address of remote GameServer, this issue needs to be resolved!");
			return string.Empty;
		}

		if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
		{
			// ipv6 does not seem to work
			logger.LogWarning($"GameServer is connecting from ipv6 address ({ipAddress})! mapping to ipv4...");
			ipAddress = ipAddress.MapToIPv4();
		}
		return ipAddress.ToString();
	}
}
