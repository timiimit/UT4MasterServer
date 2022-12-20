using System.Text;

namespace UT4MasterServer.Authorization;

public class ClientIdentification
{
	public static ClientIdentification Launcher = new ClientIdentification(new EpicID("34a02cf8f4414e29b15921876da36f9a"), new EpicID("daafbccc737745039dffe53d94fc76cf"));
	public static ClientIdentification Game = new ClientIdentification(new EpicID("1252412dc7704a9690f6ea4611bc81ee"), new EpicID("2ca0c925b4674852bff92b26f8322434"));

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
		ID = new EpicID(decoded.Substring(0, colon));
		Secret = new EpicID(decoded.Substring(colon + 1));
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
