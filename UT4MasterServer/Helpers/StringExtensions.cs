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

	public static bool IsBase64(this string input, out byte[] parsedBytes)
	{
		parsedBytes = null;
		if (string.IsNullOrWhiteSpace(input))
		{
			return false;
		}

		var buffer = new Span<byte>(new byte[input.Length]);
		var isBase64 = Convert.TryFromBase64String(input, buffer, out var bytesParsed);

		if (isBase64)
		{
			parsedBytes = buffer[..bytesParsed].ToArray();
		}

		return isBase64;
	}
}
