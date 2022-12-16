using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UT4MasterServer
{
	public class SessionStore
	{
		public List<Session> Sessions { get; private set; }
		public Dictionary<string, int> ActiveAuthCodes { get; private set; }

		public SessionStore()
		{
			Sessions = new List<Session>();
			ActiveAuthCodes = new Dictionary<string, int>();
		}

		/// <summary>
		/// Creates authorization code which can be used to create a session
		/// </summary>
		/// <param name="userID">user that wants to create a session</param>
		/// <returns></returns>
		public string CreateAuthCode(int userID)
		{
			string code = userID.ToString(); // TODO: hash together id + time
			ActiveAuthCodes.Add(code, userID);
			return code;
		}

		/// <summary>
		/// Creates session for user associated with authCode
		/// </summary>
		/// <param name="authCode"></param>
		/// <returns></returns>
		public bool CreateSession(string authCode)
		{
			if (!ActiveAuthCodes.ContainsKey(authCode))
				return false;

			var session = new Session(
				ActiveAuthCodes[authCode],
				Token.Generate(TimeSpan.FromDays(1)),
				Token.Generate(TimeSpan.FromDays(20))
			);

			ActiveAuthCodes.Remove(authCode);
			Sessions.Add(session);
			return true;
		}

		public bool RefreshSession(string refreshToken)
		{
			// TODO: refresh logic
			return true;
		}

		public int GetSessionUserID(string accessToken)
		{
			foreach (var session in Sessions)
			{
				if (session.AccessToken.Value == accessToken)
					return session.UserID;
			}
			return 0;
		}
	}
}
