using Fjeller.SimpleMapper;
using Fjeller.SimpleMapper.Exceptions;
using Fjeller.SimpleMapper.Maps;
using Fjeller.SimpleMapper.Storage;
using Tests.Fjeller.SimpleMapper.TestInfrastructure;

namespace Tests.Fjeller.SimpleMapper.Storage;

/// ======================================================================================================================
/// <summary>
/// Tests for the SimpleMapCache class covering caching and retrieval of mappings
/// </summary>
/// ======================================================================================================================
public class SimpleMapCacheTests : IDisposable
{
	public SimpleMapCacheTests()
	{
		TestHelper.ResetMapperCache();
	}

	public void Dispose()
	{
		TestHelper.ResetMapperCache();
		GC.SuppressFinalize(this);
	}

	/// ======================================================================================================================
	/// <summary>
	/// Test profile for interface mapping
	/// </summary>
	/// ======================================================================================================================
	private class InterfaceMapProfile : MappingProfile
	{
		public InterfaceMapProfile()
		{
			CreateMap<IEntity, DestinationEntity>();
		}
	}

	[Fact]
	public void AddMap_Should_RegisterMapping_When_NewMapProvided()
	{
		new TestMappingProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		SourceModel source = new() { Id = 1, Name = "Test" };

		DestinationModel result = mapper.Map<SourceModel, DestinationModel>(source);

		Assert.NotNull(result);
		Assert.Equal(1, result.Id);
	}

	[Fact]
	public void AddMap_Should_ThrowException_When_DuplicateKeyAdded()
	{
		new TestMappingProfile();

		Assert.Throws<MappingKeyException>(() => new TestMappingProfile());
	}

	[Fact]
	public void GetMap_Should_ReturnMapping_When_MappingExists()
	{
		new TestMappingProfile();

		ISimpleMap? map = SimpleMapCache.GetMap(typeof(SourceModel), typeof(DestinationModel));

		Assert.NotNull(map);
		Assert.Contains(nameof(SourceModel), map.MappingKey);
		Assert.Contains(nameof(DestinationModel), map.MappingKey);
	}

	[Fact]
	public void GetMap_Should_ReturnNull_When_MappingDoesNotExist()
	{
		ISimpleMap? map = SimpleMapCache.GetMap(typeof(SourceModel), typeof(IncompatibleDestination));

		Assert.Null(map);
	}

	[Fact]
	public void Prepare_Should_InitializeMappings_When_Called()
	{
		new TestMappingProfile();

		SimpleMapCache.Prepare();

		ISimpleMap? map = SimpleMapCache.GetMap(typeof(SourceModel), typeof(DestinationModel));

		Assert.NotNull(map);
		Assert.NotEmpty(map.ValidProperties);
	}

	[Fact]
	public void ResetCache_Should_ClearAllMappings_When_Called()
	{
		new TestMappingProfile();

		ISimpleMap? mapBefore = SimpleMapCache.GetMap(typeof(SourceModel), typeof(DestinationModel));
		Assert.NotNull(mapBefore);

		SimpleMapCache.ResetCache();

		ISimpleMap? mapAfter = SimpleMapCache.GetMap(typeof(SourceModel), typeof(DestinationModel));
		Assert.Null(mapAfter);
	}

	[Fact]
	public void GetMatchingSourceType_Should_ReturnType_When_DirectMatchExists()
	{
		new TestMappingProfile();

		SourceModel source = new() { Id = 1, Name = "Test" };
		Type? matchedType = SimpleMapCache.GetMatchingSourceType(typeof(DestinationModel), source);

		Assert.NotNull(matchedType);
		Assert.Equal(typeof(SourceModel), matchedType);
	}

	[Fact]
	public void GetMatchingSourceType_Should_ReturnInterface_When_InterfaceMapExists()
	{
		new InterfaceMapProfile();

		IEntity source = new SourceEntity { Id = 1, Name = "Interface Test" };
		Type? matchedType = SimpleMapCache.GetMatchingSourceType(typeof(DestinationEntity), source);

		Assert.NotNull(matchedType);
		Assert.Equal(typeof(IEntity), matchedType);
	}

	[Fact]
	public void GetMatchingSourceType_Should_ReturnNull_When_NoMatchExists()
	{
		SourceModel source = new() { Id = 1 };
		Type? matchedType = SimpleMapCache.GetMatchingSourceType(typeof(IncompatibleDestination), source);

		Assert.Null(matchedType);
	}

	[Fact]
	public void GetMatchingSourceType_Should_CacheResult_When_InterfaceLookupPerformed()
	{
		new InterfaceMapProfile();

		IEntity source1 = new SourceEntity { Id = 1, Name = "First" };
		IEntity source2 = new SourceEntity { Id = 2, Name = "Second" };

		Type? matchedType1 = SimpleMapCache.GetMatchingSourceType(typeof(DestinationEntity), source1);
		Type? matchedType2 = SimpleMapCache.GetMatchingSourceType(typeof(DestinationEntity), source2);

		Assert.NotNull(matchedType1);
		Assert.NotNull(matchedType2);
		Assert.Same(matchedType1, matchedType2);
	}

	[Fact]
	public void Cache_Should_HandleMultipleMappings_When_MultipleProfilesRegistered()
	{
		new TestMappingProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();

		SourceModel source1 = new() { Id = 1 };
		SourceForComputation source2 = new() { Value1 = 10, Value2 = 20 };

		DestinationModel result1 = mapper.Map<SourceModel, DestinationModel>(source1);
		DestinationWithComputation result2 = mapper.Map<SourceForComputation, DestinationWithComputation>(source2);

		Assert.NotNull(result1);
		Assert.NotNull(result2);
		Assert.Equal(30, result2.ComputedValue);
	}

	[Theory]
	[InlineData(1)]
	[InlineData(5)]
	[InlineData(10)]
	public void Cache_Should_RetrieveSameMapping_When_CalledMultipleTimes(int iterations)
	{
		new TestMappingProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();

		for (int i = 0; i < iterations; i++)
		{
			SourceModel source = new() { Id = i, Name = $"Test {i}" };
			DestinationModel result = mapper.Map<SourceModel, DestinationModel>(source);

			Assert.NotNull(result);
			Assert.Equal(i, result.Id);
			Assert.Equal($"Test {i}", result.Name);
		}
	}

	[Fact]
	public void Cache_Should_MaintainSeparateMappings_When_DifferentTypeCombinationsUsed()
	{
		new TestMappingProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();

		SourceModel source1 = new() { Id = 1, Name = "First" };
		SourceForComputation source2 = new() { Value1 = 5, Value2 = 10 };

		DestinationModel result1 = mapper.Map<SourceModel, DestinationModel>(source1);
		DestinationWithComputation result2 = mapper.Map<SourceForComputation, DestinationWithComputation>(source2);

		Assert.NotNull(result1);
		Assert.NotNull(result2);
		Assert.Equal("First", result1.Name);
		Assert.Equal(15, result2.ComputedValue);
	}

	[Fact]
	public void Prepare_Should_InitializeAllMaps_When_MultipleMapsExist()
	{
		new TestMappingProfile();

		SimpleMapCache.Prepare();

		ISimpleMap? map1 = SimpleMapCache.GetMap(typeof(SourceModel), typeof(DestinationModel));
		ISimpleMap? map2 = SimpleMapCache.GetMap(typeof(SourceForComputation), typeof(DestinationWithComputation));

		Assert.NotNull(map1);
		Assert.NotNull(map2);
		Assert.NotEmpty(map1.ValidProperties);
		Assert.NotEmpty(map2.ValidProperties);
	}

	[Fact]
	public void AddMap_Should_ResetPreparedFlag_When_MapAdded()
	{
		new TestMappingProfile();
		SimpleMapCache.Prepare();

		TestHelper.ResetMapperCache();

		MappingProfile newProfile = new TestMappingProfile();

		ISimpleMap? map = SimpleMapCache.GetMap(typeof(SourceModel), typeof(DestinationModel));
		Assert.NotNull(map);
	}

	[Fact]
	public void GetMatchingSourceType_Should_HandleConcreteClassFirst_When_InterfaceAlsoMapped()
	{
		new TestMappingProfile();

		SourceEntity source = new() { Id = 1, Name = "Test" };
		Type? matchedType = SimpleMapCache.GetMatchingSourceType(typeof(DestinationEntity), source);

		Assert.NotNull(matchedType);
		Assert.Equal(typeof(SourceEntity), matchedType);
	}
}
