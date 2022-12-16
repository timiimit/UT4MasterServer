using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UT4MasterServer
{
	public class Session
	{
		public int UserID { get; set; }
		public Token AccessToken { get; set; }
		public Token RefreshToken { get; set; }

		public Session(int userID, Token accessToken, Token refreshToken)
		{
			UserID = userID;
			AccessToken = accessToken;
			RefreshToken = refreshToken;
		}
	}
}
