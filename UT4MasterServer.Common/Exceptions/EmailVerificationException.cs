namespace UT4MasterServer.Common.Exceptions;

[Serializable]
public sealed class EmailVerificationException : Exception
{
	public EmailVerificationException(string message) : base(message)
	{
	}

	public EmailVerificationException(string message, Exception innerException) : base(message, innerException)
	{
	}
}

