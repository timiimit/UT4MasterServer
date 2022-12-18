using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.IdGenerators;
using UT4MasterServer;
using UT4MasterServer.Models;
using UT4MasterServer.Services;

namespace UT4MasterServer
{
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
			builder.Services.AddSingleton<AccountService>();
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
			app.MapControllers();

			app.Run();
		}
	}
}