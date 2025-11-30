# SimpleMapper for .NET

A fast, lightweight object-to-object mapper for .NET 10 with built-in dependency injection support and expression tree compilation for optimal performance.

## Why SimpleMapper?

- **?? Fast**: Uses compiled expression trees - up to 6x faster than reflection-based mapping
- **?? Simple**: Clean, intuitive API - map objects in one line
- **?? DI Ready**: First-class ASP.NET Core integration with assembly scanning
- **?? Lightweight**: Minimal dependencies, focused feature set
- **? Type-Safe**: Compile-time checking with generic methods
- **?? Flexible**: Support for collections, nested objects, and custom transformations

## Performance at a Glance

```
Simple Object Mapping:   80 ns  (1.8x faster than baseline)
Complex Object Mapping:  90 ns  (6.2x faster than baseline)
Memory Efficient:        Singleton with compiled expression caching
```

## Quick Start

### Installation

```bash
dotnet add package Fjeller.SimpleMapper
```

### Basic Usage

```csharp
// 1. Define your models
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

// 2. Create a mapping profile
public class UserMappingProfile : MappingProfile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>();
    }
}

// 3. Register in dependency injection
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSimpleMapper(typeof(Program).Assembly);

// 4. Inject and use
public class UsersController : ControllerBase
{
    private readonly ISimpleMapper _mapper;
    
    public UsersController(ISimpleMapper mapper)
    {
        _mapper = mapper;
    }
    
    [HttpGet("{id}")]
    public ActionResult<UserDto> GetUser(int id)
    {
        User user = GetUserFromDatabase(id);
        UserDto dto = _mapper.Map<User, UserDto>(user);
        return Ok(dto);
    }
}
```

**That's it!** You're mapping objects in production-ready code.

## Key Features

### Object-to-Object Mapping
Map between any two compatible types with automatic property matching by name and type.

### Dependency Injection
```csharp
// Assembly scanning - automatic profile discovery
builder.Services.AddSimpleMapper(typeof(Program).Assembly);

// Explicit profiles
builder.Services.AddSimpleMapper(new UserProfile(), new ProductProfile());

// Configuration options
builder.Services.AddSimpleMapper(options =>
{
    options.AddProfiles(typeof(Program).Assembly);
});
```

### Collection Mapping
```csharp
IEnumerable<User> users = GetUsers();
IEnumerable<UserDto> dtos = _mapper.Map<User, UserDto>(users);
```

### Property Ignoring
```csharp
CreateMap<User, UserDto>()
    .IgnoreMember(x => x.Password)
    .IgnoreMember(x => x.InternalId);
```

### After-Mapping Actions
```csharp
CreateMap<CreateUserDto, User>()
    .ExecuteAfterMapping((src, dest) =>
    {
        dest.CreatedAt = DateTime.UtcNow;
        dest.IsActive = true;
    });
```

### Compiled Expression Trees
SimpleMapper compiles mappings to IL code at runtime, eliminating reflection overhead for near-manual mapping performance.

## Documentation

### ?? Learning Path

**New to SimpleMapper?** Start here:
- **[Getting Started Tutorial](__documentation/_tutorial_getting_started.md)** - Learn SimpleMapper in 15 minutes

### ?? How-to Guides

Solve specific problems:
- **[Dependency Injection](__documentation/_howto_dependency_injection.md)** - Configure DI in ASP.NET Core
- **[Mapping Profiles](__documentation/_howto_mapping_profiles.md)** - Create and configure profiles
- **[Collections & Nested Objects](__documentation/_howto_collections.md)** - Map complex structures
- **[Troubleshooting](__documentation/_howto_troubleshooting.md)** - Fix common issues

### ?? Reference

Look up specific details:
- **[API Reference](__documentation/_reference_api.md)** - Complete API documentation
- **[Configuration Reference](__documentation/_reference_configuration.md)** - All configuration options

### ?? Understanding SimpleMapper

Deep dive into concepts:
- **[Architecture & Design](__documentation/_explanation_architecture.md)** - How it works internally
- **[Performance Characteristics](__documentation/_explanation_performance.md)** - Benchmarks and optimization

## Requirements

- .NET 10 or later
- C# 14 or later

## Supported Scenarios

? ASP.NET Core Web API  
? ASP.NET Core MVC  
? Blazor Server  
? Blazor WebAssembly  
? Minimal APIs  
? Console Applications  
? Desktop Applications  

## Compatible Types

- Primitive types (int, string, bool, etc.)
- Value types (decimal, DateTime, Guid, etc.)
- Complex objects (classes with properties)
- Collections (List<T>, T[], IEnumerable<T>)
- Nested objects (deep mapping)

## Examples

### Map with Existing Destination
```csharp
User user = GetUser();
UserDto existingDto = new UserDto();
_mapper.Map(user, existingDto); // Updates existing object
```

### Map Collections
```csharp
List<Product> products = GetProducts();
List<ProductDto> dtos = _mapper.Map<Product, ProductDto>(products).ToList();
```

### Dynamic Source Type
```csharp
object source = GetEntity(); // Runtime type
UserDto dto = _mapper.Map<UserDto>(source);
```

### Ignore Multiple Properties
```csharp
CreateMap<User, UserDto>()
    .IgnoreMembers(
        nameof(User.Password),
        nameof(User.PasswordHash),
        nameof(User.SecurityStamp)
    );
```

## Performance Tips

1. **Use Assembly Scanning**: Profiles are discovered automatically at startup
2. **Singleton Lifetime**: Default registration is optimal - don't change it
3. **Compiled Expressions**: First mapping compiles (~5-10ms), subsequent mappings are fast (~80-90ns)
4. **Collections**: Deep mapping of complex collection elements uses reflection (acceptable trade-off for flexibility)

## Contributing

Contributions are welcome! Please:
1. Fork the repository
2. Create a feature branch
3. Follow existing code style
4. Include tests for new features
5. Submit a pull request

## License

[Specify your license here]

## Links

- **GitHub**: [https://github.com/fjeller/fjeller.simplemapper](https://github.com/fjeller/fjeller.simplemapper)
- **NuGet**: [Coming soon]
- **Issues**: [https://github.com/fjeller/fjeller.simplemapper/issues](https://github.com/fjeller/fjeller.simplemapper/issues)

## Support

- ?? Check the [documentation](__documentation/)
- ?? Report issues on [GitHub Issues](https://github.com/fjeller/fjeller.simplemapper/issues)
- ?? Ask questions in [Discussions](https://github.com/fjeller/fjeller.simplemapper/discussions)

---

**Built with ?? for the .NET community**