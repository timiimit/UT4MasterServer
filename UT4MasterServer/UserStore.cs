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
		// we create a dummy user which acts as system user and has access
		// to anything that can be accessed publicly without an account (such as cloudstorage)
		// basically for this: https://github.com/MixV2/EpicResearch/blob/master/docs/auth/grant_types/client_credentials.md
		public static readonly User SystemUser = new User(UserID.GetSystem(), string.Empty, new byte[0]);

		private List<User> users;

		public UserStore()
		{
			users = new List<User>();
			users.Add(SystemUser);
		}

		public User CreateUser(string username, string password)
		{
			// randomly generate id
			Random r = new Random();
			byte[] bytes = new byte[16];
			r.NextBytes(bytes);
			string id = BitConverter.ToString(bytes);

			// TODO: make sure generated id is unique
			var user = new User(GenerateUniqueUserID(), username, GetPasswordHash(password));
			users.Add(user);
			return user;
		}

		public User? GetUserByID(UserID id)
		{
			for (int i = 0; i < users.Count; i++)
			{
				if (users[i].ID == id)
					return users[i];
			}
			return null;
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





		private UserID GenerateUniqueUserID()
		{
			// TODO: missing implementation
			return UserID.GenerateNew();
		}

		private static byte[] GetPasswordHash(string password)
		{
			var bytes = Encoding.UTF8.GetBytes(password);
			return SHA256.HashData(bytes);
		}
	}
}
