namespace UT4MasterServer.Other;

public static class DateTimeExtension
{
	public static string ToStringISO(this DateTime dt)
	{
		return dt.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK");
	}

	public static int ToUnixTimestamp(this DateTime dt)
	{
		// after 2038 we want it to rollover (cast to int) because the game probably cant handle that.
		// and yes, i did actually consider this :)
		return (int)(dt - UnixTimestampStartOfTime).TotalSeconds;
	}

	public static readonly DateTime UnixTimestampStartOfTime = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
}
