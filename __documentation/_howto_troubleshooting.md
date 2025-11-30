# Troubleshooting Guide

**Document Type:** How-to Guide (Problem-Oriented)  
**Time to Complete:** 5-15 minutes (depends on issue)  
**Difficulty:** All Levels

## Common Problems and Solutions

This guide addresses the most common issues encountered when using SimpleMapper.

---

## "No mapping available" Error

### Symptoms

```
ArgumentException: There is no mapping available between the types 
MyApp.Models.User and MyApp.DTOs.UserDto
```

### Root Causes

1. **Profile Not Created**
2. **Profile Not Registered**
3. **Type Names Don't Match**
4. **Wrong Assembly Scanned**

### Solutions

#### Solution 1: Create the Mapping Profile

Ensure you have a profile with `CreateMap<TSource, TDest>()`:

```csharp
public class UserMappingProfile : MappingProfile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>();  // ? Must exist
    }
}
```

#### Solution 2: Verify Assembly Scanning

```csharp
// Ensure profile's assembly is scanned
builder.Services.AddSimpleMapper(typeof(UserMappingProfile).Assembly);
```

#### Solution 3: Debug Profile Discovery

Add logging to see discovered profiles:

```csharp
builder.Services.AddSimpleMapper(options =>
{
    var assembly = typeof(Program).Assembly;
    var profiles = assembly.GetTypes()
        .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(MappingProfile)))
        .ToList();
        
    Console.WriteLine($"Found {profiles.Count} profiles:");
    foreach (var profile in profiles)
    {
        Console.WriteLine($"  - {profile.FullName}");
    }
    
    options.AddProfiles(assembly);
});
```

#### Solution 4: Check Type Match

Verify source and destination types match exactly:

```csharp
// ? Wrong - Different namespaces
CreateMap<MyApp.Entities.User, UserDto>();  // Maps Entities.User
_mapper.Map<MyApp.Models.User, UserDto>(user);  // Tries Models.User

// ? Correct - Same types
CreateMap<MyApp.Models.User, UserDto>();
_mapper.Map<MyApp.Models.User, UserDto>(user);
```

---

## Profiles Not Discovered

### Symptoms

- Profile exists but not found during assembly scanning
- "No mapping available" error despite having profile

### Root Causes

1. **No Parameterless Constructor**
2. **Abstract or Internal Class**
3. **Wrong Base Class**
4. **Wrong Assembly**

### Solutions

#### Verify Profile Requirements

Your profile must meet ALL these requirements:

```csharp
// ? Valid profile
public class UserMappingProfile : MappingProfile  // Public, inherits MappingProfile
{
    public UserMappingProfile()  // Parameterless constructor
    {
        CreateMap<User, UserDto>();
    }
}

// ? Won't be discovered - Abstract
public abstract class BaseMappingProfile : MappingProfile { }

// ? Won't be discovered - Has parameters
public class ProductProfile : MappingProfile
{
    public ProductProfile(ILogger logger) { }
}

// ? Won't be discovered - Internal
internal class OrderProfile : MappingProfile { }
```

#### Check Assembly Reference

```csharp
// Scan the assembly containing your profiles
builder.Services.AddSimpleMapper(typeof(UserMappingProfile).Assembly);
```

---

## Cannot Resolve ISimpleMapper

### Symptoms

```
InvalidOperationException: Unable to resolve service for type 
'Fjeller.SimpleMapper.ISimpleMapper' while attempting to activate...
```

### Root Causes

1. **AddSimpleMapper Not Called**
2. **Called After Build()**
3. **Wrong Service Provider**

### Solutions

#### Verify Registration Order

```csharp
var builder = WebApplication.CreateBuilder(args);

// ? Register BEFORE Build()
builder.Services.AddSimpleMapper(typeof(Program).Assembly);

var app = builder.Build();  // Build comes after registration
```

#### Check ServiceCollection

```csharp
// Verify registration
builder.Services.AddSimpleMapper(typeof(Program).Assembly);

// Debug: Check if registered
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var mapper = scope.ServiceProvider.GetService<ISimpleMapper>();
    if (mapper is null)
    {
        throw new Exception("Mapper not registered!");
    }
}
```

---

## Properties Not Mapping

### Symptoms

- Properties remain null after mapping
- Default values instead of source values

### Root Causes

1. **Property Names Don't Match**
2. **Property Types Don't Match**
3. **Property Not Public**
4. **Property Ignored**

### Solutions

#### Check Property Names (Case-Sensitive)

```csharp
// Source
public class User
{
    public string FirstName { get; set; }  // Note: FirstName
}

// Destination
public class UserDto
{
    public string Firstname { get; set; }  // ? Won't map - different case
    public string FirstName { get; set; }  // ? Will map - exact match
}
```

#### Check Property Types

```csharp
// Source
public class User
{
    public int Age { get; set; }  // int
}

// Destination
public class UserDto
{
    public string Age { get; set; }  // ? Won't map - type mismatch
    public int Age { get; set; }     // ? Will map - type match
}
```

#### Verify Property Accessibility

```csharp
public class User
{
    public string Name { get; set; }      // ? Public - will map
    private string Secret { get; set; }   // ? Private - won't map
    internal string Internal { get; set; } // ? Internal - won't map
}
```

#### Check for Ignored Members

```csharp
CreateMap<User, UserDto>()
    .IgnoreMember(x => x.Email);  // Email won't be mapped
```

---

## Collection Not Mapping

### Symptoms

- Collection property is null
- Collection is empty when source has items

### Root Causes

1. **No Profile for Element Type**
2. **Collection Property Null in Source**
3. **Type Mismatch**

### Solutions

#### Create Profile for Element Type

```csharp
// If mapping Order with OrderItems collection
CreateMap<Order, OrderDto>();
CreateMap<OrderItem, OrderItemDto>();  // ? Required for collection
```

#### Check Source Collection

```csharp
Order order = GetOrder();
if (order.Items is null)  // ? Source collection is null
{
    order.Items = new List<OrderItem>();  // Initialize
}
```

#### Verify Collection Types

```csharp
// Source
public List<OrderItem> Items { get; set; }

// Destination - Must be compatible
public List<OrderItemDto> Items { get; set; }  // ? Works
public OrderItemDto[] Items { get; set; }      // ? Works (converted)
public IEnumerable<OrderItemDto> Items { get; set; }  // ? Works
```

---

## After-Mapping Not Executing

### Symptoms

- Computed properties not populated
- Custom logic not running

### Root Causes

1. **Exception in After-Mapping (Silent)**
2. **Wrong Mapping Direction**
3. **Profile Not Applied**

### Solutions

#### Add Error Handling

```csharp
CreateMap<User, UserDto>()
    .ExecuteAfterMapping((src, dest) =>
    {
        try
        {
            dest.FullName = $"{src.FirstName} {src.LastName}";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"After-mapping error: {ex.Message}");
            throw;
        }
    });
```

#### Verify Mapping Direction

```csharp
// Define mapping for User -> UserDto
CreateMap<User, UserDto>()
    .ExecuteAfterMapping((src, dest) =>
    {
        dest.FullName = $"{src.FirstName} {src.LastName}";
    });

// ? Won't execute - mapping UserDto -> User (reverse)
_mapper.Map<UserDto, User>(dto);

// ? Executes - correct direction
_mapper.Map<User, UserDto>(user);
```

---

## Performance Issues

### Symptoms

- Slow mapping operations
- High memory usage
- Application sluggishness

### Solutions

#### First-Time Compilation

**Expected**: First mapping of each type pair takes 5-10ms (expression compilation)

```csharp
// First call: ~5-10ms (compiles expression tree)
var dto1 = _mapper.Map<User, UserDto>(user1);

// Subsequent calls: ~80-90ns (uses compiled expression)
var dto2 = _mapper.Map<User, UserDto>(user2);
```

**This is normal and unavoidable** - expression trees must be compiled once.

#### Large Collection Mapping

```csharp
// ? Bad - Map entire collection at once
List<User> millionUsers = GetMillionUsers();
var dtos = _mapper.Map<User, UserDto>(millionUsers).ToList();  // Memory issue

// ? Good - Use pagination
int pageSize = 100;
for (int i = 0; i < millionUsers.Count; i += pageSize)
{
    var page = millionUsers.Skip(i).Take(pageSize);
    var dtos = _mapper.Map<User, UserDto>(page).ToList();
    ProcessPage(dtos);
}
```

#### Verify Singleton Registration

```csharp
// ? Correct - Singleton (default)
builder.Services.AddSimpleMapper(typeof(Program).Assembly);

// ? Wrong - Creates new instance per request
builder.Services.AddScoped<ISimpleMapper, SimpleMapper>();
```

---

## Build Errors

### Error: "Type or namespace 'SimpleMapper' could not be found"

**Solution:** Install package
```bash
dotnet add package Fjeller.SimpleMapper
```

### Error: "Cannot resolve IServiceCollection"

**Solution:** Add using
```csharp
using Fjeller.SimpleMapper.DependencyInjection;
```

---

## Runtime Errors

### Error: "Destination type must have parameterless constructor"

**Solution:** Ensure destination has parameterless constructor

```csharp
// ? Won't work
public class UserDto
{
    public UserDto(string name) { Name = name; }
}

// ? Works
public class UserDto
{
    public UserDto() { }  // Parameterless constructor
    public string Name { get; set; }
}
```

---

## Debugging Tips

### Enable Detailed Logging

```csharp
builder.Services.AddSimpleMapper(options =>
{
    Console.WriteLine("=== SimpleMapper Configuration ===");
    
    var assembly = typeof(Program).Assembly;
    var profiles = assembly.GetTypes()
        .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(MappingProfile)))
        .ToList();
        
    Console.WriteLine($"Discovered {profiles.Count} profiles:");
    foreach (var profile in profiles)
    {
        Console.WriteLine($"  ? {profile.Name}");
        
        // Instantiate to see mappings
        var instance = Activator.CreateInstance(profile) as MappingProfile;
        // Profile constructor runs, creating maps
    }
    
    options.AddProfiles(assembly);
    Console.WriteLine("=== Configuration Complete ===");
});
```

### Test Mapping in Isolation

```csharp
[Fact]
public void TestUserMapping()
{
    // Arrange
    var profile = new UserMappingProfile();
    var mapper = new SimpleMapper();
    
    var user = new User
    {
        Id = 1,
        FirstName = "John",
        LastName = "Doe"
    };
    
    // Act
    var dto = mapper.Map<User, UserDto>(user);
    
    // Assert
    Assert.Equal(user.Id, dto.Id);
    Assert.Equal("John Doe", dto.FullName);
}
```

---

## Getting Help

If you're still stuck:

1. **Check Documentation**
   - [Getting Started Tutorial](_tutorial_getting_started.md)
   - [API Reference](_reference_api.md)

2. **Search Issues**
   - [GitHub Issues](https://github.com/fjeller/fjeller.simplemapper/issues)

3. **Ask for Help**
   - [GitHub Discussions](https://github.com/fjeller/fjeller.simplemapper/discussions)

4. **Report Bug**
   - Include: .NET version, SimpleMapper version, minimal reproduction code

---

## Quick Checklist

Before asking for help, verify:

- [ ] Profile exists with `CreateMap<TSource, TDest>()`
- [ ] Profile has parameterless public constructor
- [ ] `AddSimpleMapper()` called before `app.Build()`
- [ ] Correct assembly scanned
- [ ] Property names and types match exactly
- [ ] Destination type has parameterless constructor
- [ ] No exceptions in after-mapping logic
- [ ] Latest version of SimpleMapper installed

---

## Next Steps

- **[API Reference](_reference_api.md)** - Lookup specific methods
- **[Architecture](_explanation_architecture.md)** - Understand how it works
- **[Performance](_explanation_performance.md)** - Optimize your mappings
