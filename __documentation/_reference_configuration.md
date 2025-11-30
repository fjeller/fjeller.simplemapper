# Configuration Reference

**Document Type:** Reference (Information-Oriented)  
**Purpose:** Complete reference for all configuration options

---

## Dependency Injection Configuration

### Registration Methods

| Method | Parameters | Use Case |
|--------|------------|----------|
| `AddSimpleMapper(profiles)` | `params MappingProfile[]` | Explicit profiles |
| `AddSimpleMapper(configure)` | `Action<SimpleMapperOptions>` | Advanced config |
| `AddSimpleMapper(assemblies)` | `params Assembly[]` | Assembly scanning |
| `AddSimpleMapperFromAssemblyContaining<T>()` | Generic type parameter | Type-safe scanning |
| `AddSimpleMapperFromAssemblyContaining(types)` | `params Type[]` | Multiple assemblies |
| `AddSimpleMapperFromCallingAssembly()` | None | Auto-detect assembly |

### Service Lifetime

**Default:** Singleton

```csharp
services.TryAddSingleton<ISimpleMapper>(...)
```

**Why Singleton:**
- Thread-safe implementation
- Compiled expressions cached globally
- Stateless design
- Optimal performance

**Changing Lifetime (Not Recommended):**
```csharp
// Don't do this - wastes resources
services.AddScoped<ISimpleMapper, SimpleMapper>();
services.AddTransient<ISimpleMapper, SimpleMapper>();
```

---

## SimpleMapperOptions

### Properties

None (all configuration via methods)

### Methods

#### AddProfiles(params MappingProfile[] profiles)

**Purpose:** Register explicit profiles

**Parameters:**
- `profiles` - Array of profile instances

**Returns:** `SimpleMapperOptions` (fluent)

**Example:**
```csharp
options.AddProfiles(
    new UserProfile(),
    new ProductProfile()
);
```

---

#### AddProfiles(params Assembly[] assemblies)

**Purpose:** Scan assemblies for profiles

**Parameters:**
- `assemblies` - Assemblies to scan

**Returns:** `SimpleMapperOptions` (fluent)

**Discovery Rules:**
- Must inherit `MappingProfile`
- Must be public, non-abstract class
- Must have parameterless constructor

**Example:**
```csharp
options.AddProfiles(
    typeof(Program).Assembly,
    typeof(User).Assembly
);
```

---

#### AddProfilesFromAssembliesContaining(params Type[] markerTypes)

**Purpose:** Scan by marker types

**Parameters:**
- `markerTypes` - Types whose assemblies to scan

**Returns:** `SimpleMapperOptions` (fluent)

**Example:**
```csharp
options.AddProfilesFromAssembliesContaining(
    typeof(Program),
    typeof(User)
);
```

---

#### AddProfilesFromAssemblyContaining<TMarker>()

**Purpose:** Type-safe single assembly scanning

**Type Parameters:**
- `TMarker` - Type in assembly to scan

**Returns:** `SimpleMapperOptions` (fluent)

**Example:**
```csharp
options.AddProfilesFromAssemblyContaining<Program>();
```

---

## Mapping Profile Configuration

### CreateMap<TSource, TDestination>()

**Purpose:** Declare mapping between types

**Type Constraints:**
- `TSource: class`
- `TDestination: class, new()`

**Returns:** `ISimpleMap<TSource, TDestination>`

**Example:**
```csharp
CreateMap<User, UserDto>();
```

---

### IgnoreMember(Expression<Func<TSource, object>> sourceMember)

**Purpose:** Ignore property by expression

**Parameters:**
- `sourceMember` - Property selector expression

**Returns:** `ISimpleMap<TSource, TDestination>` (fluent)

**Example:**
```csharp
CreateMap<User, UserDto>()
    .IgnoreMember(x => x.Password);
```

---

### IgnoreMember(string memberName)

**Purpose:** Ignore property by name

**Parameters:**
- `memberName` - Property name (case-sensitive)

**Returns:** `ISimpleMap<TSource, TDestination>` (fluent)

**Example:**
```csharp
CreateMap<User, UserDto>()
    .IgnoreMember(nameof(User.Password));
```

---

### IgnoreMembers(params string[] memberNames)

**Purpose:** Ignore multiple properties

**Parameters:**
- `memberNames` - Array of property names

**Returns:** `ISimpleMap<TSource, TDestination>` (fluent)

**Example:**
```csharp
CreateMap<User, UserDto>()
    .IgnoreMembers(
        nameof(User.Password),
        nameof(User.PasswordHash)
    );
```

---

### ExecuteAfterMapping(Action<TSource, TDestination> action)

**Purpose:** Run custom logic after mapping

**Parameters:**
- `action` - Delegate receiving source and destination

**Returns:** `ISimpleMap<TSource, TDestination>` (fluent)

**Example:**
```csharp
CreateMap<User, UserDto>()
    .ExecuteAfterMapping((src, dest) =>
    {
        dest.FullName = $"{src.FirstName} {src.LastName}";
    });
```

---

## Assembly Scanning Rules

### Profile Discovery Requirements

**Must Have:**
- ✅ Inherit from `MappingProfile`
- ✅ Be a concrete (non-abstract) class
- ✅ Have a public parameterless constructor
- ✅ Be a public type

**Valid Profile:**
```csharp
public class UserProfile : MappingProfile
{
    public UserProfile() { }
}
```

**Invalid Profiles:**
```csharp
// ❌ Abstract
public abstract class BaseProfile : MappingProfile { }

// ❌ Has parameters
public class UserProfile : MappingProfile
{
    public UserProfile(ILogger logger) { }
}

// ❌ Internal
internal class UserProfile : MappingProfile { }
```

### Scanning Performance

- **Overhead:** < 1ms per assembly
- **Caching:** Profiles instantiated once at startup
- **Thread-Safety:** Registration is not thread-safe (called during startup only)

---

## Configuration Examples

### Minimal Configuration

```csharp
builder.Services.AddSimpleMapper(typeof(Program).Assembly);
```

### Multi-Assembly

```csharp
builder.Services.AddSimpleMapper(
    typeof(Program).Assembly,
    typeof(DataModels.User).Assembly,
    typeof(BusinessLogic.Service).Assembly
);
```

### Mixed Approach

```csharp
builder.Services.AddSimpleMapper(options =>
{
    // Scan main assembly
    options.AddProfiles(typeof(Program).Assembly);
    
    // Add explicit profile from external library
    options.AddProfiles(new ThirdPartyProfile());
    
    // Scan by marker types
    options.AddProfilesFromAssembliesContaining(
        typeof(DataModels.User)
    );
});
```

### Type-Safe Scanning

```csharp
builder.Services.AddSimpleMapperFromAssemblyContaining<Program>();
```

---

## Configuration Validation

### Runtime Validation

SimpleMapper validates profiles at first use:
- Profile must exist for source/destination pair
- Throws `ArgumentException` if mapping not found

### Debug Configuration

```csharp
builder.Services.AddSimpleMapper(options =>
{
    var assembly = typeof(Program).Assembly;
    var profiles = assembly.GetTypes()
        .Where(t => t.IsSubclassOf(typeof(MappingProfile)))
        .ToList();
        
    Console.WriteLine($"Found {profiles.Count} profiles");
    options.AddProfiles(assembly);
});
```

---

## Best Practices

### ✅ Recommended

1. **Use Assembly Scanning**
   ```csharp
   builder.Services.AddSimpleMapper(typeof(Program).Assembly);
   ```

2. **Register Early**
   ```csharp
   // Before other services that depend on it
   builder.Services.AddSimpleMapper(typeof(Program).Assembly);
   builder.Services.AddScoped<IUserService, UserService>();
   ```

3. **Keep Default Lifetime**
   ```csharp
   // Singleton is optimal
   builder.Services.AddSimpleMapper(typeof(Program).Assembly);
   ```

### ❌ Not Recommended

1. **Manual Registration**
   ```csharp
   // Maintenance burden
   builder.Services.AddSimpleMapper(
       new Profile1(), new Profile2(), ...
   );
   ```

2. **Changed Lifetime**
   ```csharp
   // Wasteful
   builder.Services.AddScoped<ISimpleMapper, SimpleMapper>();
   ```

---

## Next Steps

- **[API Reference](_reference_api.md)** - Method signatures
- **[How-to: Dependency Injection](_howto_dependency_injection.md)** - Usage examples
- **[Troubleshooting](_howto_troubleshooting.md)** - Fix issues
