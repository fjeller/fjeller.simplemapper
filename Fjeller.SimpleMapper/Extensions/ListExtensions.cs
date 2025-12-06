using System;
using System.Collections.Generic;
using System.Text;

namespace Fjeller.SimpleMapper.Extensions;

internal static class ListExtensions
{
	internal static void AddIfNotNull<T>(this List<T> list, T? item)
	{
		if (item is not null)
		{
			list.Add(item);
		}
	}

	internal static void AddIfNotContains<T>(this List<T> list, T? item)
	{
		if (item is null)
		{
			return;
		}

		if (list.Contains(item))
		{
			return;
		}

		list.Add(item);
	}
}
