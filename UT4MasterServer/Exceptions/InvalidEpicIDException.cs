using System.Runtime.Serialization;

namespace UT4MasterServer.Exceptions;

[Serializable]
public class InvalidEpicIDException : Exception
{
	public InvalidEpicIDException(string message) : base(message) { }

	protected InvalidEpicIDException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext) { }
}
