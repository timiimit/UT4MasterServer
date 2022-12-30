namespace UT4MasterServer.Helpers;

public static class StringExtensions
{
	public static bool IsHexString(this string input)
	{
		if (string.IsNullOrWhiteSpace(input))
		{
			return false;
		}

		foreach (var c in input)
		{
			if (!((c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F') || (c >= '0' && c <= '9')))
			{
				return false;
			}
		}

		return true;
	}
}
