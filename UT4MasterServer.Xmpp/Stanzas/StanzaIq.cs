using System.Xml;

namespace UT4MasterServer.Xmpp.Stanzas;

public class StanzaIq : Stanza
{
	public enum TypeAttributeValues
	{
		Get,
		Set,
		Result
	}

	public enum RequestKind
	{
		Unknown,
		Bind,
		Session,
		Roster
	}

	public TypeAttributeValues Type { get; set; } = TypeAttributeValues.Result;
	public RequestKind Kind { get; set; } = RequestKind.Unknown;

	public async Task WriteAsync(XmppWriter writer, CancellationToken cancellationToken)
	{
		await Task.Yield();
		throw new NotImplementedException();

		writer.OpenTag("iq");
		writer.Attribute("id", ID);
		if (Error == null)
		{
			writer.Attribute("type", Type.ToString().ToLower());
			{
				switch (Kind)
				{
					case RequestKind.Bind:
					{

						break;
					}
					case RequestKind.Session:
					{
						break;
					}
					case RequestKind.Roster:
					{
						break;
					}

					case RequestKind.Unknown:
					default:
						break;
				}
			}
		}
		else
		{
			writer.Attribute("type", "error");
			{
				await Error.WriteAsync(writer, cancellationToken);
			}
		}
		writer.CloseTag();
	}

	public static async Task<StanzaMessage?> ReadAsync(XmlReader reader, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}
