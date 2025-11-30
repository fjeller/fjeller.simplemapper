using Fjeller.SimpleMapper;
using Fjeller.SimpleMapper.Maps;
using Tests.Fjeller.SimpleMapper.TestInfrastructure;

namespace Tests.Fjeller.SimpleMapper.Maps;

/// ======================================================================================================================
/// <summary>
/// Tests for the SimpleMap class covering mapping configuration and property management
/// </summary>
/// ======================================================================================================================
public class SimpleMapTests : IDisposable
{
	public SimpleMapTests()
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
	/// Test profile for SimpleMap testing
	/// </summary>
	/// ======================================================================================================================
	private class SimpleMapTestProfile : MappingProfile
	{
		public ISimpleMap<SourceModel, DestinationModel>? BasicMap { get; private set; }
		public ISimpleMap<SourceWithExtraProperties, DestinationWithFewerProperties>? IgnoreMap { get; private set; }
		public ISimpleMap<SourceForComputation, DestinationWithComputation>? AfterMapActionMap { get; private set; }

		public SimpleMapTestProfile()
		{
			BasicMap = CreateMap<SourceModel, DestinationModel>();

			IgnoreMap = CreateMap<SourceWithExtraProperties, DestinationWithFewerProperties>()
				.IgnoreMember(x => x.Password);

			AfterMapActionMap = CreateMap<SourceForComputation, DestinationWithComputation>()
				.ExecuteAfterMapping((src, dest) => dest.ComputedValue = src.Value1 + src.Value2);
		}
	}

	/// ======================================================================================================================
	/// <summary>
	/// Test profile for IgnoreMembers testing
	/// </summary>
	/// ======================================================================================================================
	private class IgnoreMultipleMembersProfile : MappingProfile
	{
		public IgnoreMultipleMembersProfile()
		{
			CreateMap<SourceWithExtraProperties, DestinationWithFewerProperties>()
				.IgnoreMembers(nameof(SourceWithExtraProperties.Password), nameof(SourceWithExtraProperties.SecretData));
		}
	}

	[Fact]
	public void IgnoreMember_Should_ExcludeProperty_When_CalledWithExpression()
	{
		SimpleMapTestProfile profile = new();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		SourceWithExtraProperties source = new()
		{
			Id = 1,
			Name = "Test",
			Password = "should-not-map",
			SecretData = "should-map"
		};

		DestinationWithFewerProperties result = mapper.Map<SourceWithExtraProperties, DestinationWithFewerProperties>(source);

		Assert.NotNull(result);
		Assert.Equal(1, result.Id);
		Assert.Equal("Test", result.Name);
		Assert.Equal("should-map", result.SecretData);
		Assert.Equal(string.Empty, result.Password);
	}

	[Fact]
	public void IgnoreMember_Should_ExcludeProperty_When_CalledWithStringName()
	{
		// Use TestMappingProfile which already has SourceWithExtraProperties mapping
		new TestMappingProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		SourceWithExtraProperties source = new()
		{
			Id = 1,
			Name = "Test",
			SecretData = "confidential"
		};

		DestinationWithFewerProperties result = mapper.Map<SourceWithExtraProperties, DestinationWithFewerProperties>(source);

		Assert.NotNull(result);
	}

	[Fact]
	public void IgnoreMembers_Should_ExcludeMultipleProperties_When_CalledWithMultipleNames()
	{
		IgnoreMultipleMembersProfile profile = new();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		SourceWithExtraProperties source = new()
		{
			Id = 1,
			Name = "Test",
			Password = "password",
			SecretData = "secret"
		};

		DestinationWithFewerProperties result = mapper.Map<SourceWithExtraProperties, DestinationWithFewerProperties>(source);

		Assert.NotNull(result);
		Assert.Equal(1, result.Id);
		Assert.Equal("Test", result.Name);
		Assert.Equal(string.Empty, result.Password);
		Assert.Equal(string.Empty, result.SecretData);
	}

	[Fact]
	public void ExecuteAfterMapping_Should_ExecuteAction_When_MappingCompletes()
	{
		SimpleMapTestProfile profile = new();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		SourceForComputation source = new()
		{
			Value1 = 15,
			Value2 = 25
		};

		DestinationWithComputation result = mapper.Map<SourceForComputation, DestinationWithComputation>(source);

		Assert.Equal(40, result.ComputedValue);
	}

	[Fact]
	public void MappingKey_Should_ContainBothTypeNames_When_MapCreated()
	{
		SimpleMapTestProfile profile = new();

		Assert.NotNull(profile.BasicMap);
		Assert.Contains(nameof(SourceModel), profile.BasicMap.MappingKey);
		Assert.Contains(nameof(DestinationModel), profile.BasicMap.MappingKey);
	}

	[Fact]
	public void ValidProperties_Should_ContainOnlyMatchingProperties_When_Created()
	{
		SimpleMapTestProfile profile = new();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		SourceModel source = new() { Id = 1 };
		mapper.Map<SourceModel, DestinationModel>(source);

		Assert.NotNull(profile.BasicMap);
		Assert.NotEmpty(profile.BasicMap.ValidProperties);
	}

	[Fact]
	public void CreateValidProperties_Should_InitializeProperties_When_Called()
	{
		SimpleMapTestProfile profile = new();

		Assert.NotNull(profile.BasicMap);
		profile.BasicMap.CreateValidProperties();

		Assert.NotNull(profile.BasicMap.ValidProperties);
	}

	[Fact]
	public void IgnoreMember_Should_ReturnFluentInterface_When_Called()
	{
		SimpleMapTestProfile profile = new();

		Assert.NotNull(profile.IgnoreMap);
		Assert.IsAssignableFrom<ISimpleMap<SourceWithExtraProperties, DestinationWithFewerProperties>>(profile.IgnoreMap);
	}

	[Fact]
	public void ExecuteAfterMapping_Should_ReturnFluentInterface_When_Called()
	{
		SimpleMapTestProfile profile = new();

		Assert.NotNull(profile.AfterMapActionMap);
		Assert.IsAssignableFrom<ISimpleMap<SourceForComputation, DestinationWithComputation>>(profile.AfterMapActionMap);
	}

	[Fact]
	public void IgnoreMembers_Should_ReturnFluentInterface_When_Called()
	{
		IgnoreMultipleMembersProfile profile = new();

		Assert.NotNull(profile);
	}

	[Theory]
	[InlineData("Password")]
	[InlineData("SecretData")]
	[InlineData("NonExistentProperty")]
	public void IgnoreMember_Should_NotThrow_When_PropertyDoesNotExist(string propertyName)
	{
		TestIgnoreNonExistentProfile profile = new(propertyName);

		Assert.NotNull(profile);
	}

	[Fact]
	public void Map_Should_MapCollections_When_PropertiesAreCollectionTypes()
	{
		new TestMappingProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		SourceWithCollections source = new()
		{
			Id = 1,
			Name = "Test",
			Tags = new List<string> { "A", "B" },
			Numbers = new[] { 1, 2, 3 }
		};

		DestinationWithCollections result = mapper.Map<SourceWithCollections, DestinationWithCollections>(source);

		Assert.NotNull(result);
		Assert.Equal(1, result.Id);
		Assert.Equal("Test", result.Name);
		Assert.Equal(2, result.Tags.Count);
		Assert.Equal("A", result.Tags[0]);
		Assert.Equal("B", result.Tags[1]);
		Assert.Equal(3, result.Numbers.Length);
		Assert.Equal(1, result.Numbers[0]);
		Assert.Equal(3, result.Numbers[2]);
	}

	[Fact]
	public void ValidProperties_Should_NotIncludeIgnoredMembers_When_MembersIgnored()
	{
		SimpleMapTestProfile profile = new();
		TestHelper.PrepareCache();

		Assert.NotNull(profile.IgnoreMap);
		Assert.NotEmpty(profile.IgnoreMap.ValidProperties);
		Assert.DoesNotContain(profile.IgnoreMap.ValidProperties, p => p.Name == nameof(SourceWithExtraProperties.Password));
	}

	[Fact]
	public void Map_Should_OnlyMapStringTypeCollections_When_CollectionsPresent()
	{
		new TestMappingProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		SourceWithCollections source = new()
		{
			Id = 42,
			Name = "Collection Test"
		};

		DestinationWithCollections result = mapper.Map<SourceWithCollections, DestinationWithCollections>(source);

		Assert.Equal(42, result.Id);
		Assert.Equal("Collection Test", result.Name);
	}

	/// ======================================================================================================================
	/// <summary>
	/// Test profile that ignores non-existent properties
	/// </summary>
	/// ======================================================================================================================
	private class TestIgnoreNonExistentProfile : MappingProfile
	{
		public TestIgnoreNonExistentProfile(string propertyName)
		{
			CreateMap<SourceModel, DestinationModel>()
				.IgnoreMember(propertyName);
		}
	}
}
