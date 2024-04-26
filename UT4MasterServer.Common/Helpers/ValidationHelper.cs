using System.Text.RegularExpressions;

namespace UT4MasterServer.Common.Helpers;

public static class ValidationHelper
{
	private static readonly Regex regexEmail;
	private static readonly List<string> disallowedUsernameWords;

	static ValidationHelper()
	{
		// regex from: https://www.emailregex.com/
		regexEmail = new Regex(@"^(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])");
		disallowedUsernameWords = new List<string>
		{
			"cock", "dick", "penis", "vagina", "tits", "pussy", "boner",
			"shit", "fuck", "bitch", "slut", "sex", "cum",
			"nigger", "hitler", "nazi"
		};
	}

	public static bool ValidateEmail(string email)
	{
		if (email.Length < 6 || email.Length > 64)
		{
			return false;
		}

		return regexEmail.IsMatch(email);
	}

	public static bool ValidateUsername(string username)
	{
		if (username.Length < 3 || username.Length > 32)
		{
			return false;
		}

		username = username.ToLower();

		// try to prevent impersonation of authority
		if (username == "admin" || username == "administrator" || username == "system")
		{
			return false;
		}

		// there's no way to prevent people from getting highly creative.
		// we just try some minimal filtering for now...
		foreach (var word in disallowedUsernameWords)
		{
			if (username.Contains(word))
			{
				return false;
			}
		}
		return true;
	}

	public static bool ValidatePassword(string password)
	{
		// we are expecting password to be SHA512 hash (64 bytes) in hex string form (128 chars)
		if (password.Length != 128)
		{
			return false;
		}

		if (!password.IsHexString())
		{
			return false;
		}

		return true;
	}
}
