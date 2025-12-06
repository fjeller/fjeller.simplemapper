# Getting Started with SimpleMapper

**Document Type:** Tutorial (Learning-Oriented)  
**Time to Complete:** 15 minutes  
**Difficulty:** Beginner

## What You'll Learn

By the end of this tutorial, you will:
- Install SimpleMapper in a .NET project
- Create your first mapping profile
- Map objects between types
- Use SimpleMapper in an ASP.NET Core application

## What You'll Build

A simple ASP.NET Core Web API that maps between database entities and DTOs (Data Transfer Objects) using SimpleMapper.

## Prerequisites

Before starting, ensure you have:
- ✅ .NET 9 SDK or later (compatible with .NET 9 and .NET 10)
- ✅ Visual Studio 2022, VS Code, or Rider
- ✅ Basic knowledge of C# and ASP.NET Core
- ✅ Familiarity with REST APIs (helpful but not required)

## Step 1: Create a New Project

Open your terminal and create a new ASP.NET Core Web API project:

```bash
dotnet new webapi -n SimpleMapperDemo
cd SimpleMapperDemo
```

This creates a new Web API project with sample weather forecast code.

## Step 2: Install SimpleMapper

Add the SimpleMapper package to your project:

```bash
dotnet add package Fjeller.SimpleMapper
```

Verify the installation by opening `SimpleMapperDemo.csproj`. You should see:

```xml
<PackageReference Include="Fjeller.SimpleMapper" Version="*" />
```

## Step 3: Define Your Models

Let's create a realistic scenario: a User entity from a database and a UserDto for API responses.

Create a new folder called `Models` and add two files:

**Models/User.cs** (Entity - what's in the database):
```csharp
namespace SimpleMapperDemo.Models;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    
    // Sensitive data - should not be exposed in API
    public string PasswordHash { get; set; } = string.Empty;
    public string SecurityStamp { get; set; } = string.Empty;
}
```

**Models/UserDto.cs** (DTO - what the API returns):
```csharp
namespace SimpleMapperDemo.Models;

public class UserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}
```

**Notice**: The DTO has a `FullName` property (not in User) and excludes sensitive fields like `PasswordHash`.

## Step 4: Create a Mapping Profile

Mapping profiles tell SimpleMapper how to map between types. Create a new folder called `Mappings` and add a file:

**Mappings/UserMappingProfile.cs**:
```csharp
using Fjeller.SimpleMapper;
using SimpleMapperDemo.Models;

namespace SimpleMapperDemo.Mappings;

public class UserMappingProfile : MappingProfile
{
    public UserMappingProfile()
    {
        // Map User to UserDto
        CreateMap<User, UserDto>()
            .IgnoreMember(nameof(User.PasswordHash))      // Don't map sensitive data
            .IgnoreMember(nameof(User.SecurityStamp))
            .ExecuteAfterMapping((source, dest) =>
            {
                // Compute FullName after mapping
                dest.FullName = $"{source.FirstName} {source.LastName}";
            });
    }
}
```

**What's happening here?**
- `CreateMap<User, UserDto>()` - Declares a mapping from User to UserDto
- `.IgnoreMember()` - Excludes sensitive properties from mapping
- `.ExecuteAfterMapping()` - Runs custom logic after property mapping (computing FullName)

## Step 5: Register SimpleMapper in Dependency Injection

Open `Program.cs` and add SimpleMapper to the DI container:

```csharp
using Fjeller.SimpleMapper.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register SimpleMapper - it will find all MappingProfile classes automatically
builder.Services.AddSimpleMapper(typeof(Program).Assembly);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

**What's happening?**
- `AddSimpleMapper(typeof(Program).Assembly)` scans your assembly for all classes that inherit from `MappingProfile` and registers them automatically
- SimpleMapper is registered as a **singleton** (thread-safe, optimal for performance)

## Step 6: Create a Controller

Create a new controller to demonstrate the mapping:

**Controllers/UsersController.cs**:
```csharp
using Fjeller.SimpleMapper;
using Microsoft.AspNetCore.Mvc;
using SimpleMapperDemo.Models;

namespace SimpleMapperDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
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
        // Simulate database fetch
        User user = GetUserFromDatabase(id);
        
        if (user is null)
        {
            return NotFound();
        }

        // Map entity to DTO using SimpleMapper
        UserDto dto = _mapper.Map<User, UserDto>(user);
        
        return Ok(dto);
    }

    [HttpGet]
    public ActionResult<IEnumerable<UserDto>> GetAllUsers()
    {
        // Simulate database fetch
        List<User> users = GetUsersFromDatabase();
        
        // Map collection of entities to DTOs
        IEnumerable<UserDto> dtos = _mapper.Map<User, UserDto>(users);
        
        return Ok(dtos);
    }

    // Simulate database operations
    private User? GetUserFromDatabase(int id)
    {
        var users = GetUsersFromDatabase();
        return users.FirstOrDefault(u => u.Id == id);
    }

    private List<User> GetUsersFromDatabase()
    {
        return new List<User>
        {
            new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                LastLoginAt = DateTime.UtcNow.AddHours(-2),
                PasswordHash = "hashed_password_here",
                SecurityStamp = "security_stamp_here"
            },
            new User
            {
                Id = 2,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                LastLoginAt = DateTime.UtcNow.AddMinutes(-30),
                PasswordHash = "hashed_password_here",
                SecurityStamp = "security_stamp_here"
            }
        };
    }
}
```

**What's happening?**
- `ISimpleMapper` is injected via constructor injection
- `_mapper.Map<User, UserDto>(user)` - Maps a single object
- `_mapper.Map<User, UserDto>(users)` - Maps a collection
- Sensitive data (password, security stamp) is automatically excluded based on our profile

## Step 7: Run and Test Your Application

Run your application:

```bash
dotnet run
```

Open your browser and navigate to:
```
https://localhost:5001/swagger
```

### Test the GET endpoint

Click on `GET /api/Users/{id}` and try it with `id = 1`.

**Response:**
```json
{
  "id": 1,
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "fullName": "John Doe",
  "createdAt": "2024-11-01T10:30:00Z",
  "lastLoginAt": "2024-12-01T08:30:00Z"
}
```

**Notice:**
- ✅ Properties are automatically mapped (Id, FirstName, LastName, Email)
- ✅ `FullName` is computed via `ExecuteAfterMapping`
- ✅ Sensitive data (`PasswordHash`, `SecurityStamp`) is **not present**

### Test the collection endpoint

Click on `GET /api/Users` to see all users mapped.

## What You've Learned

Congratulations! You've successfully:

✅ **Installed** SimpleMapper in a .NET project  
✅ **Created** a mapping profile with property ignoring and custom logic  
✅ **Registered** SimpleMapper with dependency injection  
✅ **Injected** and used `ISimpleMapper` in a controller  
✅ **Mapped** both single objects and collections  
✅ **Excluded** sensitive data from API responses  

## Key Concepts Recap

| Concept | What It Does |
|---------|-------------|
| `MappingProfile` | Defines how types map to each other |
| `CreateMap<TSource, TDest>()` | Declares a mapping between two types |
| `.IgnoreMember()` | Excludes properties from mapping |
| `.ExecuteAfterMapping()` | Runs custom logic after property mapping |
| `AddSimpleMapper()` | Registers mapper in DI with assembly scanning |
| `ISimpleMapper` | Interface to inject and use the mapper |
| `Map<TSource, TDest>()` | Performs the actual mapping |

## Next Steps

Now that you understand the basics, explore more advanced scenarios:

- **[How to Configure Dependency Injection](_howto_dependency_injection.md)** - Learn all DI registration options
- **[How to Create Mapping Profiles](_howto_mapping_profiles.md)** - Master profile configuration
- **[How to Map Collections](_howto_collections.md)** - Handle complex nested structures
- **[API Reference](_reference_api.md)** - Explore all available methods

## Common Questions

**Q: Do I need to create a profile for every mapping?**  
A: Yes, SimpleMapper requires explicit profile registration. This gives you control over how objects are mapped.

**Q: Can SimpleMapper map properties with different names?**  
A: Not automatically. SimpleMapper maps by matching property names and types. For different names, use `ExecuteAfterMapping` to manually assign values.

**Q: What if I forget to register a profile?**  
A: You'll get a clear exception: `"There is no mapping available between the types..."`. Simply create and register the profile.

**Q: Can I map to an existing object?**  
A: Yes! Use `_mapper.Map(source, existingDestination)` to update an existing object instead of creating a new one.

## Troubleshooting

**Problem:** "Cannot resolve ISimpleMapper"  
**Solution:** Ensure `AddSimpleMapper()` is called in `Program.cs` before `app.Build()`

**Problem:** "No mapping available between types"  
**Solution:** Verify you've created a `MappingProfile` with `CreateMap<TSource, TDest>()` and the assembly is being scanned

**Problem:** Properties are null after mapping  
**Solution:** Check property names and types match exactly between source and destination

---

**🎉 Congratulations!** You're now ready to use SimpleMapper in your projects. For production applications, explore the how-to guides for advanced scenarios.
