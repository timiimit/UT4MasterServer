using System.Xml;

namespace UT4MasterServer.Xmpp.Stanzas;

public abstract class Stanza
{
	public string ID { get; set; } = string.Empty;
	public JID From { get; set; } = JID.Empty;
	public JID To { get; set; } = JID.Empty;
	public StanzaError? Error { get; set; }

	protected static void ReadToEndOfStanza(XmlReader reader)
	{
		while (reader.Depth > 1)
		{
			reader.Read();
		}
	}
}
