using System.Xml;

namespace UT4MasterServer.Xmpp.Stanzas;

public class StanzaMessage : Stanza
{
	public enum TypeAttributeValues
	{
		Chat,
		GroupChat,
		Headline,
		Normal
	}

	public TypeAttributeValues Type { get; set; } = TypeAttributeValues.Normal;
	public string Body { get; set; } = string.Empty;

	public async Task WriteAsync(XmppWriter writer, CancellationToken cancellationToken)
	{
		await Task.Yield();

		writer.OpenTag("message");
		if (!string.IsNullOrWhiteSpace(ID))
		{
			writer.Attribute("id", ID);
		}
		writer.Attribute("from", From.Full);
		writer.Attribute("to", To.Full);
		if (Error == null)
		{
			if (Type != TypeAttributeValues.Normal)
			{
				writer.Attribute("type", Type.ToString().ToLower());
			}

			{
				writer.StringTag("body", Body);
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
		if (reader.Name != "message")
		{
			return null;
		}

		await Task.Yield();

		try
		{
			var id = reader.GetAttribute("id") ?? string.Empty;
			var to = JID.Parse(reader.GetAttribute("to"));
			if (!to.IsValid)
			{
				return null;
			}

			// allow a missing 'from' attribute because we will replace existing
			// one anyway so people cannot spoof origin of a message.
			var from = JID.Parse(reader.GetAttribute("from"));

			var type = reader.GetAttribute("type");
			if (type == "error")
			{
				var stanzaError = await StanzaError.ReadAsync(reader, cancellationToken);
				if (stanzaError == null)
				{
					return null;
				}

				reader.Read();
				if (reader.Name != "message" || reader.NodeType != XmlNodeType.EndElement)
				{
					return null;
				}

				return new StanzaMessage() { ID = id, From = from, To = to, Error = stanzaError };
			}

			var typeAttribute = TypeAttributeValues.Normal;
			var typeNames = Enum.GetNames<TypeAttributeValues>();
			var typeValues = Enum.GetValues<TypeAttributeValues>();
			for (var i = 0; i < typeNames.Length; i++)
			{
				if (type == typeNames[i].ToLower())
				{
					typeAttribute = typeValues[i];
					break;
				}
			}

			reader.Read();
			if (reader.Name != "body")
			{
				return null;
			}

			reader.Read();
			var body = reader.Value;

			reader.Read();
			if (reader.Name != "body" || reader.NodeType != XmlNodeType.EndElement)
			{
				return null;
			}

			reader.Read();
			if (reader.Name != "message" || reader.NodeType != XmlNodeType.EndElement)
			{
				return null;
			}

			return new StanzaMessage() { ID = id, Type = typeAttribute, From = from, To = to, Body = body };
		}
		finally
		{
			ReadToEndOfStanza(reader);
		}
	}
}
