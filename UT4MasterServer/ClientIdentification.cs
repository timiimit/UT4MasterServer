using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UT4MasterServer
{
	public class ClientIdentification
	{
		public static ClientIdentification Launcher = new ClientIdentification("34a02cf8f4414e29b15921876da36f9a", "daafbccc737745039dffe53d94fc76cf");
		public static ClientIdentification Game = new ClientIdentification("1252412dc7704a9690f6ea4611bc81ee", "2ca0c925b4674852bff92b26f8322434");

		//public static string GameAuthorization = "MTI1MjQxMmRjNzcwNGE5NjkwZjZlYTQ2MTFiYzgxZWU6MmNhMGM5MjViNDY3NDg1MmJmZjkyYjI2ZjgzMjI0MzQ=";


		public string ID { get; private set; }
		public string Secret { get; private set; }
		public string Authorization { get; private set; }

		public ClientIdentification(string authorization)
		{
			string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(authorization));
			var colon = decoded.IndexOf(':');
			if (colon < 0)
				throw new ArgumentException("authorization is not in expected format");

			Authorization = authorization;
			ID = decoded.Substring(0, colon);
			Secret = decoded.Substring(colon + 1);
		}

		public ClientIdentification(string id, string secret)
		{
			string auth = $"{id}:{secret}";
			Authorization = Convert.ToBase64String(Encoding.UTF8.GetBytes(auth));
			ID = id;
			Secret = secret;
		}
	}
}
