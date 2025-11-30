# Fjeller.SimpleMapper - Project Architecture Documentation

## Overview

**Fjeller.SimpleMapper** is a lightweight object-to-object mapping library for .NET 10 that provides a simple, convention-based approach to mapping properties between objects. It serves as a streamlined alternative to more complex mapping libraries like AutoMapper, focusing on simplicity and performance.

## Target Framework
- **.NET 10** (net10.0)
- **C# 14.0** features enabled
- **Nullable reference types** enabled
- **Implicit usings** enabled

## Core Concepts

### What Problem Does This Library Solve?

The library solves the common problem of copying data between objects with similar structures, such as:
- Mapping domain entities to DTOs (Data Transfer Objects)
- Mapping API models to database entities
- Transforming data between different layers of an application
- Handling Entity Framework proxy types that have dynamically generated base classes

### Design Philosophy

1. **Convention-Based Mapping**: Automatically maps properties with matching names and types
2. **Explicit Configuration**: Supports explicit configuration through mapping profiles
3. **Type Safety**: Strongly typed with compile-time checks
4. **Fluent API**: Chainable methods for intuitive configuration
5. **Performance**: Minimal overhead with caching of mapping configurations
6. **Simplicity**: Straightforward API surface with minimal learning curve

## Architecture

### Core Components

#### 1. **SimpleMapper** (Main Entry Point)
- **Location**: `SimpleMapper.cs`
- **Purpose**: The primary mapper class that performs the actual object-to-object mapping
- **Key Responsibilities**:
  - Orchestrates the mapping process between source and destination objects
  - Retrieves mapping configurations from the cache
  - Uses reflection to copy property values
  - Executes post-mapping actions
  - Handles Entity Framework proxy types

**Key Methods**:
```csharp
// Map with explicit types
TDestination Map<TSource, TDestination>(TSource source, TDestination? destination)

// Map to new instance
TDestination Map<TSource, TDestination>(TSource source)

// Map with runtime type detection
TDestination? Map<TDestination>(object? source, TDestination? destination)
TDestination? Map<TDestination>(object? source)

// Map collections
IEnumerable<TDestination> Map<TSource, TDestination>(IEnumerable<TSource> source)
```

**Mapping Algorithm**:
1. Call `Prepare()` to ensure all mappings are initialized
2. Create destination object if null (requires parameterless constructor)
3. Retrieve mapping configuration from `SimpleMapCache`
4. Iterate through valid properties and copy values using reflection
5. Execute any post-mapping actions defined in the configuration
6. Return the populated destination object

#### 2. **MappingProfile** (Configuration Base Class)
- **Location**: `MappingProfile.cs`
- **Purpose**: Base class for defining mapping configurations
- **Key Responsibilities**:
  - Provides `CreateMap<TSource, TDestination>()` method
  - Registers maps in the global cache

**Usage Pattern**:
```csharp
public class MyMappingProfile : MappingProfile
{
    public MyMappingProfile()
    {
        CreateMap<SourceEntity, DestinationDto>()
            .IgnoreMember(x => x.SensitiveProperty)
            .ExecuteAfterMapping((src, dest) => dest.CalculatedField = src.Field1 + src.Field2);
    }
}
```

#### 3. **SimpleMap<TSource, TDestination>** (Mapping Configuration)
- **Location**: `Maps\SimpleMap.cs`
- **Purpose**: Internal class that stores the mapping configuration between two types
- **Key Responsibilities**:
  - Determines which properties can be mapped between source and destination
  - Tracks ignored properties
  - Stores post-mapping actions
  - Lazy-loads valid property lists

**Key Features**:
- **Property Matching Rules**:
  - Properties must have the same name
  - Properties must have the same type
  - Collections (except strings) are automatically excluded
  - Only properties that exist on both sides are mapped
  
- **Ignored Properties**: Maintains a list of properties explicitly excluded from mapping

- **Post-Mapping Actions**: Supports custom logic to execute after property mapping completes

**Configuration Methods**:
```csharp
ISimpleMap<TSource, TDestination> IgnoreMember(string memberName)
ISimpleMap<TSource, TDestination> IgnoreMember(Expression<Func<TSource, object>> sourceMember)
ISimpleMap<TSource, TDestination> IgnoreMembers(params string[] memberNames)
ISimpleMap<TSource, TDestination> ExecuteAfterMapping(Action<TSource, TDestination> action)
```

#### 4. **SimpleMapCache** (Mapping Storage)
- **Location**: `Storage\SimpleMapCache.cs`
- **Purpose**: Central storage and retrieval system for all mapping configurations
- **Key Responsibilities**:
  - Stores all registered mappings
  - Prevents duplicate mapping registrations
  - Provides fast lookup by type combination
  - Handles interface-based type matching
  - Lazy initialization of mapping configurations

**Key Features**:
- **Mapping Key Format**: `{SourceType.FullName}_{DestinationType.FullName}`
- **Interface Support**: Can match source types to their implemented interfaces
- **Source Type Lookup Cache**: Optimizes repeated lookups for interface-based mappings
- **Validation**: Throws `MappingKeyException` if duplicate mapping is attempted

**Internal Methods**:
```csharp
void AddMap(ISimpleMap map)              // Register a new mapping
ISimpleMap? GetMap(Type sourceType, Type destinationType)  // Retrieve mapping
Type? GetMatchingSourceType(Type destinationType, object source)  // Interface matching
void Prepare()                            // Initialize all mappings
```

### Interfaces

#### ISimpleMapper
- **Location**: `ISimpleMapper.cs`
- **Purpose**: Marker interface for the SimpleMapper (currently empty, designed for future DI scenarios)

#### ISimpleMap (Non-Generic)
- **Location**: `Maps\ISimpleMap.cs`
- **Purpose**: Base interface for mapping configurations
- **Members**:
  - `string MappingKey`: Unique identifier for the mapping
  - `List<PropertyInfo> ValidProperties`: Properties that will be mapped
  - `void CreateValidProperties()`: Initializes the valid properties list
  - `void ExecuteAfterMapAction(object source, object destination)`: Runs post-mapping logic

#### ISimpleMap<TSource, TDestination>
- **Location**: `Maps\ISimpleMap.Generic.cs`
- **Purpose**: Generic interface for strongly-typed mapping configurations
- **Extends**: `ISimpleMap`
- **Members**: Configuration methods (IgnoreMember, IgnoreMembers, ExecuteAfterMapping)

### Extension Methods

The library includes several internal extension methods that enhance functionality:

#### **ObjectExtensions**
- **Purpose**: Handle Entity Framework proxy types
- **Key Method**: `GetCorrectSourceType()` - Returns base type for EF proxies

#### **PropertyInfoExtensions**
- **Purpose**: Simplify property reflection operations
- **Key Methods**: 
  - `GetPropertyInfos()` - Get all properties from a type
  - `GetPropertyInfo(string)` - Get a specific property by name
  - Uses C# 14 **extension types** feature for cleaner syntax

#### **ExpressionExtensions**
- **Purpose**: Parse LINQ expressions to extract property information
- **Key Methods**:
  - `FindProperty()` - Extract PropertyInfo from lambda expressions
  - `FindMember()` - Navigate expression trees to find member references

#### **ListExtensions**
- **Purpose**: Add conditional list operations
- **Key Methods**:
  - `AddIfNotNull()` - Add item only if not null
  - `AddIfNotContains()` - Add item only if not already in list
  - Uses C# 14 **extension types** feature

#### **EnumerableExtensions**
- **Purpose**: Filter null values from sequences
- **Key Method**: `WhereNotNull()` - Filter out null items from IEnumerable
- Uses C# 14 **extension types** feature

#### **TupleExtensions**
- **Purpose**: Create mapping keys from type tuples
- **Key Method**: `CreateMapKey((Type, Type))` - Generate standardized mapping key

### Exception Types

#### **SimpleMapperException**
- **Location**: `Exceptions\SimpleMapperException.cs`
- **Purpose**: General exception for mapping errors
- **Usage**: Thrown when no valid mapping exists between types

#### **MappingKeyException**
- **Location**: `Exceptions\MappingKeyException.cs`
- **Purpose**: Exception for duplicate mapping registration
- **Usage**: Thrown when attempting to register a mapping that already exists

## Usage Workflow

### 1. Define Mapping Profiles

```csharp
public class UserMappingProfile : MappingProfile
{
    public UserMappingProfile()
    {
        CreateMap<UserEntity, UserDto>()
            .IgnoreMember(x => x.PasswordHash);
            
        CreateMap<UserDto, UserEntity>()
            .IgnoreMember(nameof(UserEntity.CreatedDate));
    }
}
```

### 2. Initialize Profiles (Startup)

```csharp
// Instantiate profiles to register mappings
new UserMappingProfile();
```

### 3. Perform Mapping

```csharp
var mapper = new SimpleMapper();

// Map to new instance
UserDto dto = mapper.Map<UserEntity, UserDto>(userEntity);

// Map to existing instance
mapper.Map(userEntity, existingDto);

// Map with runtime type detection
UserDto dto = mapper.Map<UserDto>(userEntity);

// Map collections
IEnumerable<UserDto> dtos = mapper.Map<UserEntity, UserDto>(userEntities);
```

## Key Design Patterns

### 1. **Fluent Interface Pattern**
- Chainable method calls for configuration
- Improves readability and discoverability

### 2. **Factory Pattern**
- `SimpleMap<TSource, TDestination>.Create()` creates new mapping instances

### 3. **Lazy Initialization**
- Valid properties are computed only when first needed
- `Prepare()` method ensures initialization before first use

### 4. **Cache-Aside Pattern**
- Mappings are stored in cache for fast retrieval
- Source type lookup cache optimizes interface matching

### 5. **Strategy Pattern**
- Post-mapping actions allow custom behavior injection

## Performance Considerations

### Optimization Strategies

1. **Reflection Caching**: Property information is computed once and cached
2. **Lazy Initialization**: Valid properties list is created only when needed
3. **Interface Lookup Cache**: Prevents repeated interface hierarchy traversal
4. **BindingFlags Constants**: Reuses flag combinations to avoid repeated allocations
5. **Collection Exclusion**: Automatically skips collection properties (except strings) to avoid complexity

### Reflection Usage

The library uses reflection for property copying with specific BindingFlags:
- `BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance`

This allows mapping of both public and private properties when needed.

## Extensibility Points

### Custom Post-Mapping Logic

```csharp
CreateMap<Source, Destination>()
    .ExecuteAfterMapping((src, dest) => 
    {
        // Custom logic here
        dest.ComputedValue = src.Value1 + src.Value2;
    });
```

### Property Exclusion

```csharp
// By name
CreateMap<Source, Destination>()
    .IgnoreMember("PropertyName");

// By expression
CreateMap<Source, Destination>()
    .IgnoreMember(x => x.PropertyName);

// Multiple properties
CreateMap<Source, Destination>()
    .IgnoreMembers("Prop1", "Prop2", "Prop3");
```

## Limitations and Constraints

### Current Limitations

1. **Collection Mapping**: Collections (except strings) are not automatically mapped
2. **Value Type Boxing**: Object-based mapping methods may cause boxing for value types
3. **Nested Objects**: Complex nested object graphs require separate mappings
4. **Constructor Parameters**: Destination types must have parameterless constructors
5. **Custom Type Conversion**: No built-in support for converting between different types

### Type Constraints

- Source types must be reference types (`where TSource : class`)
- Destination types must be reference types with parameterless constructors (`where TDestination : class, new()`)
- Properties must have identical names and types to be mapped

## Future Enhancement Opportunities

Based on the current architecture, potential enhancements could include:

1. **Value Type Support**: Remove class constraints to support structs
2. **Custom Type Converters**: Allow mapping between different property types
3. **Nested Object Mapping**: Automatically map complex object graphs
4. **Collection Mapping**: Support for mapping lists, arrays, and other collections
5. **Validation**: Built-in validation before/after mapping
6. **Async Support**: Async mapping for scenarios with async post-mapping actions
7. **Dependency Injection**: Better integration with DI containers
8. **Performance Profiling**: Built-in diagnostics for mapping performance
9. **Mapping Reverse**: Automatic reverse mapping generation
10. **Projection Support**: LINQ query projection support for Entity Framework

## Best Practices for Using This Library

### 1. Profile Organization
- Create one profile per domain or feature area
- Initialize all profiles at application startup
- Keep profiles focused and cohesive

### 2. Mapping Configuration
- Configure mappings once at startup, not at runtime
- Use explicit member exclusions rather than creating wrapper types
- Leverage post-mapping actions for computed properties

### 3. Performance
- Reuse mapper instances (they're stateless after configuration)
- Avoid mapping in tight loops when possible
- Consider manual mapping for performance-critical paths

### 4. Error Handling
- Always register mappings before first use
- Catch `SimpleMapperException` for missing mappings
- Validate destination objects after mapping if needed

### 5. Testing
- Test mapping profiles independently
- Verify ignored members are actually excluded
- Test post-mapping actions execute correctly

## Code Style and Conventions

The project follows these C# conventions:
- **PascalCase**: Public members, methods, types
- **camelCase**: Private fields (with underscore prefix like `_fieldName`)
- **ALL_UPPER**: Private constants (with underscore prefix like `_CONSTANT_NAME`)
- **File-scoped namespaces**: Modern C# style
- **Nullable reference types**: Enabled throughout
- **Extension types**: C# 14 feature used for extension methods
- **Pattern matching**: Used where applicable
- **Implicit usings**: Enabled for common namespaces

## Summary

Fjeller.SimpleMapper is a focused, convention-based object mapper designed for simplicity and performance. It excels at straightforward property-to-property mapping scenarios while providing flexibility through its fluent configuration API. The library's architecture prioritizes caching and lazy initialization to minimize runtime overhead while maintaining a clean, intuitive API surface.

When implementing new features or modifications, keep in mind:
- The library values convention over configuration
- Performance is achieved through caching, not code generation
- Type safety and compile-time checks are preferred
- The API should remain simple and discoverable
- Extension points should be clear and well-documented
