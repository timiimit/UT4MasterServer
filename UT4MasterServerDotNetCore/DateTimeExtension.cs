namespace UT4MasterServer;

public static class DateTimeExtension
{
	public static string ToStringISO(this DateTime dt)
	{
		return dt.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK");
	}
}
