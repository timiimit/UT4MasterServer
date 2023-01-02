using System.Text;
using UT4MasterServer.Other;

namespace UT4MasterServer.Authentication;

public class ClientIdentification
{
	public static ClientIdentification Launcher = new ClientIdentification(
		EpicID.FromString("34a02cf8f4414e29b15921876da36f9a"),
		EpicID.FromString("daafbccc737745039dffe53d94fc76cf")
	);
	public static ClientIdentification Game = new ClientIdentification(
		EpicID.FromString("1252412dc7704a9690f6ea4611bc81ee"),
		EpicID.FromString("2ca0c925b4674852bff92b26f8322434")
	);
	public static ClientIdentification ServerInstance = new ClientIdentification(
		EpicID.FromString("6ff43e743edc4d1dbac3594877b4bed9"),
		EpicID.FromString("54619d6f84d443e195200b54ab649a53")
	);

	//public static string GameAuthorization = "MTI1MjQxMmRjNzcwNGE5NjkwZjZlYTQ2MTFiYzgxZWU6MmNhMGM5MjViNDY3NDg1MmJmZjkyYjI2ZjgzMjI0MzQ=";

	public EpicID ID { get; private set; }
	public EpicID Secret { get; private set; }
	public string Authorization { get; private set; }

	public ClientIdentification(string authorization)
	{
		string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(authorization));
		var colon = decoded.IndexOf(':');
		if (colon < 0)
		{
			// unknown format
			Authorization = authorization;
			ID = EpicID.Empty;
			Secret = EpicID.Empty;
			return;
		}

		Authorization = authorization;
		ID = EpicID.FromString(decoded.Substring(0, colon));
		Secret = EpicID.FromString(decoded.Substring(colon + 1));
	}

	public ClientIdentification(EpicID id, EpicID secret)
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
