# API Reference

**Document Type:** Reference (Information-Oriented)  
**Purpose:** Complete technical reference for SimpleMapper API

This document provides detailed technical specifications for all public types, methods, and properties in SimpleMapper.

---

## Core Interfaces

### ISimpleMapper

The main mapper interface for performing object-to-object mappings.

**Namespace:** `Fjeller.SimpleMapper`

#### Methods

##### Map<TSource, TDestination>(TSource source, TDestination? destination)

Maps properties from source object to destination object.

**Signature:**
```csharp
TDestination Map<TSource, TDestination>(
    TSource source,
    TDestination? destination)
    where TSource : class
    where TDestination : class, new()
```

**Type Parameters:**
- `TSource` - Source object type
- `TDestination` - Destination object type

**Parameters:**
- `source` - Source object to map from
- `destination` - Destination object to map to (optional, created if null)

**Returns:** The destination object with mapped properties

**Exceptions:**
- `ArgumentException` - No mapping profile registered for types

**Example:**
```csharp
User user = GetUser();
UserDto dto = new UserDto();
_mapper.Map(user, dto);  // Maps to existing object
```

---

##### Map<TSource, TDestination>(TSource source)

Maps properties from source object to a new destination object.

**Signature:**
```csharp
TDestination Map<TSource, TDestination>(TSource source)
    where TSource : class
    where TDestination : class, new()
```

**Type Parameters:**
- `TSource` - Source object type
- `TDestination` - Destination object type

**Parameters:**
- `source` - Source object to map from

**Returns:** New destination object with mapped properties

**Exceptions:**
- `ArgumentException` - No mapping profile registered for types

**Example:**
```csharp
User user = GetUser();
UserDto dto = _mapper.Map<User, UserDto>(user);  // Creates new object
```

---

##### Map<TDestination>(object? source, TDestination? destination)

Maps from dynamic source type to destination type.

**Signature:**
```csharp
TDestination? Map<TDestination>(
    object? source,
    TDestination? destination)
    where TDestination : class, new()
```

**Type Parameters:**
- `TDestination` - Destination object type

**Parameters:**
- `source` - Source object (runtime type determined)
- `destination` - Destination object (optional)

**Returns:** Mapped destination object or null if source is null

**Exceptions:**
- `SimpleMapperException` - No matching mapping profile found

**Example:**
```csharp
object entity = GetEntity();  // Runtime type
UserDto dto = _mapper.Map<UserDto>(entity);
```

---

##### Map<TDestination>(object? source)

Maps from dynamic source type to new destination object.

**Signature:**
```csharp
TDestination? Map<TDestination>(object? source)
    where TDestination : class, new()
```

**Type Parameters:**
- `TDestination` - Destination object type

**Parameters:**
- `source` - Source object (runtime type determined)

**Returns:** New destination object or null if source is null

**Example:**
```csharp
object entity = GetEntity();
UserDto dto = _mapper.Map<UserDto>(entity);
```

---

##### Map<TSource, TDestination>(IEnumerable<TSource> source)

Maps collection of source objects to collection of destination objects.

**Signature:**
```csharp
IEnumerable<TDestination> Map<TSource, TDestination>(
    IEnumerable<TSource> source)
    where TSource : class
    where TDestination : class, new()
```

**Type Parameters:**
- `TSource` - Source element type
- `TDestination` - Destination element type

**Parameters:**
- `source` - Source collection

**Returns:** IEnumerable of mapped destination objects

**Example:**
```csharp
List<User> users = GetUsers();
IEnumerable<UserDto> dtos = _mapper.Map<User, UserDto>(users);
List<UserDto> dtoList = dtos.ToList();
```

---

### ISimpleMap<TSource, TDestination>

Interface for configuring mappings between specific types.

**Namespace:** `Fjeller.SimpleMapper.Maps`

#### Methods

##### IgnoreMember(Expression<Func<TSource, object>> sourceMember)

Ignores a property during mapping using type-safe expression.

**Signature:**
```csharp
ISimpleMap<TSource, TDestination> IgnoreMember(
    Expression<Func<TSource, object>> sourceMember)
```

**Parameters:**
- `sourceMember` - Expression selecting property to ignore

**Returns:** The map for method chaining

**Example:**
```csharp
CreateMap<User, UserDto>()
    .IgnoreMember(x => x.Password);
```

---

##### IgnoreMember(string memberName)

Ignores a property by name.

**Signature:**
```csharp
ISimpleMap<TSource, TDestination> IgnoreMember(string memberName)
```

**Parameters:**
- `memberName` - Name of property to ignore

**Returns:** The map for method chaining

**Example:**
```csharp
CreateMap<User, UserDto>()
    .IgnoreMember(nameof(User.Password));
```

---

##### IgnoreMembers(params string[] memberNames)

Ignores multiple properties by name.

**Signature:**
```csharp
ISimpleMap<TSource, TDestination> IgnoreMembers(
    params string[] memberNames)
```

**Parameters:**
- `memberNames` - Names of properties to ignore

**Returns:** The map for method chaining

**Example:**
```csharp
CreateMap<User, UserDto>()
    .IgnoreMembers(
        nameof(User.Password),
        nameof(User.PasswordHash),
        nameof(User.SecurityStamp)
    );
```

---

##### ExecuteAfterMapping(Action<TSource, TDestination> action)

Executes custom logic after property mapping.

**Signature:**
```csharp
ISimpleMap<TSource, TDestination> ExecuteAfterMapping(
    Action<TSource, TDestination> action)
```

**Parameters:**
- `action` - Delegate to execute after mapping

**Returns:** The map for method chaining

**Example:**
```csharp
CreateMap<User, UserDto>()
    .ExecuteAfterMapping((src, dest) =>
    {
        dest.FullName = $"{src.FirstName} {src.LastName}";
    });
```

---

## Base Classes

### MappingProfile

Abstract base class for defining mapping configurations.

**Namespace:** `Fjeller.SimpleMapper`

#### Methods

##### CreateMap<TSource, TDestination>()

Creates a mapping configuration between two types.

**Signature:**
```csharp
protected ISimpleMap<TSource, TDestination> CreateMap<TSource, TDestination>()
    where TSource : class
    where TDestination : class, new()
```

**Type Parameters:**
- `TSource` - Source type
- `TDestination` - Destination type

**Returns:** Map configuration for fluent configuration

**Example:**
```csharp
public class UserMappingProfile : MappingProfile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>();
    }
}
```

---

## Configuration Classes

### SimpleMapperOptions

Configuration options for dependency injection registration.

**Namespace:** `Fjeller.SimpleMapper.DependencyInjection`

#### Methods

##### AddProfiles(params MappingProfile[] profiles)

Adds explicit mapping profiles.

**Signature:**
```csharp
SimpleMapperOptions AddProfiles(params MappingProfile[] profiles)
```

**Parameters:**
- `profiles` - Profiles to register

**Returns:** Options for method chaining

**Example:**
```csharp
options.AddProfiles(
    new UserProfile(),
    new ProductProfile()
);
```

---

##### AddProfiles(params Assembly[] assemblies)

Scans assemblies for mapping profiles.

**Signature:**
```csharp
SimpleMapperOptions AddProfiles(params Assembly[] assemblies)
```

**Parameters:**
- `assemblies` - Assemblies to scan

**Returns:** Options for method chaining

**Example:**
```csharp
options.AddProfiles(typeof(Program).Assembly);
```

---

##### AddProfilesFromAssembliesContaining(params Type[] markerTypes)

Scans assemblies containing marker types.

**Signature:**
```csharp
SimpleMapperOptions AddProfilesFromAssembliesContaining(
    params Type[] markerTypes)
```

**Parameters:**
- `markerTypes` - Types whose assemblies should be scanned

**Returns:** Options for method chaining

**Example:**
```csharp
options.AddProfilesFromAssembliesContaining(
    typeof(User),
    typeof(Product)
);
```

---

##### AddProfilesFromAssemblyContaining<TMarker>()

Scans assembly containing specific type.

**Signature:**
```csharp
SimpleMapperOptions AddProfilesFromAssemblyContaining<TMarker>()
```

**Type Parameters:**
- `TMarker` - Type whose assembly should be scanned

**Returns:** Options for method chaining

**Example:**
```csharp
options.AddProfilesFromAssemblyContaining<Program>();
```

---

## Extension Methods

### IServiceCollection Extensions

Extension methods for dependency injection registration.

**Namespace:** `Fjeller.SimpleMapper.DependencyInjection`

#### AddSimpleMapper(params MappingProfile[] profiles)

Registers SimpleMapper with explicit profiles.

**Signature:**
```csharp
static IServiceCollection AddSimpleMapper(
    this IServiceCollection services,
    params MappingProfile[] profiles)
```

**Parameters:**
- `services` - Service collection
- `profiles` - Profiles to register

**Returns:** Service collection for chaining

**Example:**
```csharp
builder.Services.AddSimpleMapper(
    new UserProfile(),
    new ProductProfile()
);
```

---

#### AddSimpleMapper(Action<SimpleMapperOptions>? configureOptions)

Registers SimpleMapper with configuration.

**Signature:**
```csharp
static IServiceCollection AddSimpleMapper(
    this IServiceCollection services,
    Action<SimpleMapperOptions>? configureOptions)
```

**Parameters:**
- `services` - Service collection
- `configureOptions` - Configuration delegate

**Returns:** Service collection for chaining

**Example:**
```csharp
builder.Services.AddSimpleMapper(options =>
{
    options.AddProfiles(typeof(Program).Assembly);
});
```

---

#### AddSimpleMapper(params Assembly[] assemblies)

Registers SimpleMapper with assembly scanning.

**Signature:**
```csharp
static IServiceCollection AddSimpleMapper(
    this IServiceCollection services,
    params Assembly[] assemblies)
```

**Parameters:**
- `services` - Service collection
- `assemblies` - Assemblies to scan

**Returns:** Service collection for chaining

**Example:**
```csharp
builder.Services.AddSimpleMapper(typeof(Program).Assembly);
```

---

#### AddSimpleMapperFromAssemblyContaining<TMarker>()

Registers with type-safe assembly reference.

**Signature:**
```csharp
static IServiceCollection AddSimpleMapperFromAssemblyContaining<TMarker>(
    this IServiceCollection services)
```

**Type Parameters:**
- `TMarker` - Type whose assembly should be scanned

**Parameters:**
- `services` - Service collection

**Returns:** Service collection for chaining

**Example:**
```csharp
builder.Services.AddSimpleMapperFromAssemblyContaining<Program>();
```

---

#### AddSimpleMapperFromCallingAssembly()

Registers with calling assembly scanning.

**Signature:**
```csharp
static IServiceCollection AddSimpleMapperFromCallingAssembly(
    this IServiceCollection services)
```

**Parameters:**
- `services` - Service collection

**Returns:** Service collection for chaining

**Example:**
```csharp
builder.Services.AddSimpleMapperFromCallingAssembly();
```

---

## Exceptions

### SimpleMapperException

Base exception for SimpleMapper-specific errors.

**Namespace:** `Fjeller.SimpleMapper.Exceptions`

**Inheritance:** `Exception`

**Common Scenarios:**
- No matching mapping profile found for dynamic type mapping
- Profile misconfiguration

---

## Type Compatibility

### Supported Types

SimpleMapper automatically maps properties when:
- ? Property names match (case-sensitive)
- ? Property types match exactly
- ? Properties are public
- ? Destination property is writable

### Supported Property Types

- **Primitives**: `int`, `long`, `double`, `float`, `bool`, `byte`, etc.
- **Strings**: `string`
- **Value Types**: `decimal`, `DateTime`, `DateTimeOffset`, `Guid`, `TimeSpan`
- **Nullable Types**: `int?`, `DateTime?`, etc.
- **Complex Types**: Any class with parameterless constructor
- **Collections**: `List<T>`, `T[]`, `IEnumerable<T>`

### Unsupported Scenarios

- ? Properties with different names (use `ExecuteAfterMapping`)
- ? Properties with incompatible types (use `ExecuteAfterMapping`)
- ? Private or internal properties (unless using reflection flags)
- ? Read-only properties (destination must be writable)

---

## Service Lifetime

### Default Registration

SimpleMapper is registered as **Singleton**:
```csharp
services.TryAddSingleton<ISimpleMapper>(...)
```

### Characteristics

- **Thread-Safe**: Yes (uses `ConcurrentDictionary`)
- **Stateless**: No per-request state
- **Optimal**: Best performance and memory efficiency

---

## Performance Characteristics

### Compilation

- **First Mapping**: ~5-10ms (expression tree compilation)
- **Subsequent**: ~80-90ns (compiled expression execution)
- **Cached**: Forever (singleton lifetime)

### Memory

- **Per Instance**: ~1-2 KB (singleton overhead)
- **Per Compiled Mapper**: ~1-2 KB
- **Total for 100 Mappings**: ~100-200 KB

---

## Next Steps

- **[Configuration Reference](_reference_configuration.md)** - Detailed configuration options
- **[How-to Guides](_howto_dependency_injection.md)** - Practical examples
- **[Architecture](_explanation_architecture.md)** - Internal design
