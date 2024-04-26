namespace UT4MasterServer.Xmpp;

public class XmppReader
{
	private class Node
	{
		public string Name { get; set; } = string.Empty;
		public bool HasOpenTagEnded { get; set; } = false;

		public Node(string name)
		{
			Name = name;
		}
	}

	private TextReader r;
	private Stack<Node> elements;
	private char[] rawBuffer;
	private Memory<char> buffer;

	public XmppReader(TextReader reader)
	{
		r = reader;
		elements = new Stack<Node>();
		rawBuffer = new char[1024];
		buffer = new Memory<char>(rawBuffer, 0, rawBuffer.Length);
	}

	private void ParseXml(Span<char> data)
	{

	}


	public async Task InitialStreamHeaderAsync(CancellationToken cancellationToken)
	{
		var readCount = await r.ReadBlockAsync(buffer, cancellationToken);

	}
}
