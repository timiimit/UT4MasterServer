using System.Text.Json.Nodes;

namespace UT4MasterServer.Models;

public class GameServerAttributes
{
	public const string UT_SERVERTRUSTLEVEL_i = "UT_SERVERTRUSTLEVEL_i";
	public const string UT_SERVERNAME_s = "UT_SERVERNAME_s";

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
		foreach (var attribute in other.serverConfigs)
		{
			if (UnownedAttributeNames.Contains(attribute.Key))
				continue;

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
			return null;
		return serverConfigs[key];
	}

	public string[] GetKeys()
	{
		return serverConfigs.Keys.ToArray();
	}

	public JsonObject ToJObject()
	{
		var attrs = new KeyValuePair<string, JsonNode?>[serverConfigs.Count];

		int i = 0;
		foreach (var kvp in serverConfigs)
		{
			if (kvp.Key.EndsWith("_b"))
			{
				attrs[i] = new(kvp.Key, (bool)kvp.Value);
			}
			else if (kvp.Key.EndsWith("_i"))
			{
				attrs[i] = new(kvp.Key, (int)kvp.Value);
			}
			else if (kvp.Key.EndsWith("_s"))
			{
				attrs[i] = new(kvp.Key, (string)kvp.Value);
			}
			i++;
		}

		return new JsonObject(attrs);
	}

	private void SetDirect(string key, object? value)
	{
		if (value != null)
		{
			if (serverConfigs.ContainsKey(key))
				serverConfigs[key] = value;
			else
				serverConfigs.Add(key, value);
		}
		else
		{
			if (serverConfigs.ContainsKey(key))
				serverConfigs.Remove(key);
		}
	}
}
