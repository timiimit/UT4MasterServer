using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using UT4MasterServer.Xmpp.Stanzas;

namespace UT4MasterServer.Xmpp;

public class XmppServer
{
	private TcpListener listener;
	private List<XmppConnection> connections;

	private DateTimeOffset lastAcceptTime = default;
	private readonly TimeSpan onlyAcceptEvery = TimeSpan.FromSeconds(1);

	public string Domain { get; private set; }
	public int Port { get; private set; }
	public JID AdminID { get; private set; }
	public X509Certificate2 Certificate { get; private set; }

	public XmppServer(string domain, X509Certificate2 certificate, int port = 5222)
	{
		Domain = domain;
		Port = port;
		AdminID = new JID("xmpp-admin", Domain);
		Certificate = certificate;

		listener = new TcpListener(IPAddress.Any, Port);
		connections = new List<XmppConnection>();
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		listener.Start(8);

		Console.WriteLine("Started listening for XMPP connections on port {PortNumber}", Port);

		try
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				var tcpConnection = await listener.AcceptTcpClientAsync(cancellationToken);
				_ = OnConnectionAcceptedAsync(tcpConnection, cancellationToken);
			}
		}
		finally
		{
			Console.WriteLine("Stopped listening for XMPP connections");

			listener.Stop();
		}

	}

	public async Task<bool> SendMessageAsync(StanzaMessage message)
	{
		await Task.Yield();

		lock (connections)
		{
			foreach (var connection in connections)
			{
				if (connection.ID.Equals(message.To))
				{
					connection.QueueStanza(message);
				}
			}
		}
		return true;
	}

	public async Task<bool> BroadcastPresenceAsync(StanzaPresence presence)
	{
		await Task.Yield();

		lock (connections)
		{
			foreach (var connection in connections)
			{
				if (connection.ID.Equals(presence.From))
					continue;

				// TODO: only send presence to friends
				connection.QueueStanza(presence);
			}
		}

		return true;
	}

	public async Task SendPresenceOfFriendsAsync(XmppConnection connection)
	{
		await Task.Yield();

		foreach (var c in connections)
		{
			if (ReferenceEquals(c, connection))
				continue;

			// TODO: check if connection is a friend
			var presenceProbe = new StanzaPresence()
			{
				//Type = StanzaPresence.TypeAttributeValues.Probe,
				From = c.ID,
				To = connection.ID,
				Show = connection.Show,
				Status = connection.Status,
			};
			connection.QueueStanza(presenceProbe);
		}
	}

	public async Task SendSystemMessageAsync(XmppConnection connection, string message)
	{
		await Task.Yield();

		connection.QueueStanza(new StanzaMessage()
		{
			From = new JID("xmpp-admin", Domain),
			To = connection.ID,
			Body = message
		});
	}

	public async Task<XmppConnection?> FindConnectionAsync(JID jid)
	{
		await Task.Yield();

		lock (connections)
		{
			foreach (var connection in connections)
			{
				if (connection.ID.Equals(jid))
					return connection;
			}
		}

		return null;
	}

	private async Task OnConnectionAcceptedAsync(TcpClient tcpConnection, CancellationToken cancellationToken)
	{
		if (DateTimeOffset.UtcNow < lastAcceptTime + onlyAcceptEvery)
		{
			lastAcceptTime = DateTimeOffset.UtcNow;
			return;
		}

		Console.WriteLine("Client connected");

		if (cancellationToken.IsCancellationRequested)
			return;

		XmppConnection? connection = null;
		try
		{
			connection = new(this, tcpConnection.GetStream());
			lock (connections)
			{
				connections.Add(connection);
			}
			await connection.HandleXmppStreamAsync(cancellationToken);
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex);
		}
		finally
		{
			if (connection is not null)
			{
				connection.Dispose();
				lock (connections)
				{
					connections.Remove(connection);
				}
			}
		}
	}
}
