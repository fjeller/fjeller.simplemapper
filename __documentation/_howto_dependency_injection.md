# How to Configure Dependency Injection

**Document Type:** How-to Guide (Problem-Oriented)  
**Time to Complete:** 10 minutes  
**Difficulty:** Beginner to Intermediate

## Problem

You need to register SimpleMapper in your ASP.NET Core application's dependency injection container and configure it for your specific scenario.

## Solution Overview

SimpleMapper offers multiple registration methods to fit different architectural needs:
1. **Assembly Scanning** (Recommended) - Automatic profile discovery
2. **Explicit Profile Registration** - Manual control
3. **Configuration Options** - Advanced scenarios

## Prerequisites

- Completed the [Getting Started Tutorial](_tutorial_getting_started.md)
- An ASP.NET Core project (.NET 10)
- Basic understanding of dependency injection

---

## Method 1: Assembly Scanning (Recommended)

### When to Use
- ✅ Most common scenario
- ✅ You have multiple profiles spread across your codebase
- ✅ You want automatic discovery
- ✅ You follow conventional structure

### How It Works
SimpleMapper scans the assembly for all classes that inherit from `MappingProfile` and registers them automatically.

### Basic Registration

```csharp
using Fjeller.SimpleMapper.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Scan the current assembly for mapping profiles
builder.Services.AddSimpleMapper(typeof(Program).Assembly);

var app = builder.Build();
app.Run();
```

### Multiple Assemblies

If your profiles are spread across different assemblies:

```csharp
builder.Services.AddSimpleMapper(
    typeof(Program).Assembly,              // Web/API layer
    typeof(DataModels.User).Assembly,      // Data layer
    typeof(BusinessLogic.Service).Assembly // Business layer
);
```

### Type-Safe Assembly Reference

Use generic marker types for cleaner code:

```csharp
builder.Services.AddSimpleMapperFromAssemblyContaining<Program>();
```

Or multiple assemblies:

```csharp
builder.Services.AddSimpleMapperFromAssemblyContaining(
    typeof(Program),
    typeof(DataModels.User),
    typeof(BusinessLogic.Service)
);
```

### Profile Discovery Requirements

For a profile to be automatically discovered, it must:
- ✅ Inherit from `MappingProfile`
- ✅ Be a non-abstract class
- ✅ Have a public parameterless constructor
- ✅ Be a public type

**Example of valid profile:**
```csharp
public class UserMappingProfile : MappingProfile
{
    public UserMappingProfile() // ✅ Parameterless constructor
    {
        CreateMap<User, UserDto>();
    }
}
```

**Examples that WON'T be discovered:**
```csharp
// ❌ Abstract class
public abstract class BaseMappingProfile : MappingProfile { }

// ❌ No parameterless constructor
public class OrderProfile : MappingProfile
{
    public OrderProfile(ILogger logger) { }
}

// ❌ Internal class
internal class InternalProfile : MappingProfile { }
```

---

## Method 2: Explicit Profile Registration

### When to Use
- ✅ You have a small number of profiles
- ✅ You want explicit control over what's registered
- ✅ You need to pass dependencies to profile constructors (not typical)

### Basic Registration

```csharp
builder.Services.AddSimpleMapper(
    new UserMappingProfile(),
    new ProductMappingProfile(),
    new OrderMappingProfile()
);
```

### Pros and Cons

**Pros:**
- Explicit visibility of registered profiles
- No "magic" assembly scanning
- Easy to understand for small projects

**Cons:**
- Must manually update when adding new profiles
- Easy to forget to register new profiles
- More verbose

---

## Method 3: Configuration Options

### When to Use
- ✅ You need fine-grained control
- ✅ You want to mix assembly scanning and explicit registration
- ✅ You need advanced configuration

### Advanced Configuration

```csharp
builder.Services.AddSimpleMapper(options =>
{
    // Scan assemblies
    options.AddProfiles(typeof(Program).Assembly);
    
    // Add explicit profiles
    options.AddProfiles(new CustomMappingProfile());
    
    // Scan assemblies by marker types
    options.AddProfilesFromAssembliesContaining(
        typeof(User),
        typeof(Product)
    );
    
    // Generic type-safe scanning
    options.AddProfilesFromAssemblyContaining<Program>();
});
```

### Available Configuration Methods

```csharp
public class SimpleMapperOptions
{
    // Add explicit profiles
    SimpleMapperOptions AddProfiles(params MappingProfile[] profiles);
    
    // Scan assemblies
    SimpleMapperOptions AddProfiles(params Assembly[] assemblies);
    
    // Scan by marker types
    SimpleMapperOptions AddProfilesFromAssembliesContaining(params Type[] markerTypes);
    
    // Generic scanning
    SimpleMapperOptions AddProfilesFromAssemblyContaining<TMarker>();
}
```

---

## Application-Specific Configurations

### ASP.NET Core Web API

```csharp
using Fjeller.SimpleMapper.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add SimpleMapper
builder.Services.AddSimpleMapper(typeof(Program).Assembly);

// Add other services
builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

var app = builder.Build();

app.MapControllers();
app.Run();
```

### Blazor Server

```csharp
using Fjeller.SimpleMapper.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add SimpleMapper
builder.Services.AddSimpleMapper(typeof(Program).Assembly);

var app = builder.Build();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.Run();
```

### Blazor WebAssembly

```csharp
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Fjeller.SimpleMapper.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

// Add SimpleMapper
builder.Services.AddSimpleMapper(typeof(Program).Assembly);

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

await builder.Build().RunAsync();
```

### Minimal APIs

```csharp
using Fjeller.SimpleMapper;
using Fjeller.SimpleMapper.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add SimpleMapper
builder.Services.AddSimpleMapper(typeof(Program).Assembly);
builder.Services.AddScoped<IUserRepository, UserRepository>();

var app = builder.Build();

// Use mapper in endpoint
app.MapGet("/users/{id}", async (
    int id,
    ISimpleMapper mapper,
    IUserRepository repo) =>
{
    var user = await repo.GetByIdAsync(id);
    return user is not null
        ? Results.Ok(mapper.Map<User, UserDto>(user))
        : Results.NotFound();
});

app.Run();
```

---

## Service Lifetime

SimpleMapper is registered as a **Singleton** by default.

### Why Singleton?

1. **Thread-Safe** ✅
   - All internal caches use `ConcurrentDictionary`
   - Compiled expression trees are immutable after creation
   - No mutable shared state

2. **Performance** ✅
   - Compiled mappers reused across all requests
   - No allocation overhead per request
   - Expression trees compiled once, used many times

3. **Memory Efficient** ✅
   - Single instance for entire application
   - Compiled mappers shared across requests
   - No duplicate compilation

4. **Stateless Design** ✅
   - No per-request state
   - No per-scope dependencies
   - Pure mapping operations

### Service Registration

```csharp
// SimpleMapper internally does this:
services.TryAddSingleton<ISimpleMapper>(serviceProvider =>
{
    // Initialize mapper with profiles
    return new SimpleMapper();
});
```

### ⚠️ Do Not Change Lifetime

**DON'T** do this:
```csharp
// ❌ BAD - Wastes resources
services.AddScoped<ISimpleMapper, SimpleMapper>();
services.AddTransient<ISimpleMapper, SimpleMapper>();
```

**Why it's bad:**
- Wastes memory (new instance per scope/request)
- Wastes CPU (recompiles expression trees)
- Provides no benefit (mapper is stateless)

---

## Injecting and Using the Mapper

### Constructor Injection (Controllers)

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ISimpleMapper _mapper;
    private readonly IUserRepository _repository;
    
    public UsersController(
        ISimpleMapper mapper,
        IUserRepository repository)
    {
        _mapper = mapper;
        _repository = repository;
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user is null) return NotFound();
        
        return Ok(_mapper.Map<User, UserDto>(user));
    }
}
```

### Constructor Injection (Services)

```csharp
public class UserService : IUserService
{
    private readonly ISimpleMapper _mapper;
    private readonly IUserRepository _repository;
    
    public UserService(
        ISimpleMapper mapper,
        IUserRepository repository)
    {
        _mapper = mapper;
        _repository = repository;
    }
    
    public async Task<UserDto> GetUserByIdAsync(int id)
    {
        var user = await _repository.GetByIdAsync(id);
        return _mapper.Map<User, UserDto>(user);
    }
}
```

### Blazor Component Injection

```razor
@page "/users"
@inject ISimpleMapper Mapper
@inject IUserService UserService

<h3>Users</h3>

@if (_users is null)
{
    <p>Loading...</p>
}
else
{
    @foreach (var user in _users)
    {
        <div>@user.FullName - @user.Email</div>
    }
}

@code {
    private List<UserDto>? _users;
    
    protected override async Task OnInitializedAsync()
    {
        var entities = await UserService.GetAllAsync();
        _users = Mapper.Map<User, UserDto>(entities).ToList();
    }
}
```

### Minimal API Parameter Injection

```csharp
app.MapPost("/users", async (
    CreateUserDto dto,
    ISimpleMapper mapper,
    IUserRepository repo) =>
{
    var user = mapper.Map<CreateUserDto, User>(dto);
    await repo.AddAsync(user);
    
    var responseDto = mapper.Map<User, UserDto>(user);
    
    return Results.Created($"/users/{user.Id}", responseDto);
});
```

---

## Best Practices

### ✅ DO

1. **Use Assembly Scanning**
   ```csharp
   builder.Services.AddSimpleMapper(typeof(Program).Assembly);
   ```

2. **Register Early in Startup**
   ```csharp
   // Register before services that depend on it
   builder.Services.AddSimpleMapper(typeof(Program).Assembly);
   builder.Services.AddScoped<IUserService, UserService>();
   ```

3. **Inject ISimpleMapper Interface**
   ```csharp
   public class UsersController(ISimpleMapper mapper) // ✅ Good
   ```

4. **Keep Profiles Focused**
   ```csharp
   public class UserMappingProfile : MappingProfile // ✅ One aggregate
   ```

### ❌ DON'T

1. **Don't Depend on Concrete Type**
   ```csharp
   public class UsersController(SimpleMapper mapper) // ❌ Bad
   ```

2. **Don't Change Service Lifetime**
   ```csharp
   services.AddScoped<ISimpleMapper, SimpleMapper>(); // ❌ Wasteful
   ```

3. **Don't Create God Profiles**
   ```csharp
   // ❌ Bad - 100+ mappings in one profile
   public class AllMappingsProfile : MappingProfile
   ```

4. **Don't Manually Instantiate**
   ```csharp
   var mapper = new SimpleMapper(); // ❌ Defeats DI purpose
   ```

---

## Troubleshooting

### Problem: "Cannot resolve ISimpleMapper"

**Symptoms:** DI fails to inject ISimpleMapper

**Solutions:**
1. Verify `AddSimpleMapper()` is called in `Program.cs`
2. Ensure it's called before `app.Build()`
3. Check spelling and namespace imports

```csharp
// ✅ Correct order
builder.Services.AddSimpleMapper(typeof(Program).Assembly);
var app = builder.Build();
```

### Problem: Profiles Not Discovered

**Symptoms:** "No mapping available" exception at runtime

**Solutions:**
1. Verify profile has parameterless constructor
2. Ensure profile is public, non-abstract
3. Check profile inherits from `MappingProfile`
4. Confirm correct assembly is being scanned

**Debug:**
```csharp
builder.Services.AddSimpleMapper(options =>
{
    var assembly = typeof(Program).Assembly;
    var profiles = assembly.GetTypes()
        .Where(t => t.IsClass 
            && !t.IsAbstract 
            && t.IsSubclassOf(typeof(MappingProfile)))
        .ToList();
        
    Console.WriteLine($"Found {profiles.Count} profiles:");
    foreach (var profile in profiles)
    {
        Console.WriteLine($"  - {profile.Name}");
    }
    
    options.AddProfiles(assembly);
});
```

### Problem: Duplicate Registration

**Symptoms:** Multiple mappers registered

**Solution:** SimpleMapper uses `TryAddSingleton`, so calling `AddSimpleMapper` multiple times is safe - only the first registration is kept.

---

## Summary

| Registration Method | Use Case | Code |
|-------------------|----------|------|
| **Assembly Scanning** | Most scenarios | `AddSimpleMapper(typeof(Program).Assembly)` |
| **Multiple Assemblies** | Multi-layer apps | `AddSimpleMapper(asm1, asm2, asm3)` |
| **Type-Safe Scanning** | Cleaner code | `AddSimpleMapperFromAssemblyContaining<T>()` |
| **Explicit Profiles** | Small projects | `AddSimpleMapper(new Profile1(), new Profile2())` |
| **Configuration** | Advanced needs | `AddSimpleMapper(options => { ... })` |

---

## Next Steps

- **[How to Create Mapping Profiles](_howto_mapping_profiles.md)** - Configure mappings
- **[API Reference](_reference_api.md)** - Explore all methods
- **[Troubleshooting Guide](_howto_troubleshooting.md)** - Fix common issues
