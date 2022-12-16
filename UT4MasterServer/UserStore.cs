using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace UT4MasterServer
{
	public class UserStore
	{
		private List<User> users;

		public UserStore()
		{
			users = new List<User>();

			// temporary users for testing
			users.Add(new User(1, "user1", GetPasswordHash("user1")));
			users.Add(new User(2, "user2", GetPasswordHash("user2")));
		}

		private static byte[] GetPasswordHash(string password)
		{
			var bytes = Encoding.UTF8.GetBytes(password);
			return SHA256.HashData(bytes);
		}

		public User? GetUserByUsername(string username)
		{
			for (int i = 0; i < users.Count; i++)
			{
				if (users[i].Username == username)
					return users[i];
			}
			return null;
		}

		public User? GetUserByUsernameAndPassword(string username, string password)
		{
			var user = GetUserByUsername(username);
			if (user?.Hash == GetPasswordHash(password))
			{
				return user;
			}
			return null;
		}
	}
}
