﻿namespace UT4MasterServer.Common.Exceptions;

[Serializable]
public sealed class NotFoundException : Exception
{
	public NotFoundException(string message) : base(message)
	{
	}

	public NotFoundException(string message, Exception innerException) : base(message, innerException)
	{
	}
}
