namespace UT4MasterServer.Xmpp.XmlParser;

internal class XmlLexem
{
	XmlToken Token { get; set; }
	string Value { get; set; } = string.Empty;
	int Line { get; set; }
	int Column { get; set; }
}
