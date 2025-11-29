using System;
using System.Collections.Generic;
using System.Text;

namespace Fjeller.SimpleMapper.Extensions;

internal static class EnumerableExtensions
{
	extension<T>( IEnumerable<T?> enumerable )
	{
		internal IEnumerable<T> WhereNotNull()
		{
			foreach ( T? item in enumerable )
			{
				if ( item is null )
				{
					continue;
				}

				yield return item;
			}
		}
	}
}
