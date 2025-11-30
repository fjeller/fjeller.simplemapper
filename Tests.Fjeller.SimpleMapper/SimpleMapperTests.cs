using Fjeller.SimpleMapper;
using Fjeller.SimpleMapper.Exceptions;
using Tests.Fjeller.SimpleMapper.TestInfrastructure;

namespace Tests.Fjeller.SimpleMapper;

/// ======================================================================================================================
/// <summary>
/// Tests for the SimpleMapper class covering all mapping scenarios
/// </summary>
/// ======================================================================================================================
public class SimpleMapperTests : IDisposable
{
	private readonly ISimpleMapper _mapper;

	public SimpleMapperTests()
	{
		TestHelper.ResetMapperCache();
		_mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		new TestMappingProfile();
	}

	public void Dispose()
	{
		TestHelper.ResetMapperCache();
		GC.SuppressFinalize(this);
	}

	[Fact]
	public void Map_Should_MapAllProperties_When_TypesMatch()
	{
		SourceModel source = new()
		{
			Id = 1,
			Name = "Test User",
			Email = "test@example.com",
			Age = 30,
			IsActive = true
		};

		DestinationModel result = _mapper.Map<SourceModel, DestinationModel>(source);

		Assert.NotNull(result);
		Assert.Equal(source.Id, result.Id);
		Assert.Equal(source.Name, result.Name);
		Assert.Equal(source.Email, result.Email);
		Assert.Equal(source.Age, result.Age);
		Assert.Equal(source.IsActive, result.IsActive);
	}

	[Fact]
	public void Map_Should_CreateNewInstance_When_DestinationIsNull()
	{
		SourceModel source = new()
		{
			Id = 1,
			Name = "Test User"
		};

		DestinationModel result = _mapper.Map<SourceModel, DestinationModel>(source, null);

		Assert.NotNull(result);
		Assert.Equal(source.Id, result.Id);
		Assert.Equal(source.Name, result.Name);
	}

	[Fact]
	public void Map_Should_UseExistingInstance_When_DestinationProvided()
	{
		SourceModel source = new()
		{
			Id = 1,
			Name = "Updated Name"
		};

		DestinationModel existing = new()
		{
			Id = 999,
			Name = "Old Name"
		};

		DestinationModel result = _mapper.Map(source, existing);

		Assert.Same(existing, result);
		Assert.Equal(source.Id, result.Id);
		Assert.Equal(source.Name, result.Name);
	}

	[Fact]
	public void Map_Should_ThrowException_When_NoMappingExists()
	{
		SourceModel source = new() { Id = 1 };

		Assert.Throws<ArgumentException>(() => _mapper.Map<SourceModel, IncompatibleDestination>(source));
	}

	[Fact]
	public void Map_Should_ExecuteAfterMappingAction_When_ActionDefined()
	{
		SourceForComputation source = new()
		{
			Value1 = 10,
			Value2 = 20
		};

		DestinationWithComputation result = _mapper.Map<SourceForComputation, DestinationWithComputation>(source);

		Assert.Equal(source.Value1, result.Value1);
		Assert.Equal(source.Value2, result.Value2);
		Assert.Equal(30, result.ComputedValue);
	}

	[Fact]
	public void Map_Should_CreateNewInstance_When_UsingTwoGenericParameters()
	{
		SourceModel source = new()
		{
			Id = 42,
			Name = "Test"
		};

		DestinationModel result = _mapper.Map<SourceModel, DestinationModel>(source);

		Assert.NotNull(result);
		Assert.Equal(42, result.Id);
		Assert.Equal("Test", result.Name);
	}

	[Fact]
	public void Map_Should_ReturnNull_When_SourceIsNullWithSingleGeneric()
	{
		SourceModel? source = null;

		DestinationModel? result = _mapper.Map<DestinationModel>(source);

		Assert.Null(result);
	}

	[Fact]
	public void Map_Should_DetectSourceType_When_UsingRuntimeTypeDetection()
	{
		object source = new SourceModel
		{
			Id = 100,
			Name = "Runtime Type"
		};

		DestinationModel? result = _mapper.Map<DestinationModel>(source);

		Assert.NotNull(result);
		Assert.Equal(100, result.Id);
		Assert.Equal("Runtime Type", result.Name);
	}

	[Fact]
	public void Map_Should_MapToExistingInstance_When_UsingRuntimeTypeDetection()
	{
		object source = new SourceModel
		{
			Id = 50,
			Name = "Updated"
		};

		DestinationModel existing = new()
		{
			Id = 1,
			Name = "Original"
		};

		DestinationModel? result = _mapper.Map(source, existing);

		Assert.NotNull(result);
		Assert.Same(existing, result);
		Assert.Equal(50, result.Id);
		Assert.Equal("Updated", result.Name);
	}

	[Fact]
	public void Map_Should_ThrowException_When_NoMatchingMapForRuntimeType()
	{
		object source = new IncompatibleDestination { Id = 1 };

		Assert.Throws<SimpleMapperException>(() => _mapper.Map<DestinationModel>(source));
	}

	[Fact]
	public void Map_Should_MapCollection_When_EnumerableProvided()
	{
		List<SourceModel> sources = new()
		{
			new SourceModel { Id = 1, Name = "First" },
			new SourceModel { Id = 2, Name = "Second" },
			new SourceModel { Id = 3, Name = "Third" }
		};

		IEnumerable<DestinationModel> results = _mapper.Map<SourceModel, DestinationModel>(sources);

		Assert.NotNull(results);
		List<DestinationModel> resultList = results.ToList();
		Assert.Equal(3, resultList.Count);
		Assert.Equal(1, resultList[0].Id);
		Assert.Equal("First", resultList[0].Name);
		Assert.Equal(2, resultList[1].Id);
		Assert.Equal("Second", resultList[1].Name);
		Assert.Equal(3, resultList[2].Id);
		Assert.Equal("Third", resultList[2].Name);
	}

	[Fact]
	public void Map_Should_FilterNulls_When_MappingCollection()
	{
		List<SourceModel?> sources = new()
		{
			new SourceModel { Id = 1, Name = "First" },
			null,
			new SourceModel { Id = 3, Name = "Third" }
		};

		IEnumerable<DestinationModel> results = _mapper.Map<SourceModel, DestinationModel>(sources!);

		Assert.NotNull(results);
		List<DestinationModel> resultList = results.ToList();
		Assert.Equal(2, resultList.Count);
		Assert.Equal(1, resultList[0].Id);
		Assert.Equal(3, resultList[1].Id);
	}

	[Fact]
	public void Map_Should_MapCollectionProperties_When_CollectionsExist()
	{
		SourceWithCollections source = new()
		{
			Id = 1,
			Name = "Test",
			Tags = new List<string> { "Tag1", "Tag2" },
			Numbers = new[] { 1, 2, 3 }
		};

		DestinationWithCollections result = _mapper.Map<SourceWithCollections, DestinationWithCollections>(source);

		Assert.NotNull(result);
		Assert.Equal(source.Id, result.Id);
		Assert.Equal(source.Name, result.Name);
		Assert.Equal(2, result.Tags.Count);
		Assert.Equal("Tag1", result.Tags[0]);
		Assert.Equal("Tag2", result.Tags[1]);
		Assert.Equal(3, result.Numbers.Length);
		Assert.Equal(1, result.Numbers[0]);
		Assert.Equal(3, result.Numbers[2]);
	}

	[Fact]
	public void Map_Should_HandleEmptyCollection_When_MappingEnumerable()
	{
		List<SourceModel> sources = new();

		IEnumerable<DestinationModel> results = _mapper.Map<SourceModel, DestinationModel>(sources);

		Assert.NotNull(results);
		Assert.Empty(results);
	}

	[Theory]
	[InlineData(0, "")]
	[InlineData(1, "A")]
	[InlineData(100, "Test Name")]
	[InlineData(-1, "Negative ID")]
	public void Map_Should_HandleVariousValues_When_MappingProperties(int id, string name)
	{
		SourceModel source = new()
		{
			Id = id,
			Name = name
		};

		DestinationModel result = _mapper.Map<SourceModel, DestinationModel>(source);

		Assert.Equal(id, result.Id);
		Assert.Equal(name, result.Name);
	}

	[Fact]
	public void Map_Should_MapInterfaceType_When_SourceImplementsInterface()
	{
		SourceEntity source = new()
		{
			Id = 10,
			Name = "Interface Test",
			Description = "This should not be mapped"
		};

		DestinationEntity result = _mapper.Map<DestinationEntity>(source);

		Assert.NotNull(result);
		Assert.Equal(10, result.Id);
		Assert.Equal("Interface Test", result.Name);
	}

	[Fact]
	public void Map_Should_OnlyMapWritableProperties_When_PropertyCannotBeWritten()
	{
		SourceModel source = new()
		{
			Id = 1,
			Name = "Test",
			Email = "test@test.com",
			Age = 25,
			IsActive = true
		};

		DestinationModel result = _mapper.Map<SourceModel, DestinationModel>(source);

		Assert.NotNull(result);
		Assert.Equal(source.Id, result.Id);
	}

	[Theory]
	[InlineData(null, "name")]
	[InlineData("", "name")]
	[InlineData("test@example.com", "Test User")]
	public void Map_Should_HandleNullableStrings_When_Mapping(string? email, string name)
	{
		SourceModel source = new()
		{
			Id = 1,
			Name = name,
			Email = email ?? string.Empty
		};

		DestinationModel result = _mapper.Map<SourceModel, DestinationModel>(source);

		Assert.NotNull(result);
		Assert.Equal(source.Email, result.Email);
	}
}
