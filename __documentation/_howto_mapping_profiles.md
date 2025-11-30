# How to Create Mapping Profiles

**Document Type:** How-to Guide (Problem-Oriented)  
**Time to Complete:** 10 minutes  
**Difficulty:** Beginner to Intermediate

## Problem

You need to configure how SimpleMapper maps properties between your source and destination types, including handling property ignoring, custom transformations, and after-mapping logic.

## Solution Overview

Mapping profiles define the mapping rules between types. This guide covers:
1. Creating basic profiles
2. Ignoring properties
3. After-mapping actions
4. Profile organization
5. Common patterns

## Prerequisites

- Completed [Getting Started Tutorial](_tutorial_getting_started.md)
- Understanding of [Dependency Injection](_howto_dependency_injection.md)

---

## Creating a Basic Profile

### Minimum Profile

The simplest profile declares a mapping between two types:

```csharp
using Fjeller.SimpleMapper;

public class UserMappingProfile : MappingProfile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>();
    }
}
```

**What happens:**
- Properties with matching names and types are automatically mapped
- No configuration needed for simple scenarios

### Bidirectional Mapping

Map in both directions:

```csharp
public class UserMappingProfile : MappingProfile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<UserDto, User>();  // Reverse mapping
    }
}
```

### Multiple Mappings in One Profile

Group related mappings together:

```csharp
public class UserMappingProfile : MappingProfile
{
    public UserMappingProfile()
    {
        // Entity to DTO
        CreateMap<User, UserDto>();
        
        // Create DTO to Entity
        CreateMap<CreateUserDto, User>();
        
        // Update DTO to Entity
        CreateMap<UpdateUserDto, User>();
    }
}
```

---

## Ignoring Properties

### Why Ignore Properties?

Common scenarios:
- **Security**: Exclude sensitive data (passwords, tokens)
- **Computed Properties**: Properties that don't exist in source
- **Read-Only**: Properties that shouldn't be updated
- **Different Structures**: Properties with no direct mapping

### Ignore by Expression

Type-safe property ignoring using lambda expressions:

```csharp
CreateMap<User, UserDto>()
    .IgnoreMember(x => x.Password)
    .IgnoreMember(x => x.PasswordHash);
```

**Advantages:**
- ? Compile-time checking
- ? Refactoring-safe (rename detection)
- ? IntelliSense support

### Ignore by Name

String-based property ignoring:

```csharp
CreateMap<User, UserDto>()
    .IgnoreMember(nameof(User.Password))
    .IgnoreMember(nameof(User.PasswordHash));
```

**Use when:**
- Working with dynamic scenarios
- Property not accessible via expression

### Ignore Multiple Properties

Ignore several properties at once:

```csharp
CreateMap<User, UserDto>()
    .IgnoreMembers(
        nameof(User.Password),
        nameof(User.PasswordHash),
        nameof(User.SecurityStamp),
        nameof(User.ConcurrencyStamp)
    );
```

### Real-World Example: Security

```csharp
public class UserMappingProfile : MappingProfile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>()
            .IgnoreMembers(
                nameof(User.PasswordHash),
                nameof(User.SecurityStamp),
                nameof(User.TwoFactorSecret)
            )
            .ExecuteAfterMapping((src, dest) =>
            {
                dest.FullName = $"{src.FirstName} {src.LastName}";
            });
    }
}
```

---

## After-Mapping Actions

### What Are After-Mapping Actions?

Code that runs **after** automatic property mapping, allowing custom transformations and computed properties.

### Basic After-Mapping

```csharp
CreateMap<User, UserDto>()
    .ExecuteAfterMapping((source, destination) =>
    {
        destination.FullName = $"{source.FirstName} {source.LastName}";
    });
```

### Common Use Cases

#### 1. Computed Properties

```csharp
CreateMap<Order, OrderDto>()
    .ExecuteAfterMapping((src, dest) =>
    {
        dest.TotalAmount = src.Items.Sum(i => i.Price * i.Quantity);
        dest.ItemCount = src.Items.Count;
    });
```

#### 2. Formatting

```csharp
CreateMap<User, UserDto>()
    .ExecuteAfterMapping((src, dest) =>
    {
        dest.DisplayName = $"{src.FirstName} {src.LastName} ({src.Email})";
        dest.FormattedCreatedDate = src.CreatedAt.ToString("yyyy-MM-dd");
    });
```

#### 3. Conditional Logic

```csharp
CreateMap<User, UserDto>()
    .ExecuteAfterMapping((src, dest) =>
    {
        dest.Status = src.IsActive
            ? "Active"
            : src.IsSuspended
                ? "Suspended"
                : "Inactive";
    });
```

#### 4. Timestamp Management

```csharp
CreateMap<CreateUserDto, User>()
    .ExecuteAfterMapping((src, dest) =>
    {
        dest.CreatedAt = DateTime.UtcNow;
        dest.UpdatedAt = DateTime.UtcNow;
        dest.IsActive = true;
    });

CreateMap<UpdateUserDto, User>()
    .IgnoreMember(u => u.Id)
    .IgnoreMember(u => u.CreatedAt)
    .ExecuteAfterMapping((src, dest) =>
    {
        dest.UpdatedAt = DateTime.UtcNow;
    });
```

#### 5. Complex Calculations

```csharp
CreateMap<Invoice, InvoiceDto>()
    .ExecuteAfterMapping((src, dest) =>
    {
        decimal subtotal = src.LineItems.Sum(i => i.Amount);
        decimal tax = subtotal * 0.10m;
        decimal discount = src.DiscountPercentage > 0
            ? subtotal * (src.DiscountPercentage / 100m)
            : 0m;
            
        dest.Subtotal = subtotal;
        dest.Tax = tax;
        dest.Discount = discount;
        dest.Total = subtotal + tax - discount;
    });
```

---

## Organizing Profiles

### One Profile Per Aggregate

**Recommended**: Create one profile per domain aggregate or feature.

```
/Features
    /Users
        User.cs
        UserDto.cs
        CreateUserDto.cs
        UpdateUserDto.cs
        UserMappingProfile.cs  ? All user mappings here
    /Products
        Product.cs
        ProductDto.cs
        ProductMappingProfile.cs  ? All product mappings here
    /Orders
        Order.cs
        OrderDto.cs
        OrderMappingProfile.cs  ? All order mappings here
```

**Benefits:**
- ? Easy to find related mappings
- ? Clear ownership
- ? Easier to maintain

### Multiple Profiles for Complex Aggregates

For large aggregates, split into multiple profiles:

```csharp
// UserReadMappingProfile.cs - Query/read operations
public class UserReadMappingProfile : MappingProfile
{
    public UserReadMappingProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<User, UserListItemDto>();
        CreateMap<User, UserDetailDto>();
    }
}

// UserWriteMappingProfile.cs - Command/write operations
public class UserWriteMappingProfile : MappingProfile
{
    public UserWriteMappingProfile()
    {
        CreateMap<CreateUserDto, User>();
        CreateMap<UpdateUserDto, User>();
    }
}
```

### Base Profile Pattern (Advanced)

Share common configuration across profiles:

```csharp
public abstract class BaseMappingProfile : MappingProfile
{
    protected void ConfigureAuditableMapping<TSource, TDest>()
        where TSource : IAuditable
        where TDest : class
    {
        CreateMap<TSource, TDest>()
            .ExecuteAfterMapping((src, dest) =>
            {
                // Common auditing logic
            });
    }
}

public class UserMappingProfile : BaseMappingProfile
{
    public UserMappingProfile()
    {
        ConfigureAuditableMapping<User, UserDto>();
    }
}
```

---

## Common Patterns

### Pattern 1: DTO to Entity (Create)

```csharp
CreateMap<CreateProductDto, Product>()
    .ExecuteAfterMapping((src, dest) =>
    {
        dest.Id = 0;  // Let database generate
        dest.CreatedAt = DateTime.UtcNow;
        dest.UpdatedAt = DateTime.UtcNow;
        dest.IsActive = true;
    });
```

### Pattern 2: DTO to Entity (Update)

```csharp
CreateMap<UpdateProductDto, Product>()
    .IgnoreMember(p => p.Id)           // Never update ID
    .IgnoreMember(p => p.CreatedAt)    // Never update creation date
    .ExecuteAfterMapping((src, dest) =>
    {
        dest.UpdatedAt = DateTime.UtcNow;
    });
```

### Pattern 3: Flattening

```csharp
// Source
public class Order
{
    public int Id { get; set; }
    public Customer Customer { get; set; }
    public List<OrderItem> Items { get; set; }
}

// Destination
public class OrderSummaryDto
{
    public int Id { get; set; }
    public string CustomerName { get; set; }  // Flattened
    public int ItemCount { get; set; }        // Computed
}

// Mapping
CreateMap<Order, OrderSummaryDto>()
    .ExecuteAfterMapping((src, dest) =>
    {
        dest.CustomerName = $"{src.Customer.FirstName} {src.Customer.LastName}";
        dest.ItemCount = src.Items.Count;
    });
```

### Pattern 4: Enum to String

```csharp
public enum UserRole { Admin, User, Guest }

CreateMap<User, UserDto>()
    .ExecuteAfterMapping((src, dest) =>
    {
        dest.RoleDisplay = src.Role.ToString();  // Enum to string
    });
```

### Pattern 5: Null Handling

```csharp
CreateMap<User, UserDto>()
    .ExecuteAfterMapping((src, dest) =>
    {
        dest.DisplayName = !string.IsNullOrWhiteSpace(src.Nickname)
            ? src.Nickname
            : $"{src.FirstName} {src.LastName}";
            
        dest.LastLoginDisplay = src.LastLoginAt?.ToString("yyyy-MM-dd HH:mm")
            ?? "Never logged in";
    });
```

---

## Best Practices

### ? DO

1. **Keep Profiles Focused**
   ```csharp
   public class UserMappingProfile : MappingProfile  // ? One aggregate
   ```

2. **Use After-Mapping for Computed Properties**
   ```csharp
   .ExecuteAfterMapping((src, dest) => dest.Total = src.Price * src.Quantity)
   ```

3. **Organize by Feature**
   ```
   /Features/Users/UserMappingProfile.cs
   /Features/Products/ProductMappingProfile.cs
   ```

4. **Use Expression-Based Ignoring**
   ```csharp
   .IgnoreMember(x => x.Password)  // ? Type-safe
   ```

### ? DON'T

1. **Don't Create God Profiles**
   ```csharp
   public class AllMappingsProfile : MappingProfile  // ? Too many mappings
   {
       public AllMappingsProfile()
       {
           // 100+ CreateMap calls...
       }
   }
   ```

2. **Don't Use Profiles for Business Logic**
   ```csharp
   .ExecuteAfterMapping((src, dest) =>
   {
       // ? Database calls, external APIs, heavy processing
       dest.Score = await CalculateScoreFromExternalApi(src.Id);
   })
   ```

3. **Don't Ignore Required Properties Silently**
   ```csharp
   .IgnoreMember(x => x.Email)  // ? If email is required, handle properly
   ```

---

## Complete Example

Here's a comprehensive profile demonstrating all concepts:

```csharp
public class OrderMappingProfile : MappingProfile
{
    public OrderMappingProfile()
    {
        // Read mappings
        CreateMap<Order, OrderDto>()
            .ExecuteAfterMapping((src, dest) =>
            {
                dest.CustomerName = $"{src.Customer.FirstName} {src.Customer.LastName}";
                dest.ItemCount = src.Items.Count;
                dest.TotalAmount = src.Items.Sum(i => i.Price * i.Quantity);
                dest.StatusDisplay = src.Status.ToString();
            });

        CreateMap<Order, OrderListItemDto>()
            .ExecuteAfterMapping((src, dest) =>
            {
                dest.CustomerName = src.Customer.CompanyName ?? 
                    $"{src.Customer.FirstName} {src.Customer.LastName}";
                dest.OrderSummary = $"Order #{src.OrderNumber} - {src.Items.Count} items";
            });

        // Write mappings
        CreateMap<CreateOrderDto, Order>()
            .IgnoreMember(o => o.Id)
            .IgnoreMember(o => o.Customer)  // Set separately
            .ExecuteAfterMapping((src, dest) =>
            {
                dest.OrderNumber = GenerateOrderNumber();
                dest.Status = OrderStatus.Pending;
                dest.CreatedAt = DateTime.UtcNow;
                dest.UpdatedAt = DateTime.UtcNow;
            });

        CreateMap<UpdateOrderDto, Order>()
            .IgnoreMembers(
                nameof(Order.Id),
                nameof(Order.OrderNumber),
                nameof(Order.CreatedAt),
                nameof(Order.CustomerId)
            )
            .ExecuteAfterMapping((src, dest) =>
            {
                dest.UpdatedAt = DateTime.UtcNow;
            });
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..20];
    }
}
```

---

## Troubleshooting

### Problem: Properties Not Mapping

**Symptoms:** Properties remain null or default after mapping

**Solutions:**
1. Verify property names match exactly (case-sensitive)
2. Verify property types match
3. Ensure properties are public
4. Check if property was accidentally ignored

### Problem: After-Mapping Not Executing

**Symptoms:** Computed properties not populated

**Solutions:**
1. Verify `ExecuteAfterMapping` is called on the correct mapping
2. Ensure profile is registered and discovered
3. Check for exceptions in after-mapping logic (silent failures)

---

## Next Steps

- **[How to Map Collections](_howto_collections.md)** - Handle nested objects and lists
- **[API Reference](_reference_api.md)** - Explore all profile methods
- **[Troubleshooting](_howto_troubleshooting.md)** - Fix common issues
