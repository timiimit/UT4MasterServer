using Microsoft.Extensions.Options;
using UT4MasterServer.Authentication;
using UT4MasterServer.Models;
using UT4MasterServer.Other;

namespace UT4MasterServer.Services;

/// <summary>
/// Service capable of creating and retrieving codes used for authentication.
/// </summary>
public sealed class CodeService
{
	/// <summary>
	/// if master dies, all codes disappear. They are meant for temporary use anyway.
	/// Client can create a new one in case that happens.
	/// </summary>
	private readonly List<Code> codes;

	public CodeService(IOptions<ApplicationSettings> settings)
	{
		codes = new List<Code>();
	}

	public async Task<Code?> CreateCodeAsync(CodeKind kind, EpicID accountID, EpicID clientID)
	{
		return await Task.Run(() =>
		{
			lock (codes) // make sure codes are thread-safe
			{
				// Each user can only have a single code of some kind.
				// Remove it if one such code already exists.
				codes.RemoveAll(x => x.AccountID == accountID && x.CreatingClientID == clientID && x.Kind == kind);

				// Create new code
				var ret = new Code(accountID, clientID, Token.Generate(TimeSpan.FromMinutes(5)), kind);
				codes.Add(ret);
				return ret;
			}
		});
	}

	public async Task<Code?> TakeCodeAsync(CodeKind kind, string code)
	{
		return await Task.Run(() =>
		{
			lock (codes) // make sure codes are thread-safe
			{
				int i = codes.FindIndex(x => x.Token.Value == code && x.Kind == kind);
				if (i == -1)
					return null;

				var ret = codes[i];
				codes.RemoveAt(i);
				return ret;
			}
		});
	}
}
