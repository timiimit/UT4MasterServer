using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UT4MasterServer.Common.Enums;

namespace UT4MasterServer.Common.Helpers;

public static class EnumExtensions
{
	public static bool HasFlagAny<T>(this T _this, T other) where T : Enum
	{
		return (Convert.ToUInt64(_this) & Convert.ToUInt64(other)) != 0;
	}
}
