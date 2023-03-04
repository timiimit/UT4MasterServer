using UT4MasterServer.Common.Enums;

namespace UT4MasterServer.Common.Helpers;

public static class AccountFlagsHelper
{
	public static bool IsACLFlag(AccountFlags flag)
	{
		return flag.HasFlagAny(
			AccountFlags.Admin |
			AccountFlags.ACL_Clients |
			AccountFlags.ACL_AccountsLow |
			AccountFlags.ACL_AccountsHigh |
			AccountFlags.ACL_TrustedServers |
			AccountFlags.ACL_Stats |
			AccountFlags.ACL_CloudStorageAnnouncements |
			AccountFlags.ACL_CloudStorageRulesets |
			AccountFlags.ACL_CloudStorageChallenges |
			AccountFlags.ACL_Maintenance |
			AccountFlags.EmailVerified);
	}
}
