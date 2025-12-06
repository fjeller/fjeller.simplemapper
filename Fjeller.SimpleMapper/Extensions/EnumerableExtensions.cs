using System;
using System.Collections.Generic;
using System.Text;

namespace Fjeller.SimpleMapper.Extensions;

internal static class EnumerableExtensions
{
	internal static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable)
	{
		foreach (T? item in enumerable)
		{
			if (item is null)
			{
				continue;
			}

			yield return item;
		}
	}
}
