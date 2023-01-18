using Serilog;
using Serilog.Enrichers;

namespace UT4MasterServer.Configuration;

public class InternalLoggerConfiguration
{
	public static void Configure(IHostEnvironment env, IConfiguration configuration)
	{
		const string outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss zzz} [{Level}] ({ThreadId}) {RequestId}-{SourceContext}: {Message}{NewLine}{Exception}";
		var filePath = Path.Combine(env.ContentRootPath, "Logs", "log-.log");

		Log.Logger = new LoggerConfiguration()
			.ReadFrom.Configuration(configuration)
			.Enrich.FromLogContext()
			.WriteTo.Console(outputTemplate: outputTemplate)
			.WriteTo.Debug(outputTemplate: outputTemplate)
			.WriteTo.File(filePath, rollOnFileSizeLimit: true, rollingInterval: RollingInterval.Day, outputTemplate: outputTemplate, shared: true)
			.Enrich.With<ThreadIdEnricher>()
			.CreateLogger();
	}
}
