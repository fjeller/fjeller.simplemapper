using Fjeller.SimpleMapper.Storage;
using System.Reflection;

namespace Tests.Fjeller.SimpleMapper.TestInfrastructure;

/// ======================================================================================================================
/// <summary>
/// Test collection to ensure tests run in isolation
/// </summary>
/// ======================================================================================================================
[CollectionDefinition("Mapper Tests", DisableParallelization = true)]
public class MapperTestCollection
{
}

/// ======================================================================================================================
/// <summary>
/// Fixture to clear the cache before each test collection
/// </summary>
/// ======================================================================================================================
public class SimpleMapperTestFixture : IDisposable
{
	public SimpleMapperTestFixture()
	{
		ClearCache();
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
	}

	private static void ClearCache()
	{
		Type cacheType = typeof(global::Fjeller.SimpleMapper.SimpleMapper).Assembly.GetType("Fjeller.SimpleMapper.Storage.SimpleMapCache");
		if (cacheType is null)
		{
			return;
		}

		FieldInfo? mapsField = cacheType.GetField("_maps", BindingFlags.NonPublic | BindingFlags.Static);
		FieldInfo? sourceLookupField = cacheType.GetField("_sourceLookup", BindingFlags.NonPublic | BindingFlags.Static);
		FieldInfo? isPreparedField = cacheType.GetField("_isPrepared", BindingFlags.NonPublic | BindingFlags.Static);

		object? mapsList = mapsField?.GetValue(null);
		if (mapsList is System.Collections.IList list)
		{
			list.Clear();
		}

		object? sourceLookup = sourceLookupField?.GetValue(null);
		if (sourceLookup is System.Collections.IDictionary dict)
		{
			dict.Clear();
		}

		isPreparedField?.SetValue(null, false);
	}
}
