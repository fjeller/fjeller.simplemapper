using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Fjeller.SimpleMapper.Extensions;

internal static class PropertyInfoExtensions
{
	private static readonly BindingFlags _defaultFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	public static IEnumerable<PropertyInfo> GetPropertyInfos<T>( this T source )
	{
		return source?.GetType().GetProperties() ?? [];
	}

	public static IEnumerable<PropertyInfo> GetPropertyInfos( this Type type )
	{
		PropertyInfo[] result = type.GetProperties( _defaultFlags );
		return result;
	}

	public static PropertyInfo? GetPropertyInfo( this Type type, string propertyName )
	{
		PropertyInfo? result = type.GetProperty( propertyName, _defaultFlags );
		return result;
	}

	public static IEnumerable<PropertyInfo> GetPropertyInfos( this Type type, IEnumerable<string> propertyNames )
	{
		List<PropertyInfo> result = [];
		foreach ( string propertyName in propertyNames )
		{
			PropertyInfo? currentProperty = type.GetPropertyInfo( propertyName );
			result.AddIfNotNull( currentProperty );
		}
		return result;
	}
}
