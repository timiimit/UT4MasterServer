using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UT4MasterServer
{
	public static class EpicDateTime
	{
		public static string ToStringEpic(this DateTime dt)
		{
			return dt.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK");
		}
	}
}
