using System;
using System.Collections.Generic;
using System.Text;

namespace Fjeller.SimpleMapper.Extensions;

internal static class ListExtensions
{
	extension<T>( List<T> list )
	{
		internal void AddIfNotNull( T? item )
		{
			if ( item is not null )
			{
				list.Add( item );
			}
		}

		internal void AddIfNotContains(T? item )
		{
			if (item is null )
			{
				return;
			}

			if ( list.Contains( item ) )
			{
				return;
			}

			list.Add( item );
		}
	}
}
