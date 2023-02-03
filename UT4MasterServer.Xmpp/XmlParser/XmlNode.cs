namespace UT4MasterServer.Xmpp.XmlParser;

public class XmlNode
{
    public string Name { get; set; } = string.Empty;
    public bool HasOpenTagEnded { get; set; } = false;

    public XmlNode(string name)
    {
        Name = name;
    }
}
