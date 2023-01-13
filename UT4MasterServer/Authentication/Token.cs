using MongoDB.Bson.Serialization.Attributes;
using System.Security.Cryptography;

namespace UT4MasterServer.Authentication;

public class Token
{
	private static readonly RandomNumberGenerator r = RandomNumberGenerator.Create();


	[BsonElement("Value")]
	public string Value { get; set; }

	[BsonElement("Expiration")]
	public DateTime ExpirationTime { get; set; }

	[BsonIgnore]
	public TimeSpan ExpirationDuration { get => ExpirationTime - DateTime.UtcNow; }

	[BsonIgnore]
	public int ExpirationDurationInSeconds { get => (int)ExpirationDuration.TotalSeconds; }

	[BsonIgnore]
	public bool HasExpired { get => DateTime.UtcNow > ExpirationTime; }

	public Token(string value, DateTime expiration)
	{
		Value = value;
		ExpirationTime = expiration;
	}

	public override string ToString()
	{
		return Value;
	}



	public static Token Generate(TimeSpan expirationDuration)
	{
		return Generate(DateTime.UtcNow + expirationDuration);
	}

	public static Token Generate(DateTime expirationTime)
	{
		// NOTE: epic uses sometimes uses JWT and sometimes just random 16-byte hexstring
		//       depending on the token type

		// we could infer importance of length/security based on expiration

		byte[] bytes = new byte[32];
		r.GetBytes(bytes);

		var tokenString = Convert.ToHexString(bytes).ToLower();

		return new Token(tokenString, expirationTime);
	}
}
