using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using UT4MasterServer.Common;
using UT4MasterServer.Models.Database;
using UT4MasterServer.Models.Settings;

namespace UT4MasterServer.Services.Scoped;

/// <summary>
/// Issue + consume single-use password reset tokens.
/// Storage: mongo collection "password_reset_tokens" with TTL index on ExpiresAt.
/// </summary>
public sealed class PasswordResetService
{
	private static readonly TimeSpan TokenValidity = TimeSpan.FromHours(1);

	private readonly IMongoCollection<PasswordResetToken> collection;
	private readonly AccountService accountService;
	private readonly EmailService emailService;
	private readonly ApplicationSettings appSettings;
	private readonly ILogger<PasswordResetService> logger;

	public PasswordResetService(
		DatabaseContext db,
		AccountService accountService,
		EmailService emailService,
		IOptions<ApplicationSettings> appSettings,
		ILogger<PasswordResetService> logger)
	{
		this.collection = db.Database.GetCollection<PasswordResetToken>("password_reset_tokens");
		this.accountService = accountService;
		this.emailService = emailService;
		this.appSettings = appSettings.Value;
		this.logger = logger;
	}

	/// <summary>
	/// Ensures the mongo TTL index is present. Called once at app startup
	/// from ApplicationStartupService alongside the other CreateIndexes calls.
	/// </summary>
	public async Task CreateIndexesAsync()
	{
		var indexes = new[]
		{
			new CreateIndexModel<PasswordResetToken>(
				Builders<PasswordResetToken>.IndexKeys.Ascending(x => x.ExpiresAt),
				new CreateIndexOptions { ExpireAfter = TimeSpan.Zero }),
			new CreateIndexModel<PasswordResetToken>(
				Builders<PasswordResetToken>.IndexKeys.Ascending(x => x.AccountID)),
		};
		await collection.Indexes.CreateManyAsync(indexes);
	}

	/// <summary>
	/// Request a reset for the given email. Returns the issued token
	/// AND the account, or null if no account matches the email.
	/// CALLERS MUST NOT vary their response based on this returning null —
	/// the anti-enumeration property of the forgot-password endpoint depends
	/// on the response being identical either way.
	/// </summary>
	public async Task<(PasswordResetToken token, Account account)?> IssueAsync(string email)
	{
		var account = await accountService.GetAccountByEmailAsync(email);
		if (account is null) return null;

		var token = GenerateTokenString();
		var doc = new PasswordResetToken(token, account.ID, TokenValidity);
		await collection.InsertOneAsync(doc);
		logger.LogInformation("PasswordResetService: issued token for {AccountId}", account.ID);
		return (doc, account);
	}

	/// <summary>
	/// Send the reset email. Same anti-enumeration caveat as IssueAsync.
	/// Builds the URL from <see cref="ApplicationSettings.WebsiteDomain"/>.
	/// </summary>
	public async Task SendResetEmailAsync(Account account, string token, CancellationToken ct = default)
	{
		var resetUrl = $"{appSettings.WebsiteDomain.TrimEnd('/')}/reset-password?token={token}";
		var subject = "Reset your UT4 Master Server password";
		var plain =
			$"Hi {account.Username},\n\n" +
			$"A password reset was requested for your UT4 Master Server account.\n" +
			$"Open this link within 1 hour to choose a new password:\n\n  {resetUrl}\n\n" +
			$"If you didn't request a reset, ignore this email — your password remains unchanged.\n";
		var html =
			$"<p>Hi {System.Net.WebUtility.HtmlEncode(account.Username)},</p>" +
			$"<p>A password reset was requested for your UT4 Master Server account.</p>" +
			$"<p>Open this link within 1 hour to choose a new password:</p>" +
			$"<p><a href=\"{resetUrl}\">{resetUrl}</a></p>" +
			$"<p>If you didn't request a reset, ignore this email — your password remains unchanged.</p>";
		await emailService.SendAsync(account.Email, subject, plain, html, ct);
	}

	/// <summary>
	/// Look up + validate (not yet consumed, not expired) a reset token.
	/// Returns null if not found / not valid.
	/// </summary>
	public async Task<PasswordResetToken?> FindValidAsync(string token)
	{
		var doc = await collection.Find(x => x.Token == token).FirstOrDefaultAsync();
		if (doc is null || !doc.IsValid()) return null;
		return doc;
	}

	/// <summary>
	/// Atomically mark the token as consumed. Returns true if this call
	/// flipped Consumed from false→true (i.e. caller is the first to consume).
	/// </summary>
	public async Task<bool> ConsumeAsync(string token)
	{
		var update = Builders<PasswordResetToken>.Update.Set(x => x.Consumed, true);
		var filter = Builders<PasswordResetToken>.Filter.And(
			Builders<PasswordResetToken>.Filter.Eq(x => x.Token, token),
			Builders<PasswordResetToken>.Filter.Eq(x => x.Consumed, false),
			Builders<PasswordResetToken>.Filter.Gt(x => x.ExpiresAt, DateTime.UtcNow));
		var result = await collection.UpdateOneAsync(filter, update);
		return result.ModifiedCount == 1;
	}

	private static string GenerateTokenString()
	{
		Span<byte> buf = stackalloc byte[32];
		RandomNumberGenerator.Fill(buf);
		return Convert.ToHexString(buf).ToLowerInvariant();
	}
}
