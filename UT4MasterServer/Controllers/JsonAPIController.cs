using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Net;
using UT4MasterServer.Models.Settings;

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

	//[NonAction]
	//public JsonResult Json(object? content)
	//{
	//	return new JsonResult(content, new JsonSerializerOptions() { Converters = { new EpicIDJsonConverter() } });
	//}

	//[NonAction]
	//public JsonResult Json(object? content, int status)
	//{
	//	return new JsonResult(content, new JsonSerializerOptions() { Converters = { new EpicIDJsonConverter() } }) { StatusCode = status };
	//}

	[NonAction]
	protected IPAddress? GetClientIP(IOptions<ApplicationSettings>? proxyInfo)
	{
		var ipAddress = HttpContext.Connection.RemoteIpAddress;
		if (ipAddress == null)
		{
			return null;
		}

		// if we have no proxy info, we can only trust the actual ip
		if (proxyInfo == null)
		{
			return ipAddress;
		}

		// if we don't know the header that proxy is supposed to use,
		// we can only trust the actual ip
		if (string.IsNullOrWhiteSpace(proxyInfo.Value.ProxyClientIPHeader))
		{
			return ipAddress;
		}

		// get all instances of specified header
		var headers = HttpContext.Request.Headers[proxyInfo.Value.ProxyClientIPHeader];

		// info on how to generally handle X-Forwarded-For:
		// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Forwarded-For

		// look through each instance of the header bottom-to-top
		for (var hi = headers.Count - 1; hi >= 0; hi--)
		{
			var header = headers[hi];
			if (header == null)
			{
				continue;
			}

			string[] headerParts = header.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

			// look through each part of header from right-to-left
			for (var i = headerParts.Length - 1; i >= 0; i--)
			{
				// determine whether we trust last sender
				if (ipAddress != null &&
					IsTrustedMachine(proxyInfo, ipAddress) &&
					IPAddress.TryParse(headerParts[i], out ipAddress))
				{
					continue;
				}

				// exist straight out of all loops
				hi = 0;
				break;
			}
		}

		// return last ip to be trusted as origin of request
		return ipAddress;
	}

	private bool IsTrustedMachine(IOptions<ApplicationSettings> proxyInfo, IPAddress ip)
	{
		foreach (var trustedProxyString in proxyInfo.Value.ProxyServers)
		{
			if (!IPAddress.TryParse(trustedProxyString, out var trustedProxy))
			{
				continue;
			}

			if (trustedProxy.Equals(ip))
			{
				return true;
			}
		}
		return false;
	}
}
