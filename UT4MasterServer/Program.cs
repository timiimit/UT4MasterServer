using System.Configuration;
using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;
using MongoDB.Bson.Serialization;
using Serilog;
using UT4MasterServer.Authentication;
using UT4MasterServer.Common;
using UT4MasterServer.Configuration;
using UT4MasterServer.Formatters;
using UT4MasterServer.Models.Database;
using UT4MasterServer.Models.Settings;
using UT4MasterServer.Serializers.Bson;
using UT4MasterServer.Serializers.Json;
using UT4MasterServer.Services;
using UT4MasterServer.Services.Hosted;
using UT4MasterServer.Services.Scoped;
using UT4MasterServer.Services.Singleton;

namespace UT4MasterServer;

public static class Program
{
	public static DateTime StartupTime { get; } = DateTime.UtcNow;

	public static void Main(string[] args)
	{
		// register serializers for custom types
		BsonSerializer.RegisterSerializationProvider(new BsonSerializationProvider());

		// set defaults for bson, where defaults are not a constant expression
		BsonClassMap.RegisterClassMap<Account>(x =>
		{
			x.AutoMap();
			x.MapMember(x => x.LastMatchAt).SetDefaultValue(DateTime.UnixEpoch);
			x.MapMember(x => x.DeviceIDs).SetDefaultValue(Array.Empty<string>());
		});
		BsonClassMap.RegisterClassMap<Session>(x =>
		{
			x.AutoMap();
			x.MapMember(x => x.AccountID).SetDefaultValue(EpicID.Empty);
		});
		BsonClassMap.RegisterClassMap<CloudFile>(x =>
		{
			x.AutoMap();
			x.MapMember(x => x.AccountID).SetDefaultValue(EpicID.Empty);
		});

		// start up asp.net
		WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

		builder.Services.AddControllers(o =>
		{
			o.RespectBrowserAcceptHeader = true;
			o.InputFormatters.Insert(0, new StatisticBaseInputFormatter());
		}).AddJsonOptions(o =>
		{
			o.JsonSerializerOptions.Converters.Add(new EpicIDJsonConverter());
			o.JsonSerializerOptions.Converters.Add(new GameServerAttributesJsonConverter());
			o.JsonSerializerOptions.Converters.Add(new DateTimeISOJsonConverter());
		});

		// load settings objects
		builder.Services
			.Configure<ApplicationSettings>(builder.Configuration.GetSection("ApplicationSettings"))
			.Configure<StatisticsSettings>(builder.Configuration.GetSection("StatisticsSettings"))
			.Configure<ReCaptchaSettings>(builder.Configuration.GetSection("ReCaptchaSettings"));

		builder.Services.Configure<ApplicationSettings>(x =>
		{
			// handle proxy list loading
			if (string.IsNullOrWhiteSpace(x.ProxyServersFile))
			{
				return;
			}

			try
			{
				var proxies = File.ReadAllLines(x.ProxyServersFile);
				foreach (var proxy in proxies)
				{
					if (!IPAddress.TryParse(proxy, out IPAddress? ip))
					{
						continue;
					}

					x.ProxyServers.Add(ip.ToString());
				}
			}
			catch
			{
				// we ignore the fact that proxy list file was not found
			}
		});

		builder.Services.Configure<ReCaptchaSettings>(x =>
		{
			if (builder.Environment.IsProduction())
			{
				if (string.IsNullOrWhiteSpace(x.SecretKey) || string.IsNullOrWhiteSpace(x.SiteKey))
				{
					throw new ConfigurationErrorsException("Must specify ApplicationSettings.ReCaptchaSettings in production environment");
				}
			}
		});

		// services whose instance is created per-request
		builder.Services
			.AddScoped<DatabaseContext>()
			.AddScoped<ClientService>()
			.AddScoped<AccountService>()
			.AddScoped<SessionService>()
			.AddScoped<FriendService>()
			.AddScoped<CloudStorageService>()
			.AddScoped<TrustedGameServerService>()
			.AddScoped<MatchmakingService>()
			.AddScoped<StatisticsService>()
			.AddScoped<RatingsService>();

		// services whose instance is created once and are persistent
		builder.Services
			.AddSingleton<RuntimeInfoService>()
			.AddSingleton<CodeService>()
			.AddSingleton<MatchmakingWaitTimeEstimateService>();

		// hosted services
		builder.Services
			.AddHostedService<ApplicationStartupService>()
			.AddHostedService<ApplicationBackgroundCleanupService>();

		builder.Services
			.AddAuthentication(/*by default there is no authentication*/)
			.AddScheme<AuthenticationSchemeOptions, BearerAuthenticationHandler>(HttpAuthorization.BearerScheme, null)
			.AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(HttpAuthorization.BasicScheme, null);

		builder.Services.AddLogging(builder =>
		{
			builder.AddSerilog();
		});

		builder.Services.AddControllers();

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
					Array.Empty<string>()
				},
			});
		});

		const string allowOriginsPolicy = "_ut4msOriginsPolicy";
		const string devAllowOriginsPolicy = "_ut4msDevOriginsPolicy";

		builder.Services.AddCors(options =>
		{
			if (builder.Environment.IsDevelopment())
			{
				options.AddPolicy(
					devAllowOriginsPolicy,
					policy =>
					{
						policy.WithOrigins("http://localhost:5001", "http://localhost:8080", "http://localhost:80");
						policy.AllowAnyHeader();
						policy.AllowAnyMethod();
					}
				);
			}
			else
			{
				var websiteDomain = builder.Configuration.GetSection("ApplicationSettings")["WebsiteDomain"];
				if (string.IsNullOrWhiteSpace(websiteDomain))
				{
					throw new ConfigurationErrorsException("Must specify ApplicationSettings.WebsiteDomain in production environment");
				}

				options.AddPolicy(
					allowOriginsPolicy,
					policy =>
					{
						policy.WithOrigins($"https://{websiteDomain}");
						policy.AllowAnyHeader();
						policy.AllowAnyMethod();
					}
				);
			}
		});

		WebApplication? app = builder.Build();

		InternalLoggerConfiguration.Configure(app.Environment, app.Configuration);

		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
			app.UseCors(devAllowOriginsPolicy);
		}
		else
		{
			app.UseCors(allowOriginsPolicy);
		}

		//app.UseHttpsRedirection();
		app.UseAuthorization();
		app.UseAuthentication();
		app.MapControllers();
		app.UseStaticFiles();
		app.UseExceptionHandler("/api/errors");

		app.Run();
	}
}
