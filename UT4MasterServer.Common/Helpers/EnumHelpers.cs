using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UT4MasterServer.Common.Enums;

namespace UT4MasterServer.Common.Helpers;

public static class EnumHelpers
{
	public static bool HasFlagAny<T>(this T @this, T other) where T : struct, Enum
	{
		return (Convert.ToUInt64(@this) & Convert.ToUInt64(other)) != 0;
	}

	public static string[] EnumToStrings<T>(T @enum) where T : struct, Enum
	{
		var result = new List<string>();
		var allNames = Enum.GetNames<T>();
		var allValues = Enum.GetValues<T>();

		for (int i = 0; i < allNames.Length; i++)
		{
			var val = allValues[i];

			if (@enum.HasFlag(val))
			{
				result.Add(allNames[i]);
			}
		}

		return result.ToArray();
	}

	public static T StringsToEnum<T>(params string[] strings) where T : struct, Enum
	{
		var allNames = Enum.GetNames<T>();
		var allValues = Enum.GetValues<T>();
		ulong ret = 0;

		for (int i = 0; i < allNames.Length; i++)
		{
			if (strings.Contains(allNames[i]))
			{
				var val = allValues[i];

				ret |= Convert.ToUInt64(val);
			}
		}

		return (T)Enum.ToObject(typeof(T), ret);
	}

	public static T[] StringsToEnumArray<T>(params string[] strings) where T : struct, Enum
	{
		List<T> values = new List<T>();
		var allNames = Enum.GetNames<T>();
		var allValues = Enum.GetValues<T>();

		for (var i = 0; i < allNames.Length; i++)
		{
			if (strings.Contains(allNames[i]))
			{
				values.Add(allValues[i]);
			}
		}

		return values.ToArray();
	}

	public static T[] EnumFlagsToEnumArray<T>(T @enum) where T : struct, Enum
	{
		var result = new List<T>();
		var allValues = Enum.GetValues<T>();

		for (int i = 0; i < allValues.Length; i++)
		{
			var val = allValues[i];

			if (@enum.HasFlag(val))
			{
				result.Add(allValues[i]);
			}
		}

		return result.ToArray();
	}

	public static T EnumArrayToEnumFlags<T>(IEnumerable<T> enumArray) where T : struct, Enum
	{
		ulong ret = 0;
		foreach (var enumValue in enumArray)
		{
			ret |= Convert.ToUInt64(enumValue);
		}
		return (T)Enum.ToObject(typeof(T), ret);
	}
}
