namespace UT4MasterServer.Xmpp.XmlParser;

public class XmlAttribute : XmlNode
{
	public string Value { get; set; }

    public XmlAttribute(string name, string value) : base(name)
    {
		Value = value;
	}

	public override string ToString()
	{
		return $"{Name}=\"{Value}\"";
	}
}
