using System.Xml;

namespace UT4MasterServer.Xmpp.Stanzas;

public class StanzaPresence : Stanza
{
	public enum TypeAttributeValues
	{
		None,
		Probe,
		Subscribe,
		Subscribed,
		Unavailable,
		Unsubscribe,
		Unsubscribed,
	}

	public enum ShowElementValues
	{
		Available,
		Chat,
		Away,
		Dnd,
		Xa,
	}

	public TypeAttributeValues Type { get; set; } = TypeAttributeValues.None;
	public ShowElementValues Show { get; set; } = ShowElementValues.Available;
	public string Status { get; set; } = string.Empty;

	public async Task WriteAsync(XmppWriter writer, CancellationToken cancellationToken)
	{
		await Task.Yield();

		writer.OpenTag("presence");
		writer.Attribute("from", From.Full);
		if (To.IsValid)
		{
			writer.Attribute("to", To.Full);
		}
		if (Error == null)
		{
			if (Type != TypeAttributeValues.None)
			{
				writer.Attribute("type", Type.ToString().ToLower());
			}
			if (Type != TypeAttributeValues.Unavailable && Type != TypeAttributeValues.Probe)
			{
				if (Show != ShowElementValues.Available)
				{
					writer.StringTag("show", Show.ToString().ToLower());
				}
				if (!string.IsNullOrEmpty(Status))
				{
					writer.StringTag("status", Status);
				}
			}
		}
		else
		{
			writer.Attribute("type", "error");
			await Error.WriteAsync(writer, cancellationToken);
		}
		writer.CloseTag();
	}

	public static async Task<StanzaPresence?> ReadAsync(XmlReader reader, CancellationToken cancellationToken)
	{
		if (reader.Name != "presence")
			return null;

		try
		{
			var to = reader.GetAttribute("to");
			var from = reader.GetAttribute("from");

			var type = reader.GetAttribute("type");
			if (type == "error")
			{
				var stanzaError = await StanzaError.ReadAsync(reader, cancellationToken);
				if (stanzaError is null)
				{
					return null;
				}

				await reader.ReadAsync();
				if (reader.NodeType != XmlNodeType.EndElement || reader.Name != "presence")
					return null;

				return new StanzaPresence() { From = JID.Parse(from), Error = stanzaError };
			}

			var show = ShowElementValues.Available;
			var status = string.Empty;

			await reader.ReadAsync();
			if (reader.NodeType == XmlNodeType.Element && reader.Name == "show")
			{
				await reader.ReadAsync();
				switch (reader.Value)
				{
					case "chat":
						show = ShowElementValues.Chat;
						break;
					case "away":
						show = ShowElementValues.Away;
						break;
					case "dnd":
						show = ShowElementValues.Dnd;
						break;
					case "xa":
						show = ShowElementValues.Xa;
						break;
					default:
						break;
				}

				await reader.ReadAsync();
				if (reader.NodeType != XmlNodeType.EndElement || reader.Name != "show")
					return null;

				await reader.ReadAsync();
			}

			if (reader.NodeType == XmlNodeType.Element && reader.Name == "status")
			{
				await reader.ReadAsync();
				status = reader.Value;

				await reader.ReadAsync();
				if (reader.NodeType != XmlNodeType.EndElement || reader.Name != "status")
					return null;

				await reader.ReadAsync();
			}

			// NOTE: there might be other custom elements here, let finally block skip those

			return new StanzaPresence() { From = JID.Parse(from), To = JID.Parse(to), Show = show, Status = status };
		}
		finally
		{
			ReadToEndOfStanza(reader);
		}
	}
}
