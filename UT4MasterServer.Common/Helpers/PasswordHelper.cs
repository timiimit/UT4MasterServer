using System.Security.Cryptography;
using System.Text;
using UT4MasterServer.Common;

namespace UT4MasterServer.Common.Helpers;

public static class PasswordHelper
{
	public static string GetPasswordHash(EpicID accountID, string password)
	{
		// we combine both accountID and password to create a hash.
		// this way NO ONE can tell which users have the same password.
		string combined = accountID + password;

		return GetPasswordHash(combined);
	}

	public static string GetPasswordHash(string password)
	{
		var bytes = Encoding.UTF8.GetBytes(password);
		var hashedBytes = SHA512.HashData(bytes);
		var passwordHash = Convert.ToHexString(hashedBytes).ToLower();
		return passwordHash;
	}
}
