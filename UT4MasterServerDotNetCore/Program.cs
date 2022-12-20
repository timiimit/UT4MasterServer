using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using System.Text.Encodings.Web;
using UT4MasterServer.Authentication;
using UT4MasterServer.Models;
using UT4MasterServer.Services;

namespace UT4MasterServer;

public class BasicHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
	public BasicHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
	{
	}

	protected override Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		throw new NotImplementedException();
	}
}

public class BearerHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
	public BearerHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
	{
	}

	protected override Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		throw new NotImplementedException();
	}
}

public static partial class Program
{
	public static void Main(string[] args)
	{
		// register serializer for EpicID type
		BsonSerializer.RegisterSerializationProvider(new EpicIDSerializationProvider());

		// start up asp.net
		var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddControllers();
    builder.Services.Configure<UT4EverDatabaseSettings>(
      builder.Configuration.GetSection("UT4EverDatabase")
    );
    //builder.Services.ConfigureSwaggerGen(options =>
    //{
    //	options.OperationFilter<SwaggerAuthorizationHeaderOperationFilter>();
    //});
    builder.Services
      .AddSingleton<AccountService>()
      .AddSingleton<SessionService>()
      .AddSingleton<AccountDataService>();	

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