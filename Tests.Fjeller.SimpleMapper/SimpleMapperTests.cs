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

	#region Tests for Map<TDestination>(IEnumerable<object?> source)

	[Fact]
	public void Map_Should_MapObjectCollection_When_RuntimeTypeDetectionNeeded()
	{
		IEnumerable<object?> sources = new object?[]
		{
			new SourceModel { Id = 1, Name = "First" },
			new SourceModel { Id = 2, Name = "Second" },
			new SourceModel { Id = 3, Name = "Third" }
		};

		IEnumerable<DestinationModel> results = _mapper.Map<DestinationModel>(sources);

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
	public void Map_Should_FilterNullElements_When_ObjectCollectionContainsNulls()
	{
		IEnumerable<object?> sources = new object?[]
		{
			new SourceModel { Id = 1, Name = "First" },
			null,
			new SourceModel { Id = 3, Name = "Third" },
			null
		};

		IEnumerable<DestinationModel> results = _mapper.Map<DestinationModel>(sources);

		Assert.NotNull(results);
		List<DestinationModel> resultList = results.ToList();
		Assert.Equal(2, resultList.Count);
		Assert.Equal(1, resultList[0].Id);
		Assert.Equal("First", resultList[0].Name);
		Assert.Equal(3, resultList[1].Id);
		Assert.Equal("Third", resultList[1].Name);
	}

	[Fact]
	public void Map_Should_ReturnEmptyEnumerable_When_ObjectCollectionIsEmpty()
	{
		IEnumerable<object?> sources = Array.Empty<object?>();

		IEnumerable<DestinationModel> results = _mapper.Map<DestinationModel>(sources);

		Assert.NotNull(results);
		Assert.Empty(results);
	}

	[Fact]
	public void Map_Should_ReturnEmptyEnumerable_When_ObjectCollectionContainsOnlyNulls()
	{
		IEnumerable<object?> sources = new object?[] { null, null, null };

		IEnumerable<DestinationModel> results = _mapper.Map<DestinationModel>(sources);

		Assert.NotNull(results);
		Assert.Empty(results);
	}

	[Fact]
	public void Map_Should_MapLargeObjectCollection_When_ManyItemsProvided()
	{
		List<object?> sources = new();
		for (int i = 1; i <= 100; i++)
		{
			sources.Add(new SourceModel { Id = i, Name = $"Item {i}" });
		}

		IEnumerable<DestinationModel> results = _mapper.Map<DestinationModel>(sources);

		Assert.NotNull(results);
		List<DestinationModel> resultList = results.ToList();
		Assert.Equal(100, resultList.Count);
		Assert.Equal(1, resultList[0].Id);
		Assert.Equal("Item 1", resultList[0].Name);
		Assert.Equal(100, resultList[99].Id);
		Assert.Equal("Item 100", resultList[99].Name);
	}

	[Fact]
	public void Map_Should_UseDeferredExecution_When_MappingObjectCollection()
	{
		List<object?> sources = new()
		{
			new SourceModel { Id = 1, Name = "First" },
			new SourceModel { Id = 2, Name = "Second" }
		};

		IEnumerable<DestinationModel> results = _mapper.Map<DestinationModel>(sources);

		// Add item after creating the enumerable
		sources.Add(new SourceModel { Id = 3, Name = "Third" });

		// LINQ Select creates a snapshot of the collection when enumerated
		// Since we're enumerating the list itself (not the Select result yet),
		// the list modification will be included
		List<DestinationModel> resultList = results.ToList();
		Assert.Equal(3, resultList.Count); // Includes the newly added item
	}

	[Fact]
	public void Map_Should_HandleInterfaceImplementation_When_MappingObjectCollection()
	{
		IEnumerable<object?> sources = new object?[]
		{
			new SourceEntity { Id = 1, Name = "Entity 1", Description = "Test" },
			new SourceEntity { Id = 2, Name = "Entity 2", Description = "Test" }
		};

		IEnumerable<DestinationEntity> results = _mapper.Map<DestinationEntity>(sources);

		Assert.NotNull(results);
		List<DestinationEntity> resultList = results.ToList();
		Assert.Equal(2, resultList.Count);
		Assert.Equal(1, resultList[0].Id);
		Assert.Equal("Entity 1", resultList[0].Name);
		Assert.Equal(2, resultList[1].Id);
		Assert.Equal("Entity 2", resultList[1].Name);
	}

	[Fact]
	public void Map_Should_MapWithAfterMappingAction_When_MappingObjectCollectionWithComputation()
	{
		IEnumerable<object?> sources = new object?[]
		{
			new SourceForComputation { Value1 = 10, Value2 = 20 },
			new SourceForComputation { Value1 = 5, Value2 = 15 }
		};

		IEnumerable<DestinationWithComputation> results = _mapper.Map<DestinationWithComputation>(sources);

		Assert.NotNull(results);
		List<DestinationWithComputation> resultList = results.ToList();
		Assert.Equal(2, resultList.Count);
		Assert.Equal(30, resultList[0].ComputedValue);
		Assert.Equal(20, resultList[1].ComputedValue);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(1)]
	[InlineData(5)]
	[InlineData(10)]
	public void Map_Should_HandleVariousCollectionSizes_When_MappingObjectCollection(int count)
	{
		List<object?> sources = new();
		for (int i = 1; i <= count; i++)
		{
			sources.Add(new SourceModel { Id = i, Name = $"Item {i}" });
		}

		IEnumerable<DestinationModel> results = _mapper.Map<DestinationModel>(sources);

		Assert.NotNull(results);
		Assert.Equal(count, results.Count());
	}

	[Fact]
	public void Map_Should_ThrowException_When_NoMappingExistsForObjectCollectionElement()
	{
		IEnumerable<object?> sources = new object?[]
		{
			new IncompatibleDestination { Id = 1 }
		};

		Assert.Throws<SimpleMapperException>(() => _mapper.Map<DestinationModel>(sources).ToList());
	}

	[Fact]
	public void Map_Should_MapMixedNullsAndValidObjects_When_ObjectCollectionHasInterspersedNulls()
	{
		IEnumerable<object?> sources = new object?[]
		{
			new SourceModel { Id = 1, Name = "First" },
			null,
			null,
			new SourceModel { Id = 2, Name = "Second" },
			null,
			new SourceModel { Id = 3, Name = "Third" }
		};

		IEnumerable<DestinationModel> results = _mapper.Map<DestinationModel>(sources);

		Assert.NotNull(results);
		List<DestinationModel> resultList = results.ToList();
		Assert.Equal(3, resultList.Count);
		Assert.Equal(1, resultList[0].Id);
		Assert.Equal(2, resultList[1].Id);
		Assert.Equal(3, resultList[2].Id);
	}

	[Fact]
	public void Map_Should_PreserveOrder_When_MappingObjectCollection()
	{
		IEnumerable<object?> sources = new object?[]
		{
			new SourceModel { Id = 10, Name = "Tenth" },
			new SourceModel { Id = 5, Name = "Fifth" },
			new SourceModel { Id = 1, Name = "First" },
			new SourceModel { Id = 20, Name = "Twentieth" }
		};

		IEnumerable<DestinationModel> results = _mapper.Map<DestinationModel>(sources);

		List<DestinationModel> resultList = results.ToList();
		Assert.Equal(10, resultList[0].Id);
		Assert.Equal(5, resultList[1].Id);
		Assert.Equal(1, resultList[2].Id);
		Assert.Equal(20, resultList[3].Id);
	}

	[Fact]
	public void Map_Should_WorkWithLinqOperations_When_MappingObjectCollection()
	{
		IEnumerable<object?> sources = new object?[]
		{
			new SourceModel { Id = 1, Name = "Alice" },
			new SourceModel { Id = 2, Name = "Bob" },
			new SourceModel { Id = 3, Name = "Charlie" },
			new SourceModel { Id = 4, Name = "David" }
		};

		IEnumerable<DestinationModel> results = _mapper.Map<DestinationModel>(sources);
		List<DestinationModel> filtered = results.Where(d => d.Id > 2).ToList();

		Assert.Equal(2, filtered.Count);
		Assert.Equal(3, filtered[0].Id);
		Assert.Equal("Charlie", filtered[0].Name);
		Assert.Equal(4, filtered[1].Id);
		Assert.Equal("David", filtered[1].Name);
	}

	#endregion
}
