using System.Text;

namespace UT4MasterServer.Xmpp.XmlParser;

public class XmlElement : XmlNode
{
	protected string? value;
	protected XmlElement? parent;
	protected int depth;

	public List<XmlAttribute> Attributes { get; set; }
	public List<XmlElement> Elements { get; set; }

	public XmlElement? Parent
	{
		get => parent;
		set
		{
			parent = value;
			if (parent != null)
			{
				depth = parent.Depth + 1;
			}
			else
			{
				depth = 0;
			}
		}
	}

	public string Value
	{
		get
		{
			if (value == null)
			{
				return string.Empty;
			}

			return value;
		}
	}

	public int Depth
	{
		get
		{
			return depth;
		}
	}

	public XmlElement(string name) : base(name)
	{
		value = null;
		parent = null;
		depth = 0;

		Attributes = new List<XmlAttribute>();
		Elements = new List<XmlElement>();
	}

	public override string ToString()
	{
		if (Attributes.Count == 0)
		{
			return $"<{Name}>";
		}

		var sb = new StringBuilder($"<{Name}");
		foreach (XmlAttribute? attr in Attributes)
		{
			sb.Append($" {attr}");
		}
		sb.Append('>');
		return sb.ToString();
	}
}
