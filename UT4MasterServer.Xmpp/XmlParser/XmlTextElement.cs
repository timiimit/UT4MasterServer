namespace UT4MasterServer.Xmpp.XmlParser;

public class XmlTextElement : XmlElement
{
	public XmlTextElement(string value) : base("<TEXT>")
	{
		this.value = value;
	}

	public override string ToString()
	{
		return $"<TEXT> = {Value}";
	}
}
