using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using UT4MasterServer.Common;
using UT4MasterServer.Models.Database;
using UT4MasterServer.Services.Scoped;
using UT4MasterServer.Xmpp.Stanzas;

namespace UT4MasterServer.Xmpp;

public class XmppServer
{
	private readonly TcpListener listener;
	private readonly List<XmppConnection> connections;
	private readonly FriendService? friendService;

	private DateTimeOffset lastAcceptTime;
	private readonly TimeSpan onlyAcceptEvery = TimeSpan.FromSeconds(1);

	public string Domain { get; private set; }
	public int Port { get; private set; }
	public JID AdminID { get; private set; }
	public X509Certificate2 Certificate { get; private set; }

 public XmppServer(string domain, X509Certificate2 certificate, FriendService? friendService = null, int port = 5222)
	{
		Domain = domain;
		Port = port;
		AdminID = new JID("xmpp-admin", Domain);
		Certificate = certificate;
		this.friendService = friendService;

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
				TcpClient? tcpConnection = await listener.AcceptTcpClientAsync(cancellationToken);
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
			foreach (XmppConnection? connection in connections)
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
			foreach (XmppConnection? connection in connections)
			{
				if (connection.ID.Equals(presence.From))
				{
					continue;
				}

				// TODO: only send presence to friends
				connection.QueueStanza(presence);
			}
		}

		return true;
	}

	public async Task SendPresenceOfFriendsAsync(XmppConnection connection)
	{
		await Task.Yield();

		foreach (XmppConnection? c in connections)
		{
			if (ReferenceEquals(c, connection))
			{
				continue;
			}

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
			foreach (XmppConnection? connection in connections)
			{
				if (connection.ID.Equals(jid))
				{
					return connection;
				}
			}
		}

		return null;
	}

	public async Task<IReadOnlyList<JID>> GetRosterItemsAsync(JID requestingJid)
	{
     if (friendService == null)
		{
			return Array.Empty<JID>();
		}

		EpicID requestingAccountID;
		try
		{
			requestingAccountID = EpicID.FromString(requestingJid.Username);
		}
		catch
		{
			return Array.Empty<JID>();
		}

		List<FriendRequest> friends = await friendService.GetFriendsAsync(requestingAccountID);
		HashSet<string> acceptedFriendIDs = friends
			.Where(friend => friend.Status == FriendStatus.Accepted)
			.Select(friend => (friend.Sender == requestingAccountID ? friend.Receiver : friend.Sender).ToString())
			.ToHashSet(StringComparer.OrdinalIgnoreCase);

		await Task.Yield();

		lock (connections)
		{
			return connections
				.Where(connection => connection.IsStreamAuthenticated)
				.Select(connection => connection.ID)
             .Where(id => id.Bare != requestingJid.Bare)
				.Where(id => acceptedFriendIDs.Contains(id.Username))
				.GroupBy(id => id.Bare, StringComparer.OrdinalIgnoreCase)
				.Select(group => new JID(group.First().Username, group.First().Domain))
				.Cast<JID>()
				.ToList();
		}
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
		{
			return;
		}

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
