using MongoDB.Bson.Serialization.Attributes;
using UT4MasterServer.Authentication;

namespace UT4MasterServer.Models;

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

	[BsonElement("UserID")]
	public EpicID AccountID { get; set; } = EpicID.Empty;

	/// <summary>
	/// Client for which this session exists (eg. launcher or game)
	/// </summary>
	[BsonElement("ClientID")]
	public EpicID ClientID { get; set; } = EpicID.Empty;

	[BsonElement("AccessToken")]
	public Token AccessToken { get; set; }

	[BsonElement("RefreshToken")]
	public Token RefreshToken { get; set; }

	[BsonElement("CreationMethod")]
	public SessionCreationMethod CreationMethod { get; set; }


	public Session(EpicID session, EpicID account, EpicID clientID, SessionCreationMethod creationMethod)
	{
		ID = session;
		AccountID = account;
		ClientID = clientID;
		CreationMethod = creationMethod;

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
				AccessToken = Token.Generate(TimeSpan.FromHours(4)); // same as epic
				RefreshToken = Token.Generate(TimeSpan.FromHours(24)); // same as epic
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
