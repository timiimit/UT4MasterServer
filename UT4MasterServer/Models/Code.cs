using MongoDB.Bson.Serialization.Attributes;
using UT4MasterServer.Authentication;
using UT4MasterServer.Other;

namespace UT4MasterServer.Models;

public enum CodeKind
{
	Authorization,
	Exchange
}

/// <summary>
/// class common for all types of codes. these basically exchange identity of user between applications.
/// they expire after certain amount of time.
/// </summary>
[BsonIgnoreExtraElements]
public class Code
{
	public EpicID AccountID { get; set; }
	public EpicID CreatingClientID { get; set; }
	public Token Token { get; set; }
	public CodeKind Kind { get; set; }

	public Code(EpicID accountID, EpicID creatingClientID, Token token, CodeKind kind)
	{
		AccountID = accountID;
		CreatingClientID = creatingClientID;
		Token = token;
		Kind = kind;
	}
}
