using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UT4MasterServer
{
	public class User
	{
		public int ID { get; private set; }
		public string Username { get; private set; }
		public byte[] Hash { get; private set; }
		public int XP { get; private set; }
		public UserStats Stats { get; private set; }

		public User(int id, string username, byte[] hash)
		{
			ID = id;
			Username = username;
			Hash = hash;
			XP = 0;
			Stats = new UserStats();
		}
	}
}
