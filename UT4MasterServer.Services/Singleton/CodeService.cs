using Microsoft.Extensions.Options;
using UT4MasterServer.Models.Database;
using UT4MasterServer.Common;
using UT4MasterServer.Models.Settings;

namespace UT4MasterServer.Services.Singleton;

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
		codes = [];
	}

	public async Task<Code?> CreateCodeAsync(CodeKind kind, EpicID accountID, EpicID clientID)
	{
		return await Task.Run(() =>
		{
			lock (codes) // Make sure codes are thread-safe
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
			lock (codes) // Make sure codes are thread-safe
			{
				var i = codes.FindIndex(x => x.Token.Value == code && x.Kind == kind);
				if (i == -1)
				{
					return null;
				}

				Code? ret = codes[i];
				codes.RemoveAt(i);
				return ret;
			}
		});
	}

	public async Task<int> RemoveAllByAccountAsync(EpicID accountID)
	{
		return await Task.Run(() =>
		{
			lock (codes)
			{
				return codes.RemoveAll(x => x.AccountID == accountID);
			}
		});
	}

	public async Task<int> RemoveAllExpiredCodesAsync()
	{
		return await Task.Run(() =>
		{
			lock (codes) // Make sure codes are thread-safe
			{
				return codes.RemoveAll(x => x.Token.HasExpired);
			}
		});
	}
}
