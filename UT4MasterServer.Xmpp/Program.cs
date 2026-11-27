using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using UT4MasterServer.Models.Settings;
using UT4MasterServer.Services;
using UT4MasterServer.Services.Scoped;

namespace UT4MasterServer.Xmpp;

internal class Program
{
	private static async Task Main(string[] args)
	{
		//XmlParser.XmlScanner scanner = new();
		//XmlParser.XmlParserPermissive parsers = new(scanner, File.OpenText("../../../example.xml"));
		//while (true)
		//{
		//	await parsers.ReadElementStartAsync(CancellationToken.None);
		//	Console.WriteLine(parsers.Current.ToString());
		//}

		var cts = new CancellationTokenSource();

		var cert = new X509Certificate2("../../../Certs/master-ut4.pfx", "");

		FriendService? friendService = null;
		SessionService? sessionService = null;
		string? dbConnectionString = Environment.GetEnvironmentVariable("UT4MS_DB_CONNECTION_STRING");
		string? dbName = Environment.GetEnvironmentVariable("UT4MS_DB_NAME");
		if (!string.IsNullOrWhiteSpace(dbConnectionString) && !string.IsNullOrWhiteSpace(dbName))
		{
			var settings = Options.Create(new ApplicationSettings
			{
				DatabaseConnectionString = dbConnectionString,
				DatabaseName = dbName
			});
			var dbContext = new DatabaseContext(settings);

			friendService = new FriendService(dbContext);
			sessionService = new SessionService(dbContext);
		}
		else
		{
			// DB access is required for both accepted-friends roster filtering and token validation.
			Console.WriteLine("UT4MS_DB_CONNECTION_STRING and UT4MS_DB_NAME are not set. Roster responses will be empty and XMPP auth will fail.");
		}

		// Default to Epic production XMPP domain unless deployment overrides it.
		string xmppDomain = Environment.GetEnvironmentVariable("UT4MS_XMPP_DOMAIN") ?? "prod.ol.epicgames.com";

		var server = new XmppServer(xmppDomain, cert, friendService, sessionService);
		Task? serverTask = server.StartAsync(cts.Token);
		while (true)
		{
			var command = Console.ReadLine();
			if (command == null)
			{
				continue;
			}

			var commandParts = command.Split(' ');

			if (command == "exit")
			{
				break;
			}

			if (commandParts[0] == "send")
			{
				if (commandParts[1] == "message")
				{
					XmppConnection? connection = await server.FindConnectionAsync(JID.Parse(commandParts[2]));
					if (connection == null)
					{
						Console.WriteLine("Could not find connection for '{JID}'.", commandParts[2]);
						continue;
					}

					await server.SendSystemMessageAsync(connection, commandParts[3]);
				}
				else if (commandParts[1] == "presence")
				{

				}
			}
		}

		cts.Cancel();
		serverTask.Wait();
	}
}
