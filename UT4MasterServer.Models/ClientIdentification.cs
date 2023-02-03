using System.Text;
using UT4MasterServer.Common.Helpers;
using UT4MasterServer.Common;

namespace UT4MasterServer.Models;

public class ClientIdentification
{
	public readonly static ClientIdentification Launcher = new(
		EpicID.FromString("34a02cf8f4414e29b15921876da36f9a"),
		"daafbccc737745039dffe53d94fc76cf"
	);
	public readonly static ClientIdentification Game = new(
		EpicID.FromString("1252412dc7704a9690f6ea4611bc81ee"),
		"2ca0c925b4674852bff92b26f8322434"
	);
	public readonly static ClientIdentification ServerInstance = new(
		EpicID.FromString("6ff43e743edc4d1dbac3594877b4bed9"),
		"54619d6f84d443e195200b54ab649a53"
	);

	//public static string GameAuthorization = "MTI1MjQxMmRjNzcwNGE5NjkwZjZlYTQ2MTFiYzgxZWU6MmNhMGM5MjViNDY3NDg1MmJmZjkyYjI2ZjgzMjI0MzQ=";

	public EpicID ID { get; private set; }
	public string Secret { get; private set; }
	public string Authorization { get; private set; }

	public ClientIdentification(string authorization)
	{
		if (authorization.TryDecodeBase64(out var parsedBytes))
		{
			string decoded = Encoding.UTF8.GetString(parsedBytes);
			var colon = decoded.IndexOf(':');
			if (colon >= 0)
			{
				Authorization = authorization;
				ID = EpicID.FromString(decoded.Substring(0, colon));
				Secret = decoded.Substring(colon + 1);
				return;
			}
		}

		// unknown format
		Authorization = authorization;
		ID = EpicID.Empty;
		Secret = string.Empty;
	}

	public ClientIdentification(EpicID id, string secret)
	{
		string auth = $"{id}:{secret}";
		Authorization = Convert.ToBase64String(Encoding.UTF8.GetBytes(auth));
		ID = id;
		Secret = secret;
	}

	public override string ToString()
	{
		return $"{ID}:{Secret}";
	}
}
