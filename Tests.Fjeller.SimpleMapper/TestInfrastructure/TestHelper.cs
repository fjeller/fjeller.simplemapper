using Fjeller.SimpleMapper.Compilation;
using Fjeller.SimpleMapper.Maps;
using Fjeller.SimpleMapper.Storage;

namespace Tests.Fjeller.SimpleMapper.TestInfrastructure;

/// ======================================================================================================================
/// <summary>
/// Test helper class providing utilities for test isolation and cache management
/// </summary>
/// ======================================================================================================================
public static class TestHelper
{
	/// ======================================================================================================================
	/// <summary>
	/// Resets the SimpleMapCache and CompiledMapCache to ensure test isolation. Should be called before each test or test class.
	/// </summary>
	/// ======================================================================================================================
	public static void ResetMapperCache()
	{
		SimpleMapCache.ResetCache();
		CompiledMapCache.ClearCache();
	}

	/// ======================================================================================================================
	/// <summary>
	/// Gets a map from the cache for testing purposes
	/// </summary>
	/// <param name="sourceType">The source type</param>
	/// <param name="destinationType">The destination type</param>
	/// <returns>The map if found, otherwise null</returns>
	/// ======================================================================================================================
	public static ISimpleMap? GetMap(Type sourceType, Type destinationType)
	{
		return SimpleMapCache.GetMap(sourceType, destinationType);
	}

	/// ======================================================================================================================
	/// <summary>
	/// Adds a map to the cache for testing purposes
	/// </summary>
	/// <param name="map">The map to add</param>
	/// ======================================================================================================================
	public static void AddMap(ISimpleMap map)
	{
		SimpleMapCache.AddMap(map);
	}

	/// ======================================================================================================================
	/// <summary>
	/// Prepares all maps in the cache for testing purposes
	/// </summary>
	/// ======================================================================================================================
	public static void PrepareCache()
	{
		SimpleMapCache.Prepare();
	}

	/// ======================================================================================================================
	/// <summary>
	/// Gets the matching source type for testing interface-based mappings
	/// </summary>
	/// <param name="destinationType">The destination type</param>
	/// <param name="source">The source object</param>
	/// <returns>The matching source type if found, otherwise null</returns>
	/// ======================================================================================================================
	public static Type? GetMatchingSourceType(Type destinationType, object source)
	{
		return SimpleMapCache.GetMatchingSourceType(destinationType, source);
	}
}
