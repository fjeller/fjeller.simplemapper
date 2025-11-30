using Fjeller.SimpleMapper.Exceptions;
using Fjeller.SimpleMapper.Extensions;
using Fjeller.SimpleMapper.Maps;
using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Text;

namespace Fjeller.SimpleMapper.Storage;

internal static class SimpleMapCache
{
	private static readonly List<ISimpleMap> _maps = [];
	private static readonly Dictionary<Type, Type> _sourceLookup = new();

	private static bool _isPrepared = false;

	private static bool HasMapWithSameCombination(ISimpleMap map)
	{
		bool result = _maps.Any( m => m.MappingKey.Equals( map.MappingKey ) );

		return result;
	}

	private static bool HasKeyForCombination( Type sourceType, Type destinationType )
	{
		string mapKey = $"{sourceType.FullName}_{destinationType.FullName}";
		return _maps.Any( m => m.MappingKey == mapKey );
	}

	private static Type? GetSourceFromTypeLookup( Type destinationType )
	{
		bool hasInterfaceType = _sourceLookup.TryGetValue( destinationType, out Type? result );

		return hasInterfaceType ? result : null;
	}

	internal static void ResetCache()
	{
		// Just for Testing so we can reset the cache. Do not use otherwise.
		_maps.Clear();
		_sourceLookup.Clear();
	}

	internal static Type? GetMatchingSourceType( Type destinationType, object source )
	{
		// get all interfaces for source type
		Type sourceType = source.GetCorrectSourceType();
		if ( HasKeyForCombination( sourceType, destinationType ) )
		{
			return sourceType;
		}

		Type? result = GetSourceFromTypeLookup( destinationType );
		if ( result != null )
		{
			return result;
		}

		List<Type> allTypes = sourceType.GetInterfaces().ToList();
		allTypes.Insert( 0, sourceType );

		foreach ( Type testingSourceType in allTypes )
		{
			if ( !HasKeyForCombination( testingSourceType, destinationType ) )
			{
				continue;
			}

			_sourceLookup.Add( destinationType, testingSourceType );
			return testingSourceType;
		}

		return null;
	}

	internal static void Prepare()
	{
		if ( _isPrepared )
		{
			return;
		}

		foreach ( ISimpleMap map in _maps )
		{
			map.CreateValidProperties();
		}

		_isPrepared = true;
	}

	internal static void AddMap(ISimpleMap map )
	{
		if ( HasMapWithSameCombination( map ) )
		{
			throw new MappingKeyException( $"A mapping with the same key already exists: {map.MappingKey}" );
		}
		_maps.Add( map );
		_isPrepared = false;
	}

	internal static ISimpleMap? GetMap(Type sourceType, Type destinationType )
	{
		string mapKey = ( sourceType, destinationType ).CreateMapKey();
		ISimpleMap? result = _maps.FirstOrDefault( m => m.MappingKey.Equals( mapKey ) );

		return result;
	}
}
