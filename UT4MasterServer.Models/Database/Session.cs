using MongoDB.Bson.Serialization.Attributes;
using UT4MasterServer.Common;

namespace UT4MasterServer.Models.Database;

public enum SessionCreationMethod
{
	AuthorizationCode,
	ExchangeCode,
	ClientCredentials,
	Password
}

public class Session
{
	[BsonId]
	public EpicID ID { get; set; } = EpicID.Empty;

	[BsonElement("AccountID"), BsonIgnoreIfDefault] // default value is set in Program.cs
	public EpicID AccountID { get; set; } = EpicID.Empty;

	/// <summary>
	/// Client for which this session exists (eg. launcher or game)
	/// </summary>
	[BsonElement("ClientID")]
	public EpicID ClientID { get; set; } = EpicID.Empty;

	[BsonElement("AccessToken")]
	public Token AccessToken { get; set; }

	[BsonElement("RefreshToken"), BsonDefaultValue(null), BsonIgnoreIfDefault]
	public Token? RefreshToken { get; set; } = null;

	[BsonElement("CreationMethod")]
	public SessionCreationMethod CreationMethod { get; set; }

	public Session(EpicID session, EpicID account, EpicID clientID, SessionCreationMethod creationMethod)
	{
		ID = session;
		AccountID = account;
		ClientID = clientID;
		CreationMethod = creationMethod;

		AccessToken = null!; // NOTE: This is only for warning suppression. Refresh() will assign an actual value.
		Refresh();
	}

	public void Refresh()
	{
		// we extend duration that this session lasts. based on the creation method
		// we generate different durations for how long session lasts.
		switch (CreationMethod)
		{
			case SessionCreationMethod.Password: // unsure about duration of this
			case SessionCreationMethod.AuthorizationCode:
				AccessToken = Token.Generate(TimeSpan.FromDays(1)); // TODO: unsure if same as epic
				RefreshToken = Token.Generate(TimeSpan.FromDays(20)); // TODO: unsure if same as epic
				break;
			case SessionCreationMethod.ExchangeCode:
				AccessToken = Token.Generate(TimeSpan.FromHours(6)); // same as epic
				RefreshToken = Token.Generate(TimeSpan.FromHours(24)); // same as epic
				break;
			case SessionCreationMethod.ClientCredentials:

				// EPIC sets access token here to 4 hours and there is no refresh token.
				// game will create a new session when token is 2h from expiration.

#if DEBUG
				// debug with short sessions
				AccessToken = Token.Generate(TimeSpan.FromHours(2) + TimeSpan.FromMinutes(10));
#else
				AccessToken = Token.Generate(TimeSpan.FromHours(4));
#endif
				RefreshToken = null; // it's impossible to refresh, same as epic
				break;
			default:
				throw new ArgumentException("invalid sessionType");
		}
	}

	public bool HasExpired
	{
		get { return AccessToken.HasExpired; }
	}
}
