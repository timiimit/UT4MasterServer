using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text.Json;
using UT4MasterServer.Models;
using UT4MasterServer.Other;

namespace UT4MasterServer.Controllers;

/// <summary>
/// Should be used as a base for all controllers that handle json responses.
/// The goal of this class is to provide an easy way to guarantee a properly
/// formed response and proper Content-Type header when dealing with json.
/// </summary>
public class JsonAPIController : ControllerBase
{
	protected readonly ILogger<JsonAPIController> logger;
	private const string MimeJson = "application/json";

	public JsonAPIController(ILogger<JsonAPIController> logger)
	{
		this.logger = logger;
	}

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


	private const string HTTP_Header_XForwardedFor = "X-Forwarded-For";

	[NonAction]
	protected IPAddress? GetClientIP(IOptions<ApplicationSettings>? proxyInfo)
	{
		var ipAddress = HttpContext.Connection.RemoteIpAddress;
		if (ipAddress == null)
			return null;

		if (proxyInfo == null)
			return ipAddress;

		if (string.IsNullOrWhiteSpace(proxyInfo.Value.ProxyClientIPHeader))
			return ipAddress;

		// try locating IPAddress via proxy server's HTTP header
		foreach (var proxy in proxyInfo.Value.ProxyServers)
		{
			if (!IPAddress.TryParse(proxy, out var ipProxy))
				continue;

			if (!ipProxy.Equals(ipAddress))
				continue;

			if (proxyInfo.Value.ProxyClientIPHeader != HTTP_Header_XForwardedFor)
			{
				var headers = HttpContext.Request.Headers[proxyInfo.Value.ProxyClientIPHeader];
				if (headers.Count > 0)
				{
					if (IPAddress.TryParse(headers[0], out var ipClient))
						return ipClient;
				}
			}

			// additionally, try handle X-Forwarded-For header, because that's a standard header
			{
				var headers = HttpContext.Request.Headers[HTTP_Header_XForwardedFor];
				foreach (var headerInstance in headers)
				{
					string[] parts = headerInstance.Split(',');
					foreach (var part in parts)
					{
						// we return at first valid ip address
						if (IPAddress.TryParse(part, out var ipClient))
							return ipClient;
					}
				}
			}
		}

		// no headers found, return remote ip
		return ipAddress;
	}
}
