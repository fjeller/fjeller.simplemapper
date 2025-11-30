using Fjeller.SimpleMapper;
using Tests.Fjeller.SimpleMapper.TestInfrastructure;

namespace Tests.Fjeller.SimpleMapper;

/// ======================================================================================================================
/// <summary>
/// Tests for collection mapping functionality including deep mapping of complex collection elements
/// </summary>
/// ======================================================================================================================
public class CollectionMappingTests : IDisposable
{
	public CollectionMappingTests()
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
	/// Source model with collections of primitive types
	/// </summary>
	/// ======================================================================================================================
	private class SourceWithPrimitiveCollections
	{
		public int Id { get; set; }
		public List<int> Numbers { get; set; } = new();
		public string[] Tags { get; set; } = Array.Empty<string>();
		public List<string> Names { get; set; } = new();
	}

	/// ======================================================================================================================
	/// <summary>
	/// Destination model with collections of primitive types
	/// </summary>
	/// ======================================================================================================================
	private class DestinationWithPrimitiveCollections
	{
		public int Id { get; set; }
		public List<int> Numbers { get; set; } = new();
		public string[] Tags { get; set; } = Array.Empty<string>();
		public List<string> Names { get; set; } = new();
	}

	/// ======================================================================================================================
	/// <summary>
	/// Complex type for nested collection testing
	/// </summary>
	/// ======================================================================================================================
	private class Address
	{
		public string Street { get; set; } = string.Empty;
		public string City { get; set; } = string.Empty;
		public int ZipCode { get; set; }
	}

	/// ======================================================================================================================
	/// <summary>
	/// Source model with collections of complex types
	/// </summary>
	/// ======================================================================================================================
	private class SourceWithComplexCollections
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public List<Address> Addresses { get; set; } = new();
	}

	/// ======================================================================================================================
	/// <summary>
	/// Destination model with collections of complex types
	/// </summary>
	/// ======================================================================================================================
	private class DestinationWithComplexCollections
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public List<Address> Addresses { get; set; } = new();
	}

	/// ======================================================================================================================
	/// <summary>
	/// Test profile for collection mapping
	/// </summary>
	/// ======================================================================================================================
	private class CollectionMappingProfile : MappingProfile
	{
		public CollectionMappingProfile()
		{
			CreateMap<SourceWithPrimitiveCollections, DestinationWithPrimitiveCollections>();
			CreateMap<SourceWithComplexCollections, DestinationWithComplexCollections>();
			CreateMap<Address, Address>();
		}
	}

	[Fact]
	public void Map_Should_MapListOfIntegers_When_CollectionIsPresent()
	{
		new CollectionMappingProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		SourceWithPrimitiveCollections source = new()
		{
			Id = 1,
			Numbers = new List<int> { 1, 2, 3, 4, 5 }
		};

		DestinationWithPrimitiveCollections result = mapper.Map<SourceWithPrimitiveCollections, DestinationWithPrimitiveCollections>(source);

		Assert.NotNull(result);
		Assert.Equal(1, result.Id);
		Assert.NotNull(result.Numbers);
		Assert.Equal(5, result.Numbers.Count);
		Assert.Equal(1, result.Numbers[0]);
		Assert.Equal(5, result.Numbers[4]);
	}

	[Fact]
	public void Map_Should_MapArrayOfStrings_When_CollectionIsPresent()
	{
		new CollectionMappingProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		SourceWithPrimitiveCollections source = new()
		{
			Id = 2,
			Tags = new[] { "Tag1", "Tag2", "Tag3" }
		};

		DestinationWithPrimitiveCollections result = mapper.Map<SourceWithPrimitiveCollections, DestinationWithPrimitiveCollections>(source);

		Assert.NotNull(result);
		Assert.Equal(2, result.Id);
		Assert.NotNull(result.Tags);
		Assert.Equal(3, result.Tags.Length);
		Assert.Equal("Tag1", result.Tags[0]);
		Assert.Equal("Tag3", result.Tags[2]);
	}

	[Fact]
	public void Map_Should_MapListOfStrings_When_CollectionIsPresent()
	{
		new CollectionMappingProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		SourceWithPrimitiveCollections source = new()
		{
			Id = 3,
			Names = new List<string> { "Alice", "Bob", "Charlie" }
		};

		DestinationWithPrimitiveCollections result = mapper.Map<SourceWithPrimitiveCollections, DestinationWithPrimitiveCollections>(source);

		Assert.NotNull(result);
		Assert.Equal(3, result.Id);
		Assert.NotNull(result.Names);
		Assert.Equal(3, result.Names.Count);
		Assert.Equal("Alice", result.Names[0]);
		Assert.Equal("Charlie", result.Names[2]);
	}

	[Fact]
	public void Map_Should_MapEmptyCollection_When_CollectionIsEmpty()
	{
		new CollectionMappingProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		SourceWithPrimitiveCollections source = new()
		{
			Id = 4,
			Numbers = new List<int>()
		};

		DestinationWithPrimitiveCollections result = mapper.Map<SourceWithPrimitiveCollections, DestinationWithPrimitiveCollections>(source);

		Assert.NotNull(result);
		Assert.Equal(4, result.Id);
		Assert.NotNull(result.Numbers);
		Assert.Empty(result.Numbers);
	}

	[Fact]
	public void Map_Should_MapComplexObjectCollection_When_DeepMappingRequired()
	{
		new CollectionMappingProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		SourceWithComplexCollections source = new()
		{
			Id = 5,
			Name = "Test User",
			Addresses = new List<Address>
			{
				new Address { Street = "123 Main St", City = "Springfield", ZipCode = 12345 },
				new Address { Street = "456 Oak Ave", City = "Shelbyville", ZipCode = 67890 }
			}
		};

		DestinationWithComplexCollections result = mapper.Map<SourceWithComplexCollections, DestinationWithComplexCollections>(source);

		Assert.NotNull(result);
		Assert.Equal(5, result.Id);
		Assert.Equal("Test User", result.Name);
		Assert.NotNull(result.Addresses);
		Assert.Equal(2, result.Addresses.Count);
		Assert.Equal("123 Main St", result.Addresses[0].Street);
		Assert.Equal("Springfield", result.Addresses[0].City);
		Assert.Equal(12345, result.Addresses[0].ZipCode);
		Assert.Equal("456 Oak Ave", result.Addresses[1].Street);
		Assert.Equal("Shelbyville", result.Addresses[1].City);
		Assert.Equal(67890, result.Addresses[1].ZipCode);
	}

	[Fact]
	public void Map_Should_HandleNullCollection_When_CollectionIsNull()
	{
		new CollectionMappingProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		SourceWithPrimitiveCollections source = new()
		{
			Id = 6,
			Numbers = null!
		};

		DestinationWithPrimitiveCollections result = mapper.Map<SourceWithPrimitiveCollections, DestinationWithPrimitiveCollections>(source);

		Assert.NotNull(result);
		Assert.Equal(6, result.Id);
	}

	[Fact]
	public void Map_Should_MapMultipleCollections_When_MultipleCollectionsPresent()
	{
		new CollectionMappingProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		SourceWithPrimitiveCollections source = new()
		{
			Id = 7,
			Numbers = new List<int> { 10, 20, 30 },
			Tags = new[] { "A", "B" },
			Names = new List<string> { "John", "Jane" }
		};

		DestinationWithPrimitiveCollections result = mapper.Map<SourceWithPrimitiveCollections, DestinationWithPrimitiveCollections>(source);

		Assert.NotNull(result);
		Assert.Equal(7, result.Id);
		Assert.Equal(3, result.Numbers.Count);
		Assert.Equal(2, result.Tags.Length);
		Assert.Equal(2, result.Names.Count);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(1)]
	[InlineData(5)]
	[InlineData(10)]
	public void Map_Should_HandleVariousCollectionSizes_When_DifferentSizesProvided(int count)
	{
		new CollectionMappingProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		SourceWithPrimitiveCollections source = new()
		{
			Id = 8,
			Numbers = Enumerable.Range(1, count).ToList()
		};

		DestinationWithPrimitiveCollections result = mapper.Map<SourceWithPrimitiveCollections, DestinationWithPrimitiveCollections>(source);

		Assert.NotNull(result);
		Assert.Equal(count, result.Numbers.Count);
	}

	[Fact]
	public void Map_Should_CreateNewCollectionInstances_When_Mapping()
	{
		new CollectionMappingProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		List<int> sourceNumbers = new() { 1, 2, 3 };
		SourceWithPrimitiveCollections source = new()
		{
			Id = 9,
			Numbers = sourceNumbers
		};

		DestinationWithPrimitiveCollections result = mapper.Map<SourceWithPrimitiveCollections, DestinationWithPrimitiveCollections>(source);

		Assert.NotNull(result);
		Assert.NotSame(sourceNumbers, result.Numbers);
		sourceNumbers.Add(4);
		Assert.Equal(3, result.Numbers.Count);
	}

	[Fact]
	public void Map_Should_IgnoreNullItemsInCollection_When_CollectionContainsNulls()
	{
		new CollectionMappingProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		SourceWithComplexCollections source = new()
		{
			Id = 10,
			Name = "Test",
			Addresses = new List<Address>
			{
				new Address { Street = "123 Main St", City = "Test City", ZipCode = 12345 },
				null!,
				new Address { Street = "456 Oak Ave", City = "Other City", ZipCode = 67890 }
			}
		};

		DestinationWithComplexCollections result = mapper.Map<SourceWithComplexCollections, DestinationWithComplexCollections>(source);

		Assert.NotNull(result);
		Assert.Equal(2, result.Addresses.Count);
	}
}
