namespace UT4MasterServer.Common.Exceptions;

[Serializable]
public sealed class AwsSesClientException : Exception
{
	public AwsSesClientException(string message) : base(message)
	{
	}

	public AwsSesClientException(string message, Exception innerException) : base(message, innerException)
	{
	}
}
