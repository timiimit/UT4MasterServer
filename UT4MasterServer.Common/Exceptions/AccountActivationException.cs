namespace UT4MasterServer.Common.Exceptions;

[Serializable]
public sealed class AccountActivationException : Exception
{
	public AccountActivationException(string message) : base(message)
	{
	}

	public AccountActivationException(string message, Exception innerException) : base(message, innerException)
	{
	}
}

