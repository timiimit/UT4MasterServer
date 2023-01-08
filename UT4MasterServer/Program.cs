using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;
using MongoDB.Bson.Serialization;
using System.Net;
using UT4MasterServer.Authentication;
using UT4MasterServer.Models;
using UT4MasterServer.Other;
using UT4MasterServer.Services;

namespace UT4MasterServer;

public static class Program
{
	public static DateTime StartupTime { get; } = DateTime.UtcNow;

	public static void Main(string[] args)
	{
		// register serializers for custom types
		BsonSerializer.RegisterSerializationProvider(new BsonSerializationProvider());

		// start up asp.net
		var builder = WebApplication.CreateBuilder(args);

		builder.Services.AddControllers(o =>
		{
			o.RespectBrowserAcceptHeader = true;
		});

		builder.Services.Configure<ApplicationSettings>(
			builder.Configuration.GetSection("ApplicationSettings")
		);
		builder.Services.Configure<ApplicationSettings>(x =>
		{
			// handle proxy list loading
			if (string.IsNullOrWhiteSpace(x.ProxyServersFile))
				return;

			try
			{
				var proxies = File.ReadAllLines(x.ProxyServersFile);
				foreach (var proxy in proxies)
				{
					if (!IPAddress.TryParse(proxy, out var ip))
						continue;

					x.ProxyServers.Add(ip.ToString());
				}
			}
			catch
			{
				// we ignore the fact that proxy list file was not found
			}
		});


		// services whose instance is created per-request
		builder.Services
			.AddScoped<DatabaseContext>()
			.AddScoped<FriendService>()
			.AddScoped<AccountService>()
			.AddScoped<SessionService>()
			.AddScoped<CloudStorageService>()
			.AddScoped<MatchmakingService>()
			.AddScoped<StatisticsService>();

		// services whose instance is created once and are persistent
		builder.Services
			.AddSingleton<CodeService>();

		builder.Services
			.AddAuthentication(/*by default there is no authentication*/)
			.AddScheme<AuthenticationSchemeOptions, BearerAuthenticationHandler>(HttpAuthorization.BearerScheme, null)
			.AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(HttpAuthorization.BasicScheme, null);

		builder.Host
			.ConfigureLogging(logging =>
			{
				logging.ClearProviders();
				logging.AddConsole();
			})
			.ConfigureServices(services =>
			{
				services.AddHostedService<ApplicationStartupService>();
			});

		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen(config =>
		{
			config.AddSecurityDefinition(HttpAuthorization.BearerScheme, new OpenApiSecurityScheme
			{
				Description = "Copy 'Bearer ' + valid JWT token into field",
				Type = SecuritySchemeType.ApiKey,
				In = ParameterLocation.Header,
				Scheme = HttpAuthorization.BearerScheme,
				Name = HttpAuthorization.AuthorizationHeader,
			});

			config.AddSecurityRequirement(new OpenApiSecurityRequirement
			{
				{
					new OpenApiSecurityScheme
					{
						Reference = new OpenApiReference
						{
							Type = ReferenceType.SecurityScheme,
							Id = HttpAuthorization.BearerScheme,
						}
					},
					new string[] { }
				},
			});
		});

		var app = builder.Build();

		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}

		//app.UseHttpsRedirection();
		app.UseAuthorization();
		app.UseAuthentication();
		app.MapControllers();
		app.UseStaticFiles();

		// TODO: restrict origin
		app.UseCors(x =>
		{
			x.AllowAnyOrigin()
				.AllowAnyHeader()
				.AllowAnyMethod();
		});

		app.Run();
	}
}
