using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UT4MasterServer.Common.Enums;

namespace UT4MasterServer.Common.Helpers;

public class AccountFlagsHelper
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
			AccountFlags.ACL_Maintenance);
	}
}
