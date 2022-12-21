using Microsoft.AspNetCore.Authentication;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json.Linq;
using System.Text.Encodings.Web;
using UT4MasterServer.Authentication;
using UT4MasterServer.Controllers;
using UT4MasterServer.Models;
using UT4MasterServer.Services;

namespace UT4MasterServer;

public static class Program
{
	public static void Main(string[] args)
	{
		// register serializer for EpicID type
		BsonSerializer.RegisterSerializationProvider(new EpicIDSerializationProvider());

		// start up asp.net
		var builder = WebApplication.CreateBuilder(args);

		builder.Services.AddControllers(o =>
		{
			o.RespectBrowserAcceptHeader = true;
		});
		builder.Services.Configure<UT4EverDatabaseSettings>(
		  builder.Configuration.GetSection("UT4EverDatabase")
		);
		builder.Services
		  .AddSingleton<AccountService>()
		  .AddSingleton<SessionService>()
		  .AddSingleton<CloudstorageService>()
		  .AddSingleton<MatchmakingService>();

		builder.Services
		  .AddAuthentication(/*by default there is no authentication*/)
		  .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("basic", null)
		  .AddScheme<AuthenticationSchemeOptions, BearerAuthenticationHandler>("bearer", null);
		builder.Host.ConfigureLogging(logging =>
		{
			logging.ClearProviders();
			logging.AddConsole();
		});

		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen();

		var app = builder.Build();

		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}

		app.UseHttpsRedirection();
		app.UseAuthorization();
		app.UseAuthentication();
		app.MapControllers();

		app.Run();
	}
}