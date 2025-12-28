namespace UT4MasterServer.Common.Exceptions;

[Serializable]
public sealed class RateLimitExceededException : Exception
{
	public RateLimitExceededException(string message) : base(message)
	{
	}

	public RateLimitExceededException(string message, Exception innerException) : base(message, innerException)
	{
	}
}
