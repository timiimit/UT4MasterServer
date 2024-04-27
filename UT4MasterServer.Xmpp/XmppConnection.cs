using System.Net.Security;
using System.Security.Authentication;
using System.Text;
using System.Xml;
using UT4MasterServer.Xmpp.Stanzas;

namespace UT4MasterServer.Xmpp;

public class XmppConnection : IDisposable
{
	private JID? id;

	private readonly XmppServer server;
	private Stream stream;
	private XmlReader reader;
	private XmppWriter writer;
	private bool disposedValue;

	public JID ID
	{
		get
		{
			if (id is null)
			{
				throw new InvalidOperationException();
			}
			return id;
		}
	}
	public StanzaPresence.ShowElementValues Show { get; set; }
	public string Status { get; set; }

	public XmppServer Server => server;
	public Stream Stream => stream;
	public XmlReader Reader => reader;
	public XmppWriter Writer => writer;

	public bool IsStreamEncrypted { get; private set; }
	public bool IsStreamCompressed { get; private set; }
	public bool IsStreamAuthenticated { get; private set; }
	public Queue<Stanza> QueuedStanzas { get; private set; }

	public XmppConnection(XmppServer server, Stream stream)
	{
		id = null;
		Show = StanzaPresence.ShowElementValues.Available;
		Status = string.Empty;
		this.server = server;
		QueuedStanzas = new Queue<Stanza>();

		// these are set in SetStream()
		this.stream = null!;
		reader = null!;
		writer = null!;

		SetStream(stream);
	}

	private void SetStream(Stream stream)
	{
		// remember stream
		this.stream = stream;

		// create a proxy stream to view transfered data
		Stream tmpStream = new DebugTextProxyStream(Stream, leaveOpen: true);

		// create reader (safe to not close or dispose) for new stream
		var rawReader = new StreamReader(tmpStream);
		reader = XmlReader.Create(rawReader, new XmlReaderSettings() { Async = true, IgnoreWhitespace = true });

		// create writer (safe to not close or dispose) for new stream
		var rawWriter = new StreamWriter(tmpStream);
		writer = new(rawWriter);
	}

	public async Task HandleXmppStreamAsync(CancellationToken cancellationToken)
	{
		await HandleStreamNegotiationAsync(cancellationToken);
	}

	public void QueueStanza(Stanza stanza)
	{
		lock (writer)
		{
			QueuedStanzas.Enqueue(stanza);
		}
	}

	private async Task<Guid?> HandleStreamNegotiationAsync(CancellationToken cancellationToken)
	{
		await Task.Yield();

	LABEL_StreamStart:

		await Reader.ReadAsync();
		await Reader.ReadAsync();
		if (Reader.LocalName != "stream")
		{
			return null;
		}

		var guid = Guid.NewGuid();

		Writer.ResponseStreamHeader(Server.Domain, guid.ToString());
		Writer.Flush();

		WriteAvailableFeatures();
		Writer.Flush();

		await Reader.ReadAsync();

		if (Reader.Name == "starttls")
		{
			if (!IsStreamEncrypted)
			{
				if (!EncryptStreamSSL())
				{
					return null;
				}

				goto LABEL_StreamStart;
			}
		}
		else if (Reader.Name == "compress")
		{
			if (IsStreamCompressed)
			{
				return null;
			}

			if (!CompressStreamZLib())
			{
				return null;
			}

			goto LABEL_StreamStart;
		}
		else if (Reader.Name == "auth")
		{
			if (IsStreamAuthenticated)
			{
				return null;
			}

			var username = AuthenticateStream();
			if (username is not null)
			{
				id = new JID(username, Server.Domain);
				goto LABEL_StreamStart;
			}
			else
			{
				// TODO: send error
				return null;
			}
		}
		else
		{
			if (IsStreamAuthenticated)
			{
				try
				{
					await HandleStanzasAsync(cancellationToken);
					Writer.EndStream();
					Writer.Flush();
				}
				finally
				{
					await Server.BroadcastPresenceAsync(new StanzaPresence
					{
						Type = StanzaPresence.TypeAttributeValues.Unavailable,
						From = ID
					});
				}

				return guid;
			}
			else
			{
				return null;
			}
		}

		return null;
	}

	private async Task HandleStanzasAsync(CancellationToken cancellationToken)
	{
		await Task.Yield();

		var recievedPresenceOfFriends = false;

		while (!cancellationToken.IsCancellationRequested)
		{
			// if we reach the end of stream, break
			if (Reader.Depth == 0 && Reader.NodeType == XmlNodeType.EndElement && Reader.LocalName == "stream")
			{
				break;
			}

			if (Reader.Name == "iq")
			{
				await HandleStanzaIqAsync(cancellationToken);
			}
			else if (Reader.Name == "message")
			{
				StanzaMessage? stanza = await StanzaMessage.ReadAsync(Reader, cancellationToken);
				if (stanza is not null)
				{
					stanza = new StanzaMessage
					{
						ID = stanza.ID,
						From = ID,
						To = stanza.To,
						Type = stanza.Type,
						Body = stanza.Body
					};
					await Server.SendMessageAsync(stanza);
				}
			}
			else if (Reader.Name == "presence")
			{
				StanzaPresence? stanza = await StanzaPresence.ReadAsync(Reader, cancellationToken);
				if (stanza is not null)
				{
					Show = stanza.Show;
					Status = stanza.Status;
					stanza = new StanzaPresence
					{
						//ID = stanza.ID,
						From = ID,
						To = stanza.To,
						Type = stanza.Type,
						Show = stanza.Show,
						Status = stanza.Status
					};
					await Server.BroadcastPresenceAsync(stanza);

					if (!recievedPresenceOfFriends)
					{
						recievedPresenceOfFriends = true;
						await Server.SendPresenceOfFriendsAsync(this);
					}
				}
			}
			else
			{
				// ill-formed xml. try to keep connection alive
			}

			Task<bool>? readTask = Reader.ReadAsync();

			bool didReadNewData;
			do
			{
				await HandleQueuedAsync(cancellationToken);

				didReadNewData = readTask.Wait(50, cancellationToken);
			}
			while (!didReadNewData);
		}
	}

	private async Task HandleQueuedAsync(CancellationToken cancellationToken)
	{
		var dequeuedStanzas = new List<Stanza>();
		lock (writer)
		{
			foreach (Stanza? stanza in QueuedStanzas)
			{
				dequeuedStanzas.Add(stanza);
			}
			QueuedStanzas.Clear();
		}

		foreach (Stanza? stanza in dequeuedStanzas)
		{
			await Writer.StanzaAsync(stanza, cancellationToken);
#if DEBUG
			Writer.Flush();
#endif
		}

#if !DEBUG
		Writer.Flush();
#endif
	}

	private async Task HandleStanzaIqAsync(CancellationToken cancellationToken)
	{
		await Task.Yield();

		var id = Reader.GetAttribute("id");
		var type = Reader.GetAttribute("type");
		//var from = Reader.GetAttribute("from");
		//var to = Reader.GetAttribute("to");

		Writer.OpenTag("iq");

		if (id is null || type is null)
		{
			// iq is required to contain 'id' and 'type' attribute by definition
			Writer.Attribute("type", "error");
			{
				var err = new StanzaError(StanzaError.ConditionValues.BadRequest);
				await err.WriteAsync(Writer, cancellationToken);
			}
			Writer.CloseTag();
			Writer.Flush();

			await ReadToEndOfStanzaAsync();
			return;
		}

		// all iq response stanzas are required to set the same 'id' attribute as request stanza
		Writer.Attribute("id", id);

		Reader.Read();
		if (Reader.Name == "bind" && type == "set")
		{
			string resource;
			if (Reader.IsEmptyElement)
			{
				// generate resource
				resource = $"V3:UnrealTournamentDev:GEN::{Guid.NewGuid():N}";
			}
			else
			{
				Reader.Read();
				if (Reader.Name == "resource")
				{
					Reader.Read();
					resource = Reader.Value;
					Writer.Attribute("type", "result");
				}
				else
				{
					Writer.Attribute("type", "error");
					{
						var err = new StanzaError(StanzaError.ConditionValues.BadRequest);
						await err.WriteAsync(Writer, cancellationToken);
					}
					Writer.CloseTag();
					Writer.Flush();

					await ReadToEndOfStanzaAsync();
					return;
				}
			}

			ID.Resource = resource;

			Writer.OpenTagNS("bind", "urn:ietf:params:xml:ns:xmpp-bind");
			{
				Writer.StringTag("jid", ID.Full);
			}
			Writer.CloseTag();
		}
		else if (Reader.Name == "session" && type == "set")
		{
			Writer.Attribute("to", ID.Full);
			Writer.Attribute("type", "result");
		}
		else if (Reader.Name == "ping" && type == "get")
		{
			Writer.Attribute("to", ID.Full);
			Writer.Attribute("from", Server.Domain);
			Writer.Attribute("type", "result");
		}
		else
		{
			Writer.Attribute("type", "error");

			var err = new StanzaError(StanzaError.ConditionValues.FeatureNotImplemented);
			await err.WriteAsync(Writer, cancellationToken);
		}

		Writer.CloseTag();
		Writer.Flush();

		// read any remainer left off
		while (Reader.Depth > 1 || Reader.NodeType != XmlNodeType.EndElement)
		{
			Reader.Read();
		}
	}

	private async Task ReadToEndOfStanzaAsync()
	{
		while (Reader.Depth > 1)
		{
			await Reader.ReadAsync();
		}
	}

	private bool EncryptStreamSSL()
	{
		Writer.OpenTagNS("proceed", "urn:ietf:params:xml:ns:xmpp-tls"); Writer.CloseTag();
		Writer.Flush();

		var tmp = new SslStream(Stream, true); //, SSLCertificateValidation, SSLCertificateSelection, EncryptionPolicy.AllowNoEncryption);
		try
		{
			tmp.AuthenticateAsServer(Server.Certificate);
			SetStream(tmp);
			IsStreamEncrypted = true;

			// restart stream
			Console.WriteLine("Client enabled TLS encryption");
			return true;
		}
		catch (AuthenticationException ex)
		{
			// failed to authenticate. continue to close the stream.
			Console.WriteLine(ex.ToString());
		}
		return false;
	}

	private bool CompressStreamZLib()
	{
		Writer.OpenTagNS("compressed", "http://jabber.org/protocol/compress"); Writer.CloseTag();
		Writer.Flush();

		throw new NotImplementedException();
		/*SetStream(new ZLibStream(Stream, CompressionMode.Decompress, true));
		IsStreamCompressed = true;

		// restart stream
		Console.WriteLine("Client enabled zlib compression");
		return true;*/
	}

	private string? AuthenticateStream()
	{
		var mechanism = Reader.GetAttribute("mechanism");
		if (mechanism != "PLAIN")
		{
			return null;
		}

		// read content of <auth>
		Reader.Read();

		var authSeparatorIndex = 0;
		var bytes = Convert.FromBase64String(Reader.Value);
		for (var i = 1; i < bytes.Length; i++)
		{
			if (bytes[i] == 0)
			{
				authSeparatorIndex = i;
				break;
			}
		}

		// read </auth>
		Reader.Read();

		if (authSeparatorIndex <= 0)
		{
			return null;
		}

		var username = Encoding.UTF8.GetString(bytes, 1, authSeparatorIndex - 1);
		//var password = Encoding.UTF8.GetString(bytes, authSeparatorIndex + 1, bytes.Length - (authSeparatorIndex + 1));

		// TODO: validate credentials

		Writer.OpenTagNS("success", "urn:ietf:params:xml:ns:xmpp-sasl");
		Writer.CloseTag();
		Writer.Flush();

		// reset stream
		SetStream(Stream);

		IsStreamAuthenticated = true;
		return username;
	}

	private void WriteAvailableFeatures()
	{
		Writer.OpenTag("stream", "features");
		{
			if (!IsStreamAuthenticated)
			{
				Writer.OpenTagNS("mechanisms", "urn:ietf:params:xml:ns:xmpp-sasl");
				if (IsStreamEncrypted)
				{
					Writer.StringTag("mechanism", "PLAIN");
				}
				Writer.CloseTag();
			}

			Writer.OpenTagNS("ver", "urn:xmpp:features:rosterver"); Writer.CloseTag();

			if (!IsStreamEncrypted)
			{
				Writer.OpenTagNS("starttls", "urn:ietf:params:xml:ns:xmpp-tls");
				{
					Writer.OpenTag("required"); Writer.CloseTag();
				}
				Writer.CloseTag();
			}

			if (!IsStreamCompressed && false)
			{
				// TODO: implement zlib compression
				Writer.OpenTagNS("compression", "http://jabber.org/features/compress");
				{
					Writer.StringTag("method", "zlib");
				}
				Writer.CloseTag();
			}

			if (IsStreamEncrypted && !IsStreamAuthenticated)
			{
				Writer.OpenTagNS("auth", "http://jabber.org/features/iq-auth"); Writer.CloseTag();
			}

			if (IsStreamAuthenticated)
			{
				Writer.OpenTagNS("bind", "urn:ietf:params:xml:ns:xmpp-bind"); Writer.CloseTag();
				Writer.OpenTagNS("session", "urn:ietf:params:xml:ns:xmpp-session"); Writer.CloseTag();
			}
		}
		Writer.CloseTag();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				stream.Dispose();
			}

			disposedValue = true;
		}
	}

	/*private X509Certificate SSLCertificateSelectionCallback(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate? remoteCertificate, string[] acceptableIssuers)
	{
		return localCertificates[0];
	}

	private static bool SSLCertificateValidationCallback(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
	{
		return true;
	}*/

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
