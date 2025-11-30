using Fjeller.SimpleMapper;

namespace Benchmarks.Fjeller.SimpleMapper.Models;

public class SimpleModel
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public int Age { get; set; }
	public bool IsActive { get; set; }
}

public class SimpleModelDto
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public int Age { get; set; }
	public bool IsActive { get; set; }
}

public class ComplexModel
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string Phone { get; set; } = string.Empty;
	public string Address { get; set; } = string.Empty;
	public string City { get; set; } = string.Empty;
	public string State { get; set; } = string.Empty;
	public string ZipCode { get; set; } = string.Empty;
	public string Country { get; set; } = string.Empty;
	public DateTime CreatedDate { get; set; }
	public DateTime ModifiedDate { get; set; }
	public bool IsActive { get; set; }
	public bool IsDeleted { get; set; }
	public int CreatedBy { get; set; }
	public int ModifiedBy { get; set; }
	public string Description { get; set; } = string.Empty;
	public string Notes { get; set; } = string.Empty;
	public decimal Price { get; set; }
	public int Quantity { get; set; }
	public string Category { get; set; } = string.Empty;
	public string Tags { get; set; } = string.Empty;
}

public class ComplexModelDto
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string Phone { get; set; } = string.Empty;
	public string Address { get; set; } = string.Empty;
	public string City { get; set; } = string.Empty;
	public string State { get; set; } = string.Empty;
	public string ZipCode { get; set; } = string.Empty;
	public string Country { get; set; } = string.Empty;
	public DateTime CreatedDate { get; set; }
	public DateTime ModifiedDate { get; set; }
	public bool IsActive { get; set; }
	public bool IsDeleted { get; set; }
	public int CreatedBy { get; set; }
	public int ModifiedBy { get; set; }
	public string Description { get; set; } = string.Empty;
	public string Notes { get; set; } = string.Empty;
	public decimal Price { get; set; }
	public int Quantity { get; set; }
	public string Category { get; set; } = string.Empty;
	public string Tags { get; set; } = string.Empty;
}

public class Address
{
	public string Street { get; set; } = string.Empty;
	public string City { get; set; } = string.Empty;
	public string State { get; set; } = string.Empty;
	public int ZipCode { get; set; }
}

public class AddressDto
{
	public string Street { get; set; } = string.Empty;
	public string City { get; set; } = string.Empty;
	public string State { get; set; } = string.Empty;
	public int ZipCode { get; set; }
}

public class PersonWithCollections
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public List<string> PhoneNumbers { get; set; } = new();
	public List<Address> Addresses { get; set; } = new();
	public int[] FavoriteNumbers { get; set; } = Array.Empty<int>();
}

public class PersonWithCollectionsDto
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public List<string> PhoneNumbers { get; set; } = new();
	public List<AddressDto> Addresses { get; set; } = new();
	public int[] FavoriteNumbers { get; set; } = Array.Empty<int>();
}

public class BenchmarkMappingProfile : MappingProfile
{
	public BenchmarkMappingProfile()
	{
		CreateMap<SimpleModel, SimpleModelDto>();
		CreateMap<ComplexModel, ComplexModelDto>();
		CreateMap<PersonWithCollections, PersonWithCollectionsDto>();
		CreateMap<Address, AddressDto>();
	}
}
