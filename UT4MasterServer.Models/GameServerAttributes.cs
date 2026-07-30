using System.Text.Json.Nodes;

namespace UT4MasterServer.Models;

public class GameServerAttributes
{
	public const string UT_SERVERTRUSTLEVEL_i = "UT_SERVERTRUSTLEVEL_i";
	public const string UT_SERVERNAME_s = "UT_SERVERNAME_s";
	public const string GAMEMODE_s = "GAMEMODE_s";
	public const string UT_RANKED_i = "UT_RANKED_i";
	public const string UT_GAMEINSTANCE_i = "UT_GAMEINSTANCE_i";

	public static readonly string[] UnownedAttributeNames = new[]
	{
		UT_SERVERTRUSTLEVEL_i,
		//UT_SERVERNAME_s
	};

	private readonly Dictionary<string, object> serverConfigs;

	public GameServerAttributes()
	{
		serverConfigs = new Dictionary<string, object>();
	}

	public void Set(string key, string? value)
	{
		SetDirect(key, value);
	}

	public void Set(string key, int? value)
	{
		SetDirect(key, value);
	}

	public void Set(string key, bool? value)
	{
		SetDirect(key, value);
	}

	public void Update(GameServerAttributes other)
	{
		foreach (KeyValuePair<string, object> attribute in other.serverConfigs)
		{
			if (UnownedAttributeNames.Contains(attribute.Key))
			{
				continue;
			}

			SetDirect(attribute.Key, attribute.Value);
		}
	}

	public bool Contains(string key)
	{
		return serverConfigs.ContainsKey(key);
	}

	public object? Get(string key)
	{
		if (!Contains(key))
		{
			return null;
		}

		return serverConfigs[key];
	}

	public string[] GetKeys()
	{
		return serverConfigs.Keys.ToArray();
	}

	public JsonObject ToJObject()
	{
		var attrs = new List<KeyValuePair<string, JsonNode?>>(serverConfigs.Count);

		foreach (KeyValuePair<string, object> kvp in serverConfigs)
		{
			// emit based on the actual stored type. keys without a known type
			// suffix or with a value that mismatches their suffix used to leave
			// a null-key entry or throw an invalid cast, crashing serialization
			// of the entire server list.
			if (kvp.Value is bool valueBool)
			{
				attrs.Add(new(kvp.Key, valueBool));
			}
			else if (kvp.Value is int valueInt)
			{
				attrs.Add(new(kvp.Key, valueInt));
			}
			else if (kvp.Value is string valueString)
			{
				attrs.Add(new(kvp.Key, valueString));
			}
		}

		return new JsonObject(attrs);
	}

	private void SetDirect(string key, object? value)
	{
		if (value != null)
		{
			if (serverConfigs.ContainsKey(key))
			{
				serverConfigs[key] = value;
			}
			else
			{
				serverConfigs.Add(key, value);
			}
		}
		else
		{
			if (serverConfigs.ContainsKey(key))
			{
				serverConfigs.Remove(key);
			}
		}
	}
}
