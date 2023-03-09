namespace UT4MasterServer.Xmpp.XmlParser;

public class XmlNode
{
	public string Prefix { get; set; }
	public string LocalName { get; set; }

	public string Name
	{
		get
		{
			if (string.IsNullOrEmpty(Prefix))
				return LocalName;
			return $"{Prefix}:{LocalName}";
		}

		set
		{
			var splitIndex = value.IndexOf(':');
			if (splitIndex > 0)
			{
				Prefix = value.Remove(splitIndex);
				LocalName = value.Substring(splitIndex + 1);
			}
			else
			{
				Prefix = string.Empty;
				LocalName = value;
			}
		}
	}

    public XmlNode(string name)
    {
		// Prefix and LocalName are set by Name setter
		Prefix = null!;
		LocalName = null!;
		Name = name;
    }

	public override string ToString()
	{
		return $"Xml <{Name}>";
	}
}
