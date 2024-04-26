namespace UT4MasterServer.Xmpp;

public class JID
{
	public static readonly JID Empty = new(string.Empty);

	public string Username { get; set; }
	public string Domain { get; set; }
	public string Resource { get; set; }

	public string Full
	{
		get
		{
			if (string.IsNullOrEmpty(Username))
			{
				return Domain; // When username is empty so is Resource
			}

			if (string.IsNullOrEmpty(Resource))
			{
				return $"{Username}@{Domain}"; // Domain must always be set
			}

			return $"{Username}@{Domain}/{Resource}";
		}
	}

	public string Bare
	{
		get
		{
			if (string.IsNullOrEmpty(Username))
			{
				return Domain; // When username is empty, so is Resource
			}

			return $"{Username}@{Domain}"; // Domain must always be set
		}
	}

	public bool HasResource => string.IsNullOrEmpty(Resource);
	public bool HasUsername => string.IsNullOrEmpty(Username);

	public bool IsValid => !string.IsNullOrEmpty(Domain);

	public JID(string username, string domain, string resource)
	{
		Username = username;
		Domain = domain;
		Resource = resource;
	}

	public JID(string username, string domain) : this(username, domain, string.Empty)
	{
	}

	public JID(string domain) : this(string.Empty, domain, string.Empty)
	{
	}

	public static JID Parse(string? jidString)
	{
		if (jidString == null)
		{
			return new JID(string.Empty);
		}

		var at = jidString.IndexOf('@');
		if (at < 0)
		{
			// this is just domain
			return new JID(jidString);
		}

		var slash = jidString.IndexOf('/', at + 1);
		if (slash < 0)
		{
			// resource is empty
			return new JID(jidString.Substring(0, at), jidString.Substring(at + 1));
		}

		// all parts are present
		return new JID(jidString.Substring(0, at), jidString.Substring(at + 1, slash - at - 1), jidString.Substring(slash + 1));
	}

	public override string ToString()
	{
		return Full;
	}

	public override bool Equals(object? obj)
	{
		if (obj == null)
		{
			return false;
		}

		if (obj is not JID jid)
		{
			return false;
		}

		if (HasResource && jid.HasResource)
		{
			return Full == jid.Full;
		}

		return Bare == jid.Bare;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Username, Domain, Resource);
	}
}
