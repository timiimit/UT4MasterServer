using MongoDB.Bson.Serialization.Attributes;
using UT4MasterServer.Common;

namespace UT4MasterServer.Models.Database;

/// <summary>
/// Single-use password reset token. Generated on forgot-password request,
/// emailed to the account's address, consumed by reset-password.
///
/// Stored in collection "password_reset_tokens" with a TTL index on
/// <see cref="ExpiresAt"/> so mongo auto-expires stale rows.
/// </summary>
[BsonIgnoreExtraElements]
public sealed class PasswordResetToken
{
	/// <summary>Hex-encoded 32-byte random token. Stored hashed in a future hardening pass.</summary>
	[BsonId]
	public string Token { get; set; } = string.Empty;

	[BsonElement("AccountID")]
	public EpicID AccountID { get; set; } = EpicID.Empty;

	[BsonElement("CreatedAt")]
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	/// <summary>
	/// Token expiry. Mongo TTL index will auto-delete past this time, but the
	/// service also checks explicitly on each use as a defense-in-depth.
	/// </summary>
	[BsonElement("ExpiresAt")]
	public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(1);

	/// <summary>True after the token has been consumed by a reset.</summary>
	[BsonElement("Consumed")]
	public bool Consumed { get; set; } = false;

	public PasswordResetToken() { }

	public PasswordResetToken(string token, EpicID accountID, TimeSpan validity)
	{
		Token = token;
		AccountID = accountID;
		CreatedAt = DateTime.UtcNow;
		ExpiresAt = DateTime.UtcNow.Add(validity);
		Consumed = false;
	}

	public bool IsValid()
		=> !Consumed && DateTime.UtcNow < ExpiresAt;
}
