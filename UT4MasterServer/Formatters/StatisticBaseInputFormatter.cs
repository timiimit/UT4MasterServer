using Microsoft.AspNetCore.Mvc.Formatters;
using System.Text.Json;
using UT4MasterServer.Models.Database;

namespace UT4MasterServer.Formatters;

public sealed class StatisticBaseInputFormatter : InputFormatter
{
	public StatisticBaseInputFormatter()
	{
		SupportedMediaTypes.Add("application/json");
	}

	public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
	{
		using var reader = new StreamReader(context.HttpContext.Request.Body);

		var rawValue = await reader.ReadToEndAsync();

		var newObject = JsonSerializer.Deserialize<StatisticBase>(rawValue[..^1]);

		return InputFormatterResult.Success(newObject);
	}

	protected override bool CanReadType(Type type)
	{
		return type == typeof(StatisticBase);
	}
}
