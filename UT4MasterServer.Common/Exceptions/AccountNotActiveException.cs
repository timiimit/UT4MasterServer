namespace UT4MasterServer.Common.Exceptions;

[Serializable]
public sealed class AccountNotActiveException : Exception
{
	public AccountNotActiveException(string message) : base(message)
	{
	}

	public AccountNotActiveException(string message, Exception innerException) : base(message, innerException)
	{
	}
}
