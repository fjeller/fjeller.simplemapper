using Fjeller.SimpleMapper;
using Fjeller.SimpleMapper.Exceptions;
using Fjeller.SimpleMapper.Maps;
using Tests.Fjeller.SimpleMapper.TestInfrastructure;

namespace Tests.Fjeller.SimpleMapper;

/// ======================================================================================================================
/// <summary>
/// Tests for the MappingProfile class covering profile creation and map registration
/// </summary>
/// ======================================================================================================================
public class MappingProfileTests : IDisposable
{
	public MappingProfileTests()
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
	/// Test profile for creating maps
	/// </summary>
	/// ======================================================================================================================
	private class TestProfile : MappingProfile
	{
		public bool MapCreated { get; private set; }

		public void CreateTestMap()
		{
			CreateMap<SourceModel, DestinationModel>();
			MapCreated = true;
		}
	}

	/// ======================================================================================================================
	/// <summary>
	/// Test profile that creates duplicate maps
	/// </summary>
	/// ======================================================================================================================
	private class DuplicateMapProfile : MappingProfile
	{
		public DuplicateMapProfile()
		{
			CreateMap<SourceModel, DestinationModel>();
		}
	}

	/// ======================================================================================================================
	/// <summary>
	/// Test profile with fluent configuration
	/// </summary>
	/// ======================================================================================================================
	private class FluentConfigurationProfile : MappingProfile
	{
		public FluentConfigurationProfile()
		{
			CreateMap<SourceWithExtraProperties, DestinationWithFewerProperties>()
				.IgnoreMember(x => x.Password)
				.IgnoreMember(nameof(SourceWithExtraProperties.SecretData));
		}
	}

	/// ======================================================================================================================
	/// <summary>
	/// Test profile with multiple maps
	/// </summary>
	/// ======================================================================================================================
	private class MultipleMapProfile : MappingProfile
	{
		public int MapCount { get; private set; }

		public MultipleMapProfile()
		{
			CreateMap<SourceModel, DestinationModel>();
			MapCount++;

			CreateMap<SourceForComputation, DestinationWithComputation>();
			MapCount++;

			CreateMap<SourceEntity, DestinationEntity>();
			MapCount++;
		}
	}

	[Fact]
	public void CreateMap_Should_RegisterMapping_When_Called()
	{
		TestProfile profile = new();
		profile.CreateTestMap();

		Assert.True(profile.MapCreated);

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		SourceModel source = new() { Id = 1, Name = "Test" };

		DestinationModel result = mapper.Map<SourceModel, DestinationModel>(source);

		Assert.NotNull(result);
		Assert.Equal(1, result.Id);
	}

	[Fact]
	public void CreateMap_Should_ThrowException_When_DuplicateMapRegistered()
	{
		new TestMappingProfile();

		Assert.Throws<MappingKeyException>(() => new DuplicateMapProfile());
	}

	[Fact]
	public void CreateMap_Should_ReturnFluentInterface_When_Called()
	{
		TestProfile profile = new();

		profile.CreateTestMap();

		Assert.True(profile.MapCreated);
	}

	[Fact]
	public void CreateMap_Should_AllowFluentConfiguration_When_ChainedCalls()
	{
		FluentConfigurationProfile profile = new();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		SourceWithExtraProperties source = new()
		{
			Id = 1,
			Name = "Test",
			Password = "secret",
			SecretData = "confidential"
		};

		DestinationWithFewerProperties result = mapper.Map<SourceWithExtraProperties, DestinationWithFewerProperties>(source);

		Assert.NotNull(result);
		Assert.Equal(1, result.Id);
		Assert.Equal("Test", result.Name);
	}

	[Fact]
	public void MappingProfile_Should_InheritFromBaseClass_When_Created()
	{
		TestProfile profile = new();

		Assert.IsAssignableFrom<MappingProfile>(profile);
	}

	[Fact]
	public void CreateMap_Should_RegisterMultipleMaps_When_CalledMultipleTimes()
	{
		MultipleMapProfile profile = new();

		Assert.Equal(3, profile.MapCount);

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		SourceModel source = new() { Id = 1 };

		DestinationModel result = mapper.Map<SourceModel, DestinationModel>(source);

		Assert.NotNull(result);
	}

	[Fact]
	public void CreateMap_Should_StoreMapInCache_When_ProfileCreated()
	{
		TestProfile profile = new();
		profile.CreateTestMap();

		ISimpleMap? map = TestHelper.GetMap(typeof(SourceModel), typeof(DestinationModel));

		Assert.NotNull(map);
	}

	[Fact]
	public void CreateMap_Should_AllowIgnoreMembers_When_ConfiguringMap()
	{
		FluentConfigurationProfile profile = new();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		SourceWithExtraProperties source = new()
		{
			Id = 1,
			Name = "Test",
			Password = "password123",
			SecretData = "secret123"
		};

		DestinationWithFewerProperties result = mapper.Map<SourceWithExtraProperties, DestinationWithFewerProperties>(source);

		Assert.NotNull(result);
		Assert.Equal(1, result.Id);
		Assert.Equal("Test", result.Name);
		Assert.Equal(string.Empty, result.Password);
		Assert.Equal(string.Empty, result.SecretData);
	}
}
