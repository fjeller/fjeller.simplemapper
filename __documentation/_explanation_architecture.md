# Architecture and Design

**Document Type:** Explanation (Understanding-Oriented)  
**Purpose:** Understand how SimpleMapper works internally and why design decisions were made

---

## Design Philosophy

SimpleMapper is built on three core principles:

1. **Simplicity First** - Clean API, minimal configuration
2. **Performance-Conscious** - Fast execution without compromising usability
3. **Convention Over Configuration** - Sensible defaults, explicit when needed

---

## Core Architecture

### High-Level Overview

```
???????????????????????????????????????????????????????????????
?                      ISimpleMapper                          ?
?                  (Public API Interface)                     ?
???????????????????????????????????????????????????????????????
                       ?
???????????????????????????????????????????????????????????????
?                    SimpleMapper                             ?
?              (Main Implementation Class)                     ?
?                                                              ?
?  ???????????????  ????????????????  ????????????????????  ?
?  ?   Profile   ?  ?   Compiled   ?  ?    Collection    ?  ?
?  ?   System    ?  ?   Mappers    ?  ?     Mapping      ?  ?
?  ???????????????  ????????????????  ????????????????????  ?
???????????????????????????????????????????????????????????????
                        ?
        ?????????????????????????????????
        ?               ?               ?
???????????????  ?????????????????  ????????????????
? SimpleMap   ?  ? CompiledMap   ?  ?  SimpleMap   ?
?   Cache     ?  ?    Cache      ?  ?   Storage    ?
???????????????  ?????????????????  ????????????????
```

---

## Core Components

### 1. Mapping Profile System

**Purpose:** Define mapping rules between types

**Design:**
```csharp
public abstract class MappingProfile
{
    protected ISimpleMap<TSource, TDest> CreateMap<TSource, TDest>()
    {
        // Creates and registers mapping configuration
        // Stores in SimpleMapCache
    }
}
```

**Why This Design:**
- **Explicit Over Implicit**: Forces developers to declare mappings
- **Discoverable**: Can scan assemblies for subclasses
- **Flexible**: Supports configuration through fluent API
- **Testable**: Profiles can be tested in isolation

**Alternatives Considered:**
- ? Attribute-based (`[MapFrom]`, `[MapTo]`) - Clutters domain models
- ? Convention-based only - Hard to debug, unpredictable
- ? Profile-based - Best balance of explicitness and convenience

---

### 2. Expression Tree Compilation

**Purpose:** Generate fast compiled mappers

**How It Works:**

1. **First Mapping Request:**
   ```
   User Request
   ? Check CompiledMapCache
   ? Not Found
   ? Generate Expression Tree
   ? Compile to IL Code
   ? Cache Forever
   ? Execute
   ```

2. **Subsequent Requests:**
   ```
   User Request
   ? Check CompiledMapCache
   ? Found!
   ? Execute Cached Mapper (~80-90ns)
   ```

**Implementation:**
```csharp
// Simplified version
Expression<Func<TSource, TDest, TDest>> BuildExpression()
{
    var sourceParam = Expression.Parameter(typeof(TSource));
    var destParam = Expression.Parameter(typeof(TDest));
    
    var assignments = validProperties
        .Select(prop => Expression.Assign(
            Expression.Property(destParam, prop.Name),
            Expression.Property(sourceParam, prop.Name)
        ))
        .ToList();
    
    var block = Expression.Block(assignments);
    return Expression.Lambda<Func<TSource, TDest, TDest>>(
        block, sourceParam, destParam
    );
}
```

**Why Expression Trees:**
- ? Near-native performance (compiled to IL)
- ? Type-safe
- ? One-time compilation cost
- ? More complex than reflection
- ? Initial compilation overhead (~5-10ms)

**Alternatives:**
- ? Pure Reflection - Too slow (50x+ slower)
- ? Source Generators - Compile-time only, less flexible
- ? Expression Trees - Best runtime performance

---

### 3. Caching Strategy

**Two-Level Cache:**

#### Level 1: SimpleMapCache (Profile Storage)
```csharp
// Stores mapping configurations
ConcurrentDictionary<(Type Source, Type Dest), ISimpleMap>
```

- **Purpose**: Store profile-defined mappings
- **Lifetime**: Application lifetime (singleton)
- **Thread-Safety**: `ConcurrentDictionary`

#### Level 2: CompiledMapCache (Compiled Expressions)
```csharp
// Stores compiled mappers
ConcurrentDictionary<(Type Source, Type Dest), Delegate>
```

- **Purpose**: Store compiled expression trees
- **Lifetime**: Application lifetime (singleton)
- **Thread-Safety**: `ConcurrentDictionary`

**Why Two Caches:**
- Profile cache: Fast lookup for configuration
- Compiled cache: Lazy compilation (only compile when first used)

---

### 4. Collection Mapping Pipeline

**Design Decision:** Use reflection for collections

**Why:**
```csharp
// Problem: Collection element types unknown at compile time
List<object> items = GetItems();  // Could be List<User>, List<Product>, etc.

// Solution: Reflection-based deep mapping
foreach (object item in items)
{
    Type itemType = item.GetType();  // Runtime discovery
    ISimpleMap? map = SimpleMapCache.GetMap(itemType, destElementType);
    // Map using discovered profile
}
```

**Trade-Offs:**
- ? Flexibility: Works with any collection structure
- ? Deep Mapping: Automatically handles nested objects
- ? Performance: Slower than compiled expressions
- ? Acceptable: Collections are typically smaller datasets

---

## Design Decisions

### Why Singleton Lifetime?

**Decision:** Register as singleton by default

**Rationale:**
1. **Thread-Safe**: All caches use `ConcurrentDictionary`
2. **Stateless**: No mutable per-request state
3. **Performance**: Compiled expressions shared across requests
4. **Memory**: Single instance for entire application

**Code:**
```csharp
services.TryAddSingleton<ISimpleMapper>(serviceProvider =>
{
    var mapper = new SimpleMapper();
    // Initialize with profiles
    return mapper;
});
```

---

### Why Not Attribute-Based Configuration?

**Considered:**
```csharp
public class UserDto
{
    [MapFrom("User.FirstName")]
    public string Name { get; set; }
}
```

**Rejected Because:**
- ? Pollutes domain models with mapping concerns
- ? DTOs know about entities (wrong dependency direction)
- ? Hard to test mappings in isolation
- ? Difficult to have multiple mappings for same type

**Our Approach (Profiles):**
```csharp
public class UserMappingProfile : MappingProfile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>();
    }
}
```

**Benefits:**
- ? Separation of concerns
- ? Domain models stay clean
- ? Easy to test
- ? Multiple mappings possible

---

### Why No Automatic Bidirectional Mapping?

**Considered:**
```csharp
CreateMap<User, UserDto>();  // Automatically creates reverse?
```

**Rejected Because:**
- ? Mapping often asymmetric (different rules each direction)
- ? DTO ? Entity may need validation
- ? Implicit behavior is confusing

**Our Approach:**
```csharp
CreateMap<User, UserDto>();     // Explicit
CreateMap<UserDto, User>();     // Explicit
```

**Benefits:**
- ? Explicit intent
- ? Different rules per direction
- ? No surprises

---

### Why Property Name Matching Only?

**Considered:** Auto-flatten, auto-map different names

**Rejected Because:**
- ? "Magic" behavior is hard to debug
- ? Unpredictable in edge cases
- ? Performance cost of heuristics

**Our Approach:**
```csharp
CreateMap<User, UserDto>()
    .ExecuteAfterMapping((src, dest) =>
    {
        dest.FullName = $"{src.FirstName} {src.LastName}";
    });
```

**Benefits:**
- ? Explicit transformations
- ? No hidden logic
- ? Easy to understand

---

## Extension Points

### Custom Mapping Logic

Use `ExecuteAfterMapping` for custom transformations:

```csharp
CreateMap<Order, OrderDto>()
    .ExecuteAfterMapping((src, dest) =>
    {
        dest.Total = src.Items.Sum(i => i.Price * i.Quantity);
    });
```

### Custom Type Converters

Currently not supported. Workaround:

```csharp
CreateMap<Source, Dest>()
    .ExecuteAfterMapping((src, dest) =>
    {
        dest.CustomField = CustomConverter.Convert(src.CustomField);
    });
```

---

## Performance Architecture

### Cold Start (First Mapping)

```
Request
? Check CompiledMapCache (miss)
? Get profile from SimpleMapCache
? Build expression tree (~2-3ms)
? Compile to IL (~3-5ms)
? Cache compiled mapper
? Execute (~80-90ns)

Total: ~5-10ms
```

### Warm Path (Subsequent Mappings)

```
Request
? Check CompiledMapCache (hit)
? Execute cached mapper

Total: ~80-90ns
```

### Memory Layout

```
SimpleMapper Instance:               ~1-2 KB
?? SimpleMapCache
?  ?? 100 Profile Configs:          ~50 KB
?? CompiledMapCache
?  ?? 100 Compiled Mappers:         ~100 KB
?? Internal State:                   ~10 KB

Total for 100 Mappings:              ~161 KB
```

---

## Comparison of Approaches

| Approach | Performance | Flexibility | Complexity |
|----------|-------------|-------------|------------|
| **Reflection** | ? Slow (1000ns) | ? High | ? Simple |
| **Expression Trees** | ? Fast (80ns) | ? High | ?? Medium |
| **Source Generators** | ? Fastest (50ns) | ? Low | ? Complex |
| **Manual Code** | ? Fastest (50ns) | ? None | ? Simple |

**SimpleMapper Choice:** Expression Trees
- Best balance of performance and flexibility
- Runtime compilation allows dynamic scenarios
- Acceptable one-time compilation cost

---

## Design Patterns Used

### 1. Builder Pattern
```csharp
CreateMap<User, UserDto>()
    .IgnoreMember(x => x.Password)
    .ExecuteAfterMapping((src, dest) => { });
```

### 2. Singleton Pattern
```csharp
services.TryAddSingleton<ISimpleMapper>(...)
```

### 3. Factory Pattern
```csharp
CompiledMapCache.GetOrCreateMapper<TSource, TDest>(profile)
```

### 4. Template Method Pattern
```csharp
public abstract class MappingProfile
{
    protected ISimpleMap<TSource, TDest> CreateMap<TSource, TDest>()
}
```

---

## Future Considerations

### Potential Enhancements

1. **Source Generators** (opt-in)
   - Compile-time code generation
   - Zero reflection, zero expression compilation
   - Trade-off: Less flexible

2. **Custom Type Converters**
   - Register converters for specific type pairs
   - Handle complex transformations

3. **Value Resolvers**
   - Reusable property resolution logic
   - Reduce duplication

---

## Summary

SimpleMapper's architecture prioritizes:

1. **Simplicity**: Clean API, profile-based configuration
2. **Performance**: Expression tree compilation
3. **Flexibility**: Runtime dynamic mapping
4. **Maintainability**: Clear separation of concerns

**Key Design Decisions:**
- ? Explicit profiles over attributes
- ? Expression trees over reflection
- ? Singleton lifetime for caching
- ? Fluent configuration API

---

## Next Steps

- **[Performance Characteristics](_explanation_performance.md)** - Understand benchmarks
- **[API Reference](_reference_api.md)** - Technical specifications
- **[How-to Guides](_howto_dependency_injection.md)** - Practical usage
