using System.Reflection.Metadata.Ecma335;
using System.Xml.Linq;

namespace UT4MasterServer.Xmpp.XmlParser;

public class XmlParserPermissive
{
	private XmlScanner scanner;

	private char[] buffer;
	private int bufferIndex;
	private int bufferReadCount;
	private int scannerState;

	private Task<int>? taskRead;

	public XmlElement Root { get; private set; }
	public TextReader Reader { get; private set; }
	public XmlElement Current { get; private set; }


	public XmlParserPermissive(XmlScanner scanner, TextReader reader)
	{
		this.scanner = scanner;

		buffer = new char[1024];
		bufferIndex = 0;
		bufferReadCount = 0;

		scannerState = 0;

		Reader = reader;
		Current = Root = new XmlElement("#ROOT");
	}

	public async Task ReadElementStartAsync(CancellationToken cancellationToken = default)
	{
		await Task.Yield();

		Read(x => (x & 0b011) != 0, cancellationToken);
	}

	public async Task ReadElementToEndAsync(CancellationToken cancellationToken = default)
	{
		await Task.Yield();

		Read(x => (x & 0b110) != 0, cancellationToken);
	}

	private void Read(Func<int, bool> condition, CancellationToken cancellationToken)
	{
		lock (buffer)
		{
			while (true)
			{
				while (bufferIndex < bufferReadCount)
				{
					scannerState = scanner.NextChar(scannerState, buffer[bufferIndex]);

					if (scannerState > 0)
					{
						// need more chars to read next lexem
					}
					else if (scannerState < 0)
					{
						// detected invalid sequence of chars, try to start fresh
						scannerState = 0;
					}
					else if (scannerState == 0)
					{
						// smallest unit of meaning has been read

						if (scanner.LastLexem == null)
						{
							// LastLexem should never be null here
							throw new NullReferenceException();
						}

						var result = ProcessNextLexem(scanner.LastLexem);
						if (condition(result))
						{
							return;
						}

						// we need to enter same character into the state machine again
						continue;
					}

					bufferIndex++;
				}

				// TODO: optimize reading logic to read while scanning the previous read
				taskRead = Reader.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).AsTask();
				taskRead.Wait(CancellationToken.None);

				bufferIndex = 0;
				bufferReadCount = taskRead.Result;
			}
		}
	}

	private int tokenState = 0;
	private XmlToken lastOpenType;
	private bool hasExitedCurrent = false;

	public int ProcessNextLexem(XmlLexem lexem)
	{
		if (tokenState == 0)
		{
			if (lexem.Token == XmlToken.Open || lexem.Token == XmlToken.OpenProcInst)
			{
				if (hasExitedCurrent)
				{
					hasExitedCurrent = false;

					if (Current.Parent != null)
					{
						Current = Current.Parent;
					}
				}
				var old = Current;
				Current = new XmlElement("#UNKNOWN") { Parent = old };
				old.Elements.Add(Current);

				tokenState = 1;
				lastOpenType = lexem.Token;
				return 0;
			}
			else if (lexem.Token == XmlToken.OpenEnd)
			{
				hasExitedCurrent = true;
				tokenState = 5;
				return 0;
			}
			else
			{
				Current.Elements.Add(new XmlTextElement(lexem.Value));
				return 0;
			}
		}
		else if (tokenState == 1)
		{
			if (lexem.Token == XmlToken.Name)
			{
				Current.Name = lexem.Value;
				tokenState = 2;
				return 0;
			}
		}
		else if (tokenState == 2)
		{
			if (lexem.Token == XmlToken.Name)
			{
				Current.Attributes.Add(new XmlAttribute(lexem.Value, string.Empty));
				tokenState = 3;
				return 0;
			}
			if (lexem.Token == XmlToken.Close || lexem.Token == XmlToken.CloseProcInst)
			{
				if (lastOpenType == XmlToken.OpenProcInst)
				{
					hasExitedCurrent = true;
				}
				tokenState = 0;
				return 1;
			}
			if (lexem.Token == XmlToken.CloseEmpty)
			{
				hasExitedCurrent = true;
				tokenState = 0;
				return 2;
			}
		}
		else if (tokenState == 3)
		{
			if (lexem.Token == XmlToken.Equal)
			{
				tokenState = 4;
				return 0;
			}
		}
		else if (tokenState == 4)
		{
			if (lexem.Token == XmlToken.String)
			{
				Current.Attributes.Last().Value = lexem.Value[1..^1];
				tokenState = 2;
				return 0;
			}
		}
		else if (tokenState == 5)
		{
			if (lexem.Token == XmlToken.OpenEnd)
			{
				hasExitedCurrent = true;
				tokenState = 0;
				return 4;
			}
		}

		return 0;
	}
}
