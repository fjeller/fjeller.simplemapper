# SimpleMapper - Dependency Injection Integration Guide

**Version**: 1.0  
**Date**: December 2024  
**Target Framework**: .NET 10

---

## Overview

SimpleMapper provides seamless integration with ASP.NET Core's dependency injection container, making it easy to use in Web APIs, Blazor Server, Blazor WebAssembly, and other .NET applications.

---

## Installation

The DI integration is built into the `Fjeller.SimpleMapper` package. No additional packages are required.

**Package Dependency**: `Microsoft.Extensions.DependencyInjection.Abstractions` (automatically included)

---

## Quick Start

### ASP.NET Core Web API

```csharp
using Fjeller.SimpleMapper.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Option 1: Scan assembly for mapping profiles
builder.Services.AddSimpleMapper(typeof(Program).Assembly);

// Option 2: Register explicit profiles
builder.Services.AddSimpleMapper(
    new UserMappingProfile(),
    new ProductMappingProfile()
);

// Option 3: Use configuration options
builder.Services.AddSimpleMapper(options =>
{
    options.AddProfiles(typeof(Program).Assembly);
});

builder.Services.AddControllers();

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

await builder.Build().RunAsync();
```

---

## Registration Methods

### 1. AddSimpleMapper with Explicit Profiles

Register SimpleMapper with specific mapping profiles:

```csharp
builder.Services.AddSimpleMapper(
    new UserMappingProfile(),
    new ProductMappingProfile(),
    new OrderMappingProfile()
);
```

**Use when**: You want explicit control over which profiles are registered.

---

### 2. AddSimpleMapper with Assembly Scanning

Automatically discover all `MappingProfile` subclasses in one or more assemblies:

```csharp
// Single assembly
builder.Services.AddSimpleMapper(typeof(Program).Assembly);

// Multiple assemblies
builder.Services.AddSimpleMapper(
    typeof(Program).Assembly,
    typeof(Data.User).Assembly,
    typeof(Services.ProductService).Assembly
);
```

**Use when**: You want automatic profile discovery (recommended for most scenarios).

**Requirements for profile discovery**:
- Must inherit from `MappingProfile`
- Must be a non-abstract class
- Must have a parameterless constructor
- Must be public

---

### 3. AddSimpleMapper with Configuration Options

Use the fluent configuration API:

```csharp
builder.Services.AddSimpleMapper(options =>
{
    // Scan assemblies
    options.AddProfiles(typeof(Program).Assembly);
    
    // Add explicit profiles
    options.AddProfiles(new CustomMappingProfile());
    
    // Scan assemblies containing specific types
    options.AddProfilesFromAssembliesContaining(
        typeof(User),
        typeof(Product)
    );
    
    // Scan assembly containing a specific type
    options.AddProfilesFromAssemblyContaining<Program>();
});
```

**Use when**: You need fine-grained control over profile registration.

---

### 4. AddSimpleMapperFromAssemblyContaining\<T>

Scan the assembly containing a specific type:

```csharp
builder.Services.AddSimpleMapperFromAssemblyContaining<Program>();
```

**Use when**: You want a clean, type-safe way to reference an assembly.

---

### 5. AddSimpleMapperFromAssemblyContaining (multiple types)

Scan assemblies containing multiple marker types:

```csharp
builder.Services.AddSimpleMapperFromAssemblyContaining(
    typeof(Program),
    typeof(Data.User),
    typeof(Services.ProductService)
);
```

**Use when**: You need to scan multiple assemblies with type-safe references.

---

### 6. AddSimpleMapperFromCallingAssembly

Automatically scan the calling assembly:

```csharp
builder.Services.AddSimpleMapperFromCallingAssembly();
```

**Use when**: You want the simplest registration possible (suitable for single-assembly applications).

---

## Usage in Application Code

### ASP.NET Core Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
	private readonly ISimpleMapper _mapper;
	private readonly IUserRepository _userRepository;

	public UsersController(ISimpleMapper mapper, IUserRepository userRepository)
	{
		_mapper = mapper;
		_userRepository = userRepository;
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<UserDto>> GetUser(int id)
	{
		User? user = await _userRepository.GetByIdAsync(id);
		
		if (user is null)
		{
			return NotFound();
		}

		UserDto dto = _mapper.Map<User, UserDto>(user);
		
		return Ok(dto);
	}

	[HttpPost]
	public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createDto)
	{
		User user = _mapper.Map<CreateUserDto, User>(createDto);
		await _userRepository.AddAsync(user);

		UserDto dto = _mapper.Map<User, UserDto>(user);
		
		return CreatedAtAction(nameof(GetUser), new { id = user.Id }, dto);
	}

	[HttpGet]
	public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
	{
		IEnumerable<User> users = await _userRepository.GetAllAsync();
		IEnumerable<UserDto> dtos = _mapper.Map<User, UserDto>(users);
		
		return Ok(dtos);
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> UpdateUser(int id, UpdateUserDto updateDto)
	{
		User? existingUser = await _userRepository.GetByIdAsync(id);
		
		if (existingUser is null)
		{
			return NotFound();
		}

		_mapper.Map(updateDto, existingUser);
		await _userRepository.UpdateAsync(existingUser);

		return NoContent();
	}
}
```

---

### Blazor Component

```razor
@page "/users"
@inject ISimpleMapper Mapper
@inject IUserService UserService

<h3>User List</h3>

@if (_users is null)
{
	<p>Loading...</p>
}
else
{
	<table class="table">
		<thead>
			<tr>
				<th>ID</th>
				<th>Name</th>
				<th>Email</th>
			</tr>
		</thead>
		<tbody>
			@foreach (var user in _users)
			{
				<tr>
					<td>@user.Id</td>
					<td>@user.Name</td>
					<td>@user.Email</td>
				</tr>
			}
		</tbody>
	</table>
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

---

### Razor Page

```csharp
using Fjeller.SimpleMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyApp.Pages;

public class UserDetailsModel : PageModel
{
	private readonly ISimpleMapper _mapper;
	private readonly IUserRepository _userRepository;

	public UserDetailsModel(ISimpleMapper mapper, IUserRepository userRepository)
	{
		_mapper = mapper;
		_userRepository = userRepository;
	}

	[BindProperty]
	public UserDto User { get; set; } = null!;

	public async Task<IActionResult> OnGetAsync(int id)
	{
		var entity = await _userRepository.GetByIdAsync(id);
		
		if (entity is null)
		{
			return NotFound();
		}

		User = _mapper.Map<User, UserDto>(entity);
		
		return Page();
	}

	public async Task<IActionResult> OnPostAsync(int id)
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var entity = await _userRepository.GetByIdAsync(id);
		
		if (entity is null)
		{
			return NotFound();
		}

		_mapper.Map(User, entity);
		await _userRepository.UpdateAsync(entity);

		return RedirectToPage("./Index");
	}
}
```

---

### Minimal API

```csharp
using Fjeller.SimpleMapper;
using Fjeller.SimpleMapper.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSimpleMapper(typeof(Program).Assembly);
builder.Services.AddScoped<IUserRepository, UserRepository>();

var app = builder.Build();

app.MapGet("/users/{id}", async (int id, ISimpleMapper mapper, IUserRepository repo) =>
{
	var user = await repo.GetByIdAsync(id);
	
	return user is not null
		? Results.Ok(mapper.Map<User, UserDto>(user))
		: Results.NotFound();
});

app.MapPost("/users", async (CreateUserDto dto, ISimpleMapper mapper, IUserRepository repo) =>
{
	var user = mapper.Map<CreateUserDto, User>(dto);
	await repo.AddAsync(user);
	
	var responseDto = mapper.Map<User, UserDto>(user);
	
	return Results.Created($"/users/{user.Id}", responseDto);
});

app.MapGet("/users", async (ISimpleMapper mapper, IUserRepository repo) =>
{
	var users = await repo.GetAllAsync();
	var dtos = mapper.Map<User, UserDto>(users);
	
	return Results.Ok(dtos);
});

app.Run();
```

---

## Service Lifetime

SimpleMapper is registered as a **Singleton** by default:

```csharp
services.TryAddSingleton<ISimpleMapper>(...)
```

### Why Singleton?

1. ? **Thread-Safe**: All compiled mappers use thread-safe `ConcurrentDictionary`
2. ? **Performance**: Compiled expression trees are reused across all requests
3. ? **Memory Efficient**: Single instance serves the entire application
4. ? **Stateless**: No per-request or per-scope state

### Benefits

- **Fast**: No allocation overhead per request
- **Consistent**: Same instance across all requests
- **Scalable**: Supports high-concurrency scenarios

### Important Notes

?? **Do not change the lifetime to Scoped or Transient** unless you have a very specific reason. Using a different lifetime will:
- Waste memory (new instance per scope/request)
- Waste CPU (recompiling expression trees)
- Provide no benefit (mapper is stateless)

---

## Assembly Scanning

SimpleMapper automatically discovers and registers all `MappingProfile` subclasses when you use assembly scanning methods.

### Profile Discovery Requirements

For a profile to be automatically discovered:

1. ? Must inherit from `MappingProfile`
2. ? Must be a `class` (not `abstract` or `interface`)
3. ? Must have a public parameterless constructor
4. ? Must be a public type

### Example: Valid Profiles

```csharp
// ? This will be discovered
public class UserMappingProfile : MappingProfile
{
	public UserMappingProfile()
	{
		CreateMap<User, UserDto>();
		CreateMap<UserDto, User>();
	}
}

// ? This will also be discovered
public class ProductMappingProfile : MappingProfile
{
	public ProductMappingProfile()
	{
		CreateMap<Product, ProductDto>()
			.IgnoreMember(x => x.InternalId);
	}
}
```

### Example: Invalid Profiles

```csharp
// ? NOT discovered: abstract class
public abstract class BaseMappingProfile : MappingProfile
{
}

// ? NOT discovered: no parameterless constructor
public class OrderMappingProfile : MappingProfile
{
	public OrderMappingProfile(ILogger logger)
	{
		CreateMap<Order, OrderDto>();
	}
}

// ? NOT discovered: internal class
internal class InternalMappingProfile : MappingProfile
{
	public InternalMappingProfile()
	{
		CreateMap<Internal, InternalDto>();
	}
}
```

### Scanning Multiple Assemblies

You can scan multiple assemblies in a single call:

```csharp
builder.Services.AddSimpleMapper(
	typeof(Program).Assembly,           // Web/API layer
	typeof(User).Assembly,              // Domain layer
	typeof(UserRepository).Assembly     // Data access layer
);
```

---

## Best Practices

### 1. Register Early in Startup

Register SimpleMapper before other services that depend on it:

```csharp
var builder = WebApplication.CreateBuilder(args);

// ? Register SimpleMapper first
builder.Services.AddSimpleMapper(typeof(Program).Assembly);

// Then register dependent services
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
```

---

### 2. Prefer Assembly Scanning

Use assembly scanning instead of explicit profile registration:

```csharp
// ? Good: Automatic discovery
builder.Services.AddSimpleMapper(typeof(Program).Assembly);

// ?? Less maintainable: Manual registration
builder.Services.AddSimpleMapper(
	new UserMappingProfile(),
	new ProductMappingProfile()
	// Easy to forget to add new profiles
);
```

---

### 3. Organize Profiles by Feature

Organize your code by feature, keeping mapping profiles close to related code:

```
/Features
	/Users
		- UsersController.cs
		- UserMappingProfile.cs
		- UserDto.cs
		- User.cs
	/Products
		- ProductsController.cs
		- ProductMappingProfile.cs
		- ProductDto.cs
		- Product.cs
```

---

### 4. Always Inject ISimpleMapper Interface

Depend on the interface, not the concrete implementation:

```csharp
// ? Good: Depend on interface
public class UsersController : ControllerBase
{
	private readonly ISimpleMapper _mapper;
	
	public UsersController(ISimpleMapper mapper)
	{
		_mapper = mapper;
	}
}

// ? Bad: Depend on concrete type
public class UsersController : ControllerBase
{
	private readonly SimpleMapper _mapper;  // Breaks abstraction
	
	public UsersController(SimpleMapper mapper)
	{
		_mapper = mapper;
	}
}
```

---

### 5. Create One Profile Per Aggregate

Create separate profiles for each domain aggregate or feature:

```csharp
// ? Good: Focused profiles
public class UserMappingProfile : MappingProfile
{
	public UserMappingProfile()
	{
		CreateMap<User, UserDto>();
		CreateMap<UserDto, User>();
		CreateMap<CreateUserDto, User>();
	}
}

public class ProductMappingProfile : MappingProfile
{
	public ProductMappingProfile()
	{
		CreateMap<Product, ProductDto>();
		CreateMap<ProductDto, Product>();
	}
}

// ? Bad: God object profile
public class AllMappingsProfile : MappingProfile
{
	public AllMappingsProfile()
	{
		// Maps for everything in the application
		CreateMap<User, UserDto>();
		CreateMap<Product, ProductDto>();
		CreateMap<Order, OrderDto>();
		// ... 100 more mappings
	}
}
```

---

## Troubleshooting

### Problem: Profiles Not Being Discovered

**Symptoms**: Assembly scanning doesn't find your profiles

**Solutions**:

1. **Verify Profile Requirements**
```csharp
// Check your profile meets all requirements:
// - Inherits from MappingProfile
// - Is public
// - Is not abstract
// - Has parameterless constructor
public class UserMappingProfile : MappingProfile  // ? Correct
{
	public UserMappingProfile()  // ? Parameterless constructor
	{
		CreateMap<User, UserDto>();
	}
}
```

2. **Verify Correct Assembly**
```csharp
// Make sure you're scanning the right assembly
builder.Services.AddSimpleMapper(typeof(UserMappingProfile).Assembly);
```

3. **Debug Profile Discovery**
```csharp
builder.Services.AddSimpleMapper(options =>
{
	var assembly = typeof(Program).Assembly;
	var profiles = assembly.GetTypes()
		.Where(t => t.IsClass 
			&& !t.IsAbstract 
			&& t.IsSubclassOf(typeof(MappingProfile))
			&& t.GetConstructor(Type.EmptyTypes) is not null)
		.ToList();
		
	Console.WriteLine($"Found {profiles.Count} profiles:");
	foreach (var profile in profiles)
	{
		Console.WriteLine($"  - {profile.Name}");
	}
	
	options.AddProfiles(assembly);
});
```

---

### Problem: "No mapping available" Exception

**Symptoms**: Runtime exception when trying to map

**Solutions**:

1. **Verify CreateMap Was Called**
```csharp
public class UserMappingProfile : MappingProfile
{
	public UserMappingProfile()
	{
		// ? Make sure you call CreateMap
		CreateMap<User, UserDto>();
	}
}
```

2. **Check Type Names Match Exactly**
```csharp
// ? Wrong: Namespace mismatch
CreateMap<Data.User, Models.UserDto>();  // Maps Data.User -> Models.UserDto

// ? Correct: Use actual types
mapper.Map<Data.User, Models.UserDto>(user);  // Must match exactly
```

3. **Ensure Profile Is Registered**
```csharp
// Add logging to verify registration
builder.Services.AddSimpleMapper(options =>
{
	Console.WriteLine("Registering profiles...");
	options.AddProfiles(typeof(Program).Assembly);
	Console.WriteLine($"Registered {options.Profiles.Count} profiles");
});
```

---

### Problem: Cannot Resolve ISimpleMapper

**Symptoms**: Dependency injection fails to resolve `ISimpleMapper`

**Solutions**:

1. **Verify Registration**
```csharp
// Make sure AddSimpleMapper is called
builder.Services.AddSimpleMapper(typeof(Program).Assembly);
```

2. **Check Registration Order**
```csharp
var builder = WebApplication.CreateBuilder(args);

// ? Register before building the app
builder.Services.AddSimpleMapper(typeof(Program).Assembly);

var app = builder.Build();  // Must be after registration
```

3. **Verify Service Provider**
```csharp
// In Program.cs, test resolution
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
	var mapper = scope.ServiceProvider.GetRequiredService<ISimpleMapper>();
	Console.WriteLine($"Mapper resolved: {mapper is not null}");
}
```

---

## Complete Example: ASP.NET Core Web API

Here's a complete, production-ready example:

### Program.cs

```csharp
using Fjeller.SimpleMapper.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add database context
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add SimpleMapper with assembly scanning
builder.Services.AddSimpleMapper(options =>
{
	options.AddProfiles(typeof(Program).Assembly);
});

// Add repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
```

### UserMappingProfile.cs

```csharp
using Fjeller.SimpleMapper;
using MyApp.Models;
using MyApp.DTOs;

namespace MyApp.Mappings;

public class UserMappingProfile : MappingProfile
{
	public UserMappingProfile()
	{
		CreateMap<User, UserDto>();
		
		CreateMap<CreateUserDto, User>()
			.ExecuteAfterMapping((src, dest) =>
			{
				dest.CreatedAt = DateTime.UtcNow;
				dest.IsActive = true;
			});
		
		CreateMap<UpdateUserDto, User>()
			.IgnoreMember(u => u.Id)
			.IgnoreMember(u => u.CreatedAt)
			.ExecuteAfterMapping((src, dest) =>
			{
				dest.UpdatedAt = DateTime.UtcNow;
			});
	}
}
```

### UsersController.cs

```csharp
using Fjeller.SimpleMapper;
using Microsoft.AspNetCore.Mvc;
using MyApp.Data;
using MyApp.DTOs;
using MyApp.Models;

namespace MyApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
	private readonly ISimpleMapper _mapper;
	private readonly IUserRepository _userRepository;
	private readonly ILogger<UsersController> _logger;

	public UsersController(
		ISimpleMapper mapper,
		IUserRepository userRepository,
		ILogger<UsersController> logger)
	{
		_mapper = mapper;
		_userRepository = userRepository;
		_logger = logger;
	}

	[HttpGet]
	public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
	{
		_logger.LogInformation("Getting all users");
		
		var users = await _userRepository.GetAllAsync();
		var dtos = _mapper.Map<User, UserDto>(users);
		
		return Ok(dtos);
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<UserDto>> GetById(int id)
	{
		_logger.LogInformation("Getting user {UserId}", id);
		
		var user = await _userRepository.GetByIdAsync(id);
		
		if (user is null)
		{
			return NotFound();
		}

		var dto = _mapper.Map<User, UserDto>(user);
		
		return Ok(dto);
	}

	[HttpPost]
	public async Task<ActionResult<UserDto>> Create(CreateUserDto createDto)
	{
		_logger.LogInformation("Creating new user");
		
		var user = _mapper.Map<CreateUserDto, User>(createDto);
		await _userRepository.AddAsync(user);
		await _userRepository.SaveChangesAsync();

		var dto = _mapper.Map<User, UserDto>(user);
		
		return CreatedAtAction(nameof(GetById), new { id = user.Id }, dto);
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> Update(int id, UpdateUserDto updateDto)
	{
		_logger.LogInformation("Updating user {UserId}", id);
		
		var user = await _userRepository.GetByIdAsync(id);
		
		if (user is null)
		{
			return NotFound();
		}

		_mapper.Map(updateDto, user);
		await _userRepository.UpdateAsync(user);
		await _userRepository.SaveChangesAsync();

		return NoContent();
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> Delete(int id)
	{
		_logger.LogInformation("Deleting user {UserId}", id);
		
		var user = await _userRepository.GetByIdAsync(id);
		
		if (user is null)
		{
			return NotFound();
		}

		await _userRepository.DeleteAsync(user);
		await _userRepository.SaveChangesAsync();

		return NoContent();
	}
}
```

---

## Summary

SimpleMapper's dependency injection integration provides:

? **Seamless ASP.NET Core Integration** - Works with Web API, Blazor, and Minimal APIs  
? **Multiple Registration Methods** - Explicit profiles, assembly scanning, or configuration  
? **Automatic Profile Discovery** - Scans assemblies for `MappingProfile` subclasses  
? **Singleton Lifetime** - Thread-safe, performant, memory-efficient  
? **Clean API** - Fluent configuration with method chaining  
? **Production-Ready** - Follows ASP.NET Core best practices

---

**Document Version**: 1.0  
**Last Updated**: December 2024  
**Compatibility**: .NET 10, ASP.NET Core 10, Blazor Server, Blazor WebAssembly
