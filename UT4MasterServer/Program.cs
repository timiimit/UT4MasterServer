using System.Net;

namespace UT4MasterServer
{
	internal class Program
	{
		static void Main(string[] args)
		{
			Server server = new Server();
			server.StartAsync();


			// wait...
			Console.ReadLine();
		}
	}
}