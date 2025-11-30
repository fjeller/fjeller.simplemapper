using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Benchmarks.Fjeller.SimpleMapper.Models;
using Fjeller.SimpleMapper;

namespace Benchmarks.Fjeller.SimpleMapper;

[MemoryDiagnoser]
[ThreadingDiagnoser]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
public class SimpleMapperBenchmarks
{
	private ISimpleMapper _mapper = null!;
	private SimpleModel _simpleSource = null!;
	private ComplexModel _complexSource = null!;
	private PersonWithCollections _personWithCollections = null!;
	private List<SimpleModel> _simpleList = null!;
	private List<ComplexModel> _complexList = null!;
	private List<PersonWithCollections> _personsWithCollections = null!;

	[GlobalSetup]
	public void Setup()
	{
		_mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		new BenchmarkMappingProfile();

		_simpleSource = new SimpleModel
		{
			Id = 1,
			Name = "John Doe",
			Email = "john.doe@example.com",
			Age = 30,
			IsActive = true
		};

		_complexSource = new ComplexModel
		{
			Id = 1,
			Name = "John Doe",
			Email = "john.doe@example.com",
			Phone = "555-1234",
			Address = "123 Main St",
			City = "Springfield",
			State = "IL",
			ZipCode = "62701",
			Country = "USA",
			CreatedDate = DateTime.Now,
			ModifiedDate = DateTime.Now,
			IsActive = true,
			IsDeleted = false,
			CreatedBy = 1,
			ModifiedBy = 1,
			Description = "Test description with some content",
			Notes = "Some notes about this item",
			Price = 99.99m,
			Quantity = 10,
			Category = "Electronics",
			Tags = "tag1,tag2,tag3"
		};

		_personWithCollections = new PersonWithCollections
		{
			Id = 1,
			Name = "Jane Smith",
			PhoneNumbers = new List<string> { "555-1111", "555-2222", "555-3333" },
			Addresses = new List<Address>
			{
				new() { Street = "123 Main St", City = "Springfield", State = "IL", ZipCode = 62701 },
				new() { Street = "456 Oak Ave", City = "Shelbyville", State = "IL", ZipCode = 62702 },
				new() { Street = "789 Elm Rd", City = "Capital City", State = "IL", ZipCode = 62703 }
			},
			FavoriteNumbers = new[] { 7, 13, 21, 42, 99 }
		};

		_simpleList = Enumerable.Range(1, 100)
			.Select(i => new SimpleModel
			{
				Id = i,
				Name = $"Person {i}",
				Email = $"person{i}@example.com",
				Age = 20 + (i % 50),
				IsActive = i % 2 == 0
			})
			.ToList();

		_complexList = Enumerable.Range(1, 100)
			.Select(i => new ComplexModel
			{
				Id = i,
				Name = $"Item {i}",
				Email = $"item{i}@example.com",
				Phone = $"555-{i:D4}",
				Address = $"{i} Main Street",
				City = "Springfield",
				State = "IL",
				ZipCode = $"6270{i % 10}",
				Country = "USA",
				CreatedDate = DateTime.Now.AddDays(-i),
				ModifiedDate = DateTime.Now,
				IsActive = i % 2 == 0,
				IsDeleted = false,
				CreatedBy = 1,
				ModifiedBy = 1,
				Description = $"Description for item {i}",
				Notes = $"Notes for item {i}",
				Price = 10.00m * i,
				Quantity = i,
				Category = $"Category {i % 5}",
				Tags = $"tag{i},tag{i + 1},tag{i + 2}"
			})
			.ToList();

		_personsWithCollections = Enumerable.Range(1, 50)
			.Select(i => new PersonWithCollections
			{
				Id = i,
				Name = $"Person {i}",
				PhoneNumbers = new List<string> { $"555-{i:D4}", $"555-{i + 1000:D4}" },
				Addresses = new List<Address>
				{
					new() { Street = $"{i} Main St", City = "City A", State = "IL", ZipCode = 62700 + i },
					new() { Street = $"{i * 2} Oak Ave", City = "City B", State = "IL", ZipCode = 62800 + i }
				},
				FavoriteNumbers = new[] { i, i * 2, i * 3 }
			})
			.ToList();
	}

	[Benchmark(Description = "Map Simple Object (5 properties)")]
	public SimpleModelDto MapSimpleObject()
	{
		return _mapper.Map<SimpleModel, SimpleModelDto>(_simpleSource);
	}

	[Benchmark(Description = "Map Complex Object (20 properties)")]
	public ComplexModelDto MapComplexObject()
	{
		return _mapper.Map<ComplexModel, ComplexModelDto>(_complexSource);
	}

	[Benchmark(Description = "Map Object with Collections")]
	public PersonWithCollectionsDto MapObjectWithCollections()
	{
		return _mapper.Map<PersonWithCollections, PersonWithCollectionsDto>(_personWithCollections);
	}

	[Benchmark(Description = "Map Simple Collection (100 items)")]
	public List<SimpleModelDto> MapSimpleCollection()
	{
		return _mapper.Map<SimpleModel, SimpleModelDto>(_simpleList).ToList();
	}

	[Benchmark(Description = "Map Complex Collection (100 items)")]
	public List<ComplexModelDto> MapComplexCollection()
	{
		return _mapper.Map<ComplexModel, ComplexModelDto>(_complexList).ToList();
	}

	[Benchmark(Description = "Map Collection with Nested Collections (50 items)")]
	public List<PersonWithCollectionsDto> MapCollectionWithNestedCollections()
	{
		return _mapper.Map<PersonWithCollections, PersonWithCollectionsDto>(_personsWithCollections).ToList();
	}

	[Benchmark(Description = "Map to Existing Instance")]
	public SimpleModelDto MapToExistingInstance()
	{
		SimpleModelDto destination = new();
		return _mapper.Map(_simpleSource, destination);
	}

	[Benchmark(Description = "Map with Runtime Type Detection")]
	public SimpleModelDto? MapWithRuntimeTypeDetection()
	{
		object source = _simpleSource;
		return _mapper.Map<SimpleModelDto>(source);
	}
}

[MemoryDiagnoser]
public class ManualMappingComparison
{
	private SimpleModel _simpleSource = null!;
	private ComplexModel _complexSource = null!;
	private ISimpleMapper _mapper = null!;

	[GlobalSetup]
	public void Setup()
	{
		_mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		new BenchmarkMappingProfile();

		_simpleSource = new SimpleModel
		{
			Id = 1,
			Name = "John Doe",
			Email = "john.doe@example.com",
			Age = 30,
			IsActive = true
		};

		_complexSource = new ComplexModel
		{
			Id = 1,
			Name = "John Doe",
			Email = "john.doe@example.com",
			Phone = "555-1234",
			Address = "123 Main St",
			City = "Springfield",
			State = "IL",
			ZipCode = "62701",
			Country = "USA",
			CreatedDate = DateTime.Now,
			ModifiedDate = DateTime.Now,
			IsActive = true,
			IsDeleted = false,
			CreatedBy = 1,
			ModifiedBy = 1,
			Description = "Test description",
			Notes = "Some notes",
			Price = 99.99m,
			Quantity = 10,
			Category = "Electronics",
			Tags = "tag1,tag2,tag3"
		};
	}

	[Benchmark(Baseline = true, Description = "Manual Simple Mapping")]
	public SimpleModelDto ManualSimpleMapping()
	{
		return new SimpleModelDto
		{
			Id = _simpleSource.Id,
			Name = _simpleSource.Name,
			Email = _simpleSource.Email,
			Age = _simpleSource.Age,
			IsActive = _simpleSource.IsActive
		};
	}

	[Benchmark(Description = "SimpleMapper Simple Mapping")]
	public SimpleModelDto SimpleMapperMapping()
	{
		return _mapper.Map<SimpleModel, SimpleModelDto>(_simpleSource);
	}

	[Benchmark(Description = "Manual Complex Mapping")]
	public ComplexModelDto ManualComplexMapping()
	{
		return new ComplexModelDto
		{
			Id = _complexSource.Id,
			Name = _complexSource.Name,
			Email = _complexSource.Email,
			Phone = _complexSource.Phone,
			Address = _complexSource.Address,
			City = _complexSource.City,
			State = _complexSource.State,
			ZipCode = _complexSource.ZipCode,
			Country = _complexSource.Country,
			CreatedDate = _complexSource.CreatedDate,
			ModifiedDate = _complexSource.ModifiedDate,
			IsActive = _complexSource.IsActive,
			IsDeleted = _complexSource.IsDeleted,
			CreatedBy = _complexSource.CreatedBy,
			ModifiedBy = _complexSource.ModifiedBy,
			Description = _complexSource.Description,
			Notes = _complexSource.Notes,
			Price = _complexSource.Price,
			Quantity = _complexSource.Quantity,
			Category = _complexSource.Category,
			Tags = _complexSource.Tags
		};
	}

	[Benchmark(Description = "SimpleMapper Complex Mapping")]
	public ComplexModelDto SimpleMapperComplexMapping()
	{
		return _mapper.Map<ComplexModel, ComplexModelDto>(_complexSource);
	}
}
