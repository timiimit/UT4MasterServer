using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace UT4MasterServer
{
	public class SessionStore
	{

		public List<Session> Sessions { get; private set; }
		public Dictionary<string, User> ActiveAuthCodes { get; private set; }

		private List<ExchangeCode> exchangeCodes { get; }


		public SessionStore()
		{
			Sessions = new List<Session>();
			ActiveAuthCodes = new Dictionary<string, User>();

			exchangeCodes = new List<ExchangeCode>();
		}

		/// <summary>
		/// Creates authorization code which can be used to create a session
		/// </summary>
		/// <param name="userID">user that wants to create a session</param>
		/// <returns></returns>
		public string CreateAuthCode(User user)
		{
			string code = user.ID.ToString(); // TODO: hash together id + time
#if DEBUG
			Console.WriteLine($"Authorization code for '{user}' = {code}");
#endif
			ActiveAuthCodes.Add(code, user);
			return code;
		}

		/// <summary>
		/// Creates session for user associated with authCode
		/// </summary>
		/// <param name="authCode"></param>
		/// <returns></returns>
		public Session? CreateSessionWithAuthCode(string authCode, string clientID)
		{
			if (!ActiveAuthCodes.ContainsKey(authCode))
				return null;

			var session = new Session(ActiveAuthCodes[authCode], clientID, SessionType.AuthorizationCode);

#if DEBUG
			Console.WriteLine($"Created session with authorization code for '{session.User}'");
#endif

			ActiveAuthCodes.Remove(authCode);
			Sessions.Add(session);
			return session;
		}

		public Session? CreateSessionWithExchangeCode(string exchangeCode, string clientID)
		{
			var code = TakeExchangeCode(exchangeCode);
			if (code == null)
				return null;

			var session = new Session(code.AssociatedSession.User, clientID, SessionType.ExchangeCode);

#if DEBUG
			Console.WriteLine($"Created session with exchange code '{exchangeCode}' for '{session.User}'");
#endif

			Sessions.Add(session);
			return session;
		}

		public Session CreateSessionWithPublicAccess(string clientID)
		{
			var session = new Session(UserStore.SystemUser, clientID, SessionType.ClientCredentials);
#if DEBUG
			Console.WriteLine($"Created session with public access");
#endif

			Sessions.Add(session);
			return session;
		}

		public ExchangeCode? CreateExchangeCode(string accessToken)
		{
			var session = GetSession(accessToken);
			if (session == null)
				return null;

			ExchangeCode code = new ExchangeCode(session, Token.Generate(TimeSpan.FromSeconds(300)));

#if DEBUG
			Console.WriteLine($"Created exchange code {code} for {session.User}");
#endif

			exchangeCodes.Add(code);
			return code;
		}

		public Session? RefreshSession(string refreshToken)
		{
			foreach (var session in Sessions)
			{
				if (session.RefreshToken.Value != refreshToken)
					continue;

				if (!session.RefreshToken.IsExpired)
				{
					session.Refresh();
					return session;
				}

				if (session.AccessToken.IsExpired)
					KillSession(session.AccessToken.Value);

				return null;
			}
			return null;
		}

		public Session? GetSession(string accessToken)
		{
			foreach (var session in Sessions)
			{
				if (session.AccessToken.Value == accessToken)
				{
					if (session.AccessToken.IsExpired)
					{
						// don't kill session here
						// refresh token might still be valid
						return null;
					}
					return session;
				}
			}
			return null;
		}

		public ExchangeCode? TakeExchangeCode(string exchangeCode)
		{
			for (int i = 0; i < exchangeCodes.Count; i++)
			{
				if (exchangeCodes[i].Token.Value == exchangeCode)
				{
					var exchange = exchangeCodes[i];
					exchangeCodes.RemoveAt(i);
					return exchange;
				}
			}
			return null;
		}

		public bool KillSession(string accessToken)
		{
			for (int i = 0; i < Sessions.Count; i++)
			{
				if (Sessions[i].AccessToken.Value == accessToken)
				{
					Sessions.RemoveAt(i);
					return true;
				}
			}
			return false;
		}

		public int KillOtherSessions(string accessToken)
		{
			var session = GetSession(accessToken);
			if (session == null)
				return -1;

			int killCount = 0;
			for (int i = 0; i < Sessions.Count; i++)
			{
				if (Sessions[i].ClientID == session.ClientID && Sessions[i] != session)
				{
					Sessions.RemoveAt(i);
					i--;
					killCount++;
				}
			}
			return killCount;
		}
	}
}
