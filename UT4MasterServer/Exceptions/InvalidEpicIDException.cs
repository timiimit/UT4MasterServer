using System.Runtime.Serialization;

namespace UT4MasterServer.Exceptions;

[Serializable]
public class InvalidEpicIDException : Exception
{
	private int _numericErrorCode { get; set; }
	public int NumericErrorCode { get { return _numericErrorCode; } }

	private string _errorCode { get; set; }
	public string ErrorCode { get { return _errorCode; } }

	public InvalidEpicIDException(string message, int numericErrorCode, string errorCode) : base(message)
	{
		_numericErrorCode = numericErrorCode;
		_errorCode = errorCode;
	}

	public InvalidEpicIDException(string message, int numericErrorCode, string errorCode, Exception innerException) : base(message, innerException)
	{
		_numericErrorCode = numericErrorCode;
		_errorCode = errorCode;
	}

	protected InvalidEpicIDException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
	{
		_numericErrorCode = serializationInfo.GetInt32(nameof(NumericErrorCode));
		_errorCode = serializationInfo.GetString(nameof(ErrorCode)) ?? string.Empty;
	}

	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException(nameof(info));
		}

		info.AddValue(nameof(NumericErrorCode), NumericErrorCode);
		info.AddValue(nameof(ErrorCode), ErrorCode);

		base.GetObjectData(info, context);
	}
}
