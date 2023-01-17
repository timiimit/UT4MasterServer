using System.Runtime.Serialization;

namespace UT4MasterServer.Exceptions;

[Serializable]
public class InvalidEpicIDException : Exception
{
	public string ID { get; private set; }
	public int NumericErrorCode { get; private set; }
	public string ErrorCode { get; private set; }

	public InvalidEpicIDException(string message, string id, int numericErrorCode, string errorCode) : base(message)
	{
		ID = id;
		NumericErrorCode = numericErrorCode;
		ErrorCode = errorCode;
	}

	public InvalidEpicIDException(string message, string id, int numericErrorCode, string errorCode, Exception innerException) : base(message, innerException)
	{
		ID = id;
		NumericErrorCode = numericErrorCode;
		ErrorCode = errorCode;
	}

	protected InvalidEpicIDException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
	{
		ID = serializationInfo.GetString(nameof(ID)) ?? string.Empty;
		NumericErrorCode = serializationInfo.GetInt32(nameof(NumericErrorCode));
		ErrorCode = serializationInfo.GetString(nameof(ErrorCode)) ?? string.Empty;
	}

	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException(nameof(info));
		}

		info.AddValue(nameof(ID), ID);
		info.AddValue(nameof(NumericErrorCode), NumericErrorCode);
		info.AddValue(nameof(ErrorCode), ErrorCode);

		base.GetObjectData(info, context);
	}
}
