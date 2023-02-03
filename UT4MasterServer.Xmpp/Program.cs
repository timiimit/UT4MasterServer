namespace UT4MasterServer.Xmpp;

internal class Program
{
	static async Task Main(string[] args)
	{
		var cts = new CancellationTokenSource();
		var server = new XmppServer("master-ut4.timiimit.com");
		var serverTask = server.StartAsync(cts.Token);
		while (true)
		{
			string? command = Console.ReadLine();
			if (command == null)
				continue;

			string[] commandParts = command.Split(' ');

			if (command == "exit")
			{
				break;
			}
			else if (commandParts[0] == "send")
			{
				if (commandParts[1] == "message")
				{
					var connection = await server.FindConnectionAsync(JID.Parse(commandParts[2]));
					if (connection == null)
					{
						Console.WriteLine($"Could not find connection for '{commandParts[2]}'.");
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
