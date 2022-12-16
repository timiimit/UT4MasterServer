using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UT4MasterServer
{
	public class User
	{
		public UserID ID { get; private set; }
		public string Username { get; private set; }
		public byte[] Hash { get; private set; }
		public int XP { get; private set; }
		public UserStats Stats { get; private set; }
		public List<string> Friends { get; private set; }
		public List<string> BlockedUsers { get; private set; }

		public User(UserID id, string username, byte[] hash)
		{
			ID = id;
			Username = username;
			Hash = hash;
			XP = 0;
			Stats = new UserStats();
			Friends = new List<string>();
			BlockedUsers = new List<string>();
		}

		public override string ToString()
		{
			return $"[{ID}] {Username}";
		}
	}
}
