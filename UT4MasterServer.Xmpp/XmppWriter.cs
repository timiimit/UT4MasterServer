using System.Security;
using UT4MasterServer.Xmpp.Stanzas;

namespace UT4MasterServer.Xmpp;

public class XmppWriter
{
	private class StackNode
	{
		public string Name { get; set; } = string.Empty;
		public bool HasOpenTagEnded { get; set; }

		public StackNode(string name)
		{
			Name = name;
		}
	}

	private readonly TextWriter w;
	private readonly Stack<StackNode> elements;

	public XmppWriter(TextWriter writer)
	{
		w = writer;
		elements = new Stack<StackNode>();
	}

	public void Flush()
	{
		w.Flush();
	}

	public void ResponseStreamHeader(string from, string id)
	{
		w.Write($"<?xml version=\"1.0\"?><stream:stream xmlns=\"jabber:client\" xmlns:stream=\"http://etherx.jabber.org/streams\" from=\"{from}\" id=\"{id}\" version=\"1.0\" xml:lang=\"en\">");
	}

	public void EndStream()
	{
		w.Write("</stream>");
	}

	public void OpenTag(string prefix, string localName)
	{
		var name = string.Empty;
		if (prefix is not null)
		{
			name = prefix + ":";
		}

		name += localName;

		OpenTag(name);
	}

	public void OpenTag(string name)
	{
		if (elements.Count > 0)
		{
			StackNode? top = elements.Peek();
			if (!top.HasOpenTagEnded)
			{
				OpenTagEnd();
				top.HasOpenTagEnded = true;
			}
		}

		w.Write($"<{name}");

		elements.Push(new StackNode(name));
	}

	public void OpenTagNS(string prefix, string localName, string ns)
	{
		var name = string.Empty;
		if (prefix is not null)
		{
			name = prefix + ":";
		}

		name += localName;

		OpenTag(name);
	}

	public void OpenTagNS(string name, string ns)
	{
		if (elements.Count > 0)
		{
			StackNode? top = elements.Peek();
			if (!top.HasOpenTagEnded)
			{
				OpenTagEnd();
				top.HasOpenTagEnded = true;
			}
		}

		w.Write($"<{name}");
		Attribute("xmlns", ns);

		elements.Push(new StackNode(name));
	}

	public void OpenTagEnd()
	{
		StackNode? top = elements.Peek();
		if (!top.HasOpenTagEnded)
		{
			w.Write('>');
			top.HasOpenTagEnded = true;
		}
	}

	public void Attribute(string? prefix, string name, string value)
	{
		w.Write(" ");
		if (prefix is not null)
		{
			w.Write($"{prefix}:");
		}

		w.Write($"{name}=\"{value}\"");
	}

	public void Attribute(string name, string value)
	{
		w.Write($" {name}='{value}'");
	}

	public void CloseTag()
	{
		StackNode? top = elements.Pop();
		if (!top.HasOpenTagEnded)
		{
			w.Write("/>");
		}
		else
		{
			w.Write($"</{top.Name}>");
		}
	}

	public void StringTag(string prefix, string localName, string value)
	{
		OpenTag(prefix, localName);
		w.Write(SecurityElement.Escape(value));
		CloseTag();
	}

	public void StringTag(string name, string value)
	{
		OpenTag(name);
		OpenTagEnd();
		w.Write(SecurityElement.Escape(value));
		CloseTag();
	}

	public async Task StanzaAsync(Stanza stanza, CancellationToken cancellationToken)
	{
		if (stanza is StanzaMessage stanzaMessage)
		{
			await stanzaMessage.WriteAsync(this, cancellationToken);
		}
		else if (stanza is StanzaPresence stanzaPresence)
		{
			await stanzaPresence.WriteAsync(this, cancellationToken);
		}
	}
}
