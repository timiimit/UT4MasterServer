using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UT4MasterServer.Common.Enums;

[Flags]
public enum AccountFlags
{
	None = 0,

	Admin = 1,
	Moderator = 2,
	Developer = 4,
	ContentCreator = 8,
	HubOwner = 16,

	All = ~0
}
