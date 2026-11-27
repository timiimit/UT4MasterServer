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
	private readonly SessionService? sessionService;

	private DateTimeOffset lastAcceptTime;
	private readonly TimeSpan onlyAcceptEvery = TimeSpan.FromSeconds(1);

	public string Domain { get; private set; }
	public int Port { get; private set; }
	public JID AdminID { get; private set; }
	public X509Certificate2 Certificate { get; private set; }

	public XmppServer(string domain, X509Certificate2 certificate, FriendService? friendService = null, SessionService? sessionService = null, int port = 5222)
	{
		Domain = domain;
		Port = port;
		AdminID = new JID("xmpp-admin", Domain);
		Certificate = certificate;
		this.friendService = friendService;
		this.sessionService = sessionService;

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

	public async Task<string?> AuthenticateClientAsync(string login, string accessToken)
	{
		if (sessionService == null)
		{
			return null;
		}

      // Enforce account@domain login identity so clients follow Epic-style addressing.
     if (!login.Contains('@'))
		{
            return null;
		}

		JID jid = JID.Parse(login);
		if (string.IsNullOrWhiteSpace(jid.Username) || !string.Equals(jid.Domain, Domain, StringComparison.OrdinalIgnoreCase))
		{
			return null;
		}

		string accountIDString = jid.Username;

		EpicID accountID;
		try
		{
			accountID = EpicID.FromString(accountIDString);
		}
		catch
		{
			return null;
		}

       // Password in XMPP PLAIN is expected to be an OAuth access token.
		Session? session = await sessionService.GetSessionAsync(accessToken);
		if (session == null || session.AccountID != accountID)
		{
			return null;
		}

		return session.AccountID.ToString();
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
     HashSet<string> acceptedFriendIDs = await GetAcceptedFriendIDsAsync(presence.From.Username);

		lock (connections)
		{
			foreach (XmppConnection? connection in connections)
			{
				if (connection.ID.Equals(presence.From))
				{
					continue;
				}

              if (!acceptedFriendIDs.Contains(connection.ID.Username))
				{
					continue;
				}

				connection.QueueStanza(presence);
			}
		}

		return true;
	}

	public async Task SendPresenceOfFriendsAsync(XmppConnection connection)
	{
     HashSet<string> acceptedFriendIDs = await GetAcceptedFriendIDsAsync(connection.ID.Username);

		foreach (XmppConnection? c in connections)
		{
			if (ReferenceEquals(c, connection))
			{
				continue;
			}

            if (!acceptedFriendIDs.Contains(c.ID.Username))
			{
				continue;
			}

			var presenceProbe = new StanzaPresence()
			{
				//Type = StanzaPresence.TypeAttributeValues.Probe,
				From = c.ID,
				To = connection.ID,
             Show = c.Show,
				Status = c.Status,
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
     HashSet<string> acceptedFriendIDs = await GetAcceptedFriendIDsAsync(requestingJid.Username);

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

	private async Task<HashSet<string>> GetAcceptedFriendIDsAsync(string accountIDString)
	{
		if (friendService == null)
		{
			return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		}

		EpicID accountID;
		try
		{
			accountID = EpicID.FromString(accountIDString);
		}
		catch
		{
			return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		}

		List<FriendRequest> friends = await friendService.GetFriendsAsync(accountID);
		return friends
			.Where(friend => friend.Status == FriendStatus.Accepted)
			.Select(friend => (friend.Sender == accountID ? friend.Receiver : friend.Sender).ToString())
			.ToHashSet(StringComparer.OrdinalIgnoreCase);
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
