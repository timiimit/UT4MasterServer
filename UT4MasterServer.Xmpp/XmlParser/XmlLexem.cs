namespace UT4MasterServer.Xmpp.XmlParser;

internal class XmlLexem
{
	public XmlToken Token { get; set; }
	public string Value { get; set; } = string.Empty;
	public int Line { get; set; }
	public int Column { get; set; }
}
