# How to Create Custom Property Mappings with ForMember

**Document Type:** How-to Guide (Problem-Oriented)  
**Time to Complete:** 15 minutes  
**Difficulty:** Intermediate

## Problem

You need to map properties between types where:
- Property names don't match
- Values need transformation or computation
- Source data comes from multiple properties
- Destination properties require custom logic

## Solution Overview

The `ForMember` method provides explicit control over how individual destination properties are populated from source data. This guide covers:
1. Basic property-to-property mapping with different names
2. Computed values and transformations
3. Combining multiple source properties
4. Working with ForMember alongside other mapping features

## Prerequisites

- Completed [Getting Started Tutorial](_tutorial_getting_started.md)
- Understanding of [Mapping Profiles](_howto_mapping_profiles.md)

---

## Understanding ForMember

### What Is ForMember?

`ForMember` lets you explicitly define how a destination property should be populated:

```csharp
CreateMap<Source, Destination>()
    .ForMember(dest => dest.TargetProperty, opt => opt.MapFrom(src => src.SourceProperty));
```

**Key Concepts:**
- **First parameter**: Selects the destination property to configure
- **Second parameter**: Configuration action where you call `MapFrom`
- **MapFrom**: Specifies the source expression to get the value from

### When to Use ForMember

**Use ForMember when:**
- ✅ Property names differ between source and destination
- ✅ Values need transformation or computation
- ✅ Combining multiple source properties
- ✅ Custom logic is required for specific properties

**Don't use ForMember when:**
- ❌ Properties have matching names and types (automatic mapping handles this)
- ❌ You just want to ignore a property (use `IgnoreMember` instead)

---

## Basic Property Mapping

### Map from Different Property Name

**Problem:** Source property has a different name than destination property.

```csharp
public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string DisplayName { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Name { get; set; }  // Maps from DisplayName
}

public class UserMappingProfile : MappingProfile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.DisplayName));
    }
}
```

**Usage:**
```csharp
User user = new User
{
    Id = 1,
    Username = "johndoe",
    DisplayName = "John Doe"
};

UserDto dto = mapper.Map<User, UserDto>(user);
// Result: dto.Name = "John Doe"
```

### Map Integer from Different Property

```csharp
public class Product
{
    public string SKU { get; set; }
    public int StockLevel { get; set; }
}

public class ProductDto
{
    public string SKU { get; set; }
    public int AvailableQuantity { get; set; }  // Maps from StockLevel
}

public class ProductMappingProfile : MappingProfile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.AvailableQuantity, opt => opt.MapFrom(src => src.StockLevel));
    }
}
```

---

## Computed Values and Transformations

### String Concatenation

**Problem:** Combine multiple properties into one.

```csharp
public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

public class PersonDto
{
    public string FullName { get; set; }
}

public class PersonMappingProfile : MappingProfile
{
    public PersonMappingProfile()
    {
        CreateMap<Person, PersonDto>()
            .ForMember(dest => dest.FullName, 
                opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
    }
}
```

**Usage:**
```csharp
Person person = new Person
{
    FirstName = "Jane",
    LastName = "Smith"
};

PersonDto dto = mapper.Map<Person, PersonDto>(person);
// Result: dto.FullName = "Jane Smith"
```

### Mathematical Operations

**Problem:** Calculate values during mapping.

```csharp
public class Product
{
    public decimal Price { get; set; }
    public decimal DiscountPercentage { get; set; }
}

public class ProductDisplayDto
{
    public decimal OriginalPrice { get; set; }
    public decimal FinalPrice { get; set; }
}

public class ProductMappingProfile : MappingProfile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, ProductDisplayDto>()
            .ForMember(dest => dest.OriginalPrice, 
                opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.FinalPrice, 
                opt => opt.MapFrom(src => src.Price * (1 - src.DiscountPercentage / 100m)));
    }
}
```

**Usage:**
```csharp
Product product = new Product
{
    Price = 100m,
    DiscountPercentage = 20m
};

ProductDisplayDto dto = mapper.Map<Product, ProductDisplayDto>(product);
// Result: dto.OriginalPrice = 100, dto.FinalPrice = 80
```

### Conditional Logic

**Problem:** Apply logic to determine the mapped value.

```csharp
public class User
{
    public bool IsActive { get; set; }
    public bool IsSuspended { get; set; }
    public bool IsDeleted { get; set; }
}

public class UserDto
{
    public string Status { get; set; }
}

public class UserMappingProfile : MappingProfile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                src.IsDeleted ? "Deleted" :
                src.IsSuspended ? "Suspended" :
                src.IsActive ? "Active" : "Inactive"));
    }
}
```

### String Formatting

**Problem:** Format values for display.

```csharp
public class Order
{
    public DateTime CreatedAt { get; set; }
    public decimal Amount { get; set; }
}

public class OrderListItemDto
{
    public string CreatedDate { get; set; }
    public string FormattedAmount { get; set; }
}

public class OrderMappingProfile : MappingProfile
{
    public OrderMappingProfile()
    {
        CreateMap<Order, OrderListItemDto>()
            .ForMember(dest => dest.CreatedDate, 
                opt => opt.MapFrom(src => src.CreatedAt.ToString("yyyy-MM-dd")))
            .ForMember(dest => dest.FormattedAmount, 
                opt => opt.MapFrom(src => $"${src.Amount:N2}"));
    }
}
```

---

## Advanced Scenarios

### Multiple ForMember Calls

**Problem:** Configure multiple custom mappings for different properties.

```csharp
public class Employee
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public DateTime HireDate { get; set; }
}

public class EmployeeDto
{
    public int EmployeeId { get; set; }
    public string FullName { get; set; }
    public string ContactEmail { get; set; }
    public string Tenure { get; set; }
}

public class EmployeeMappingProfile : MappingProfile
{
    public EmployeeMappingProfile()
    {
        CreateMap<Employee, EmployeeDto>()
            .ForMember(dest => dest.EmployeeId, 
                opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.FullName, 
                opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.ContactEmail, 
                opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Tenure, 
                opt => opt.MapFrom(src => $"{(DateTime.UtcNow - src.HireDate).Days / 365} years"));
    }
}
```

### Nested Property Access

**Problem:** Access nested properties from complex objects.

```csharp
public class Order
{
    public int Id { get; set; }
    public Customer Customer { get; set; }
    public Address ShippingAddress { get; set; }
}

public class Customer
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

public class Address
{
    public string City { get; set; }
    public string Country { get; set; }
}

public class OrderSummaryDto
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; }
    public string ShippingLocation { get; set; }
}

public class OrderMappingProfile : MappingProfile
{
    public OrderMappingProfile()
    {
        CreateMap<Order, OrderSummaryDto>()
            .ForMember(dest => dest.OrderId, 
                opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CustomerName, 
                opt => opt.MapFrom(src => $"{src.Customer.FirstName} {src.Customer.LastName}"))
            .ForMember(dest => dest.ShippingLocation, 
                opt => opt.MapFrom(src => $"{src.ShippingAddress.City}, {src.ShippingAddress.Country}"));
    }
}
```

### Null Handling

**Problem:** Handle potentially null values safely.

```csharp
public class User
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Nickname { get; set; }  // May be null
    public DateTime? LastLoginAt { get; set; }  // May be null
}

public class UserDto
{
    public string DisplayName { get; set; }
    public string LastLoginDisplay { get; set; }
}

public class UserMappingProfile : MappingProfile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.DisplayName, 
                opt => opt.MapFrom(src => 
                    !string.IsNullOrWhiteSpace(src.Nickname) 
                        ? src.Nickname 
                        : $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.LastLoginDisplay, 
                opt => opt.MapFrom(src => 
                    src.LastLoginAt.HasValue 
                        ? src.LastLoginAt.Value.ToString("yyyy-MM-dd") 
                        : "Never"));
    }
}
```

---

## Combining with Other Features

### ForMember with IgnoreMember

**Problem:** Use custom mapping for some properties and ignore others.

```csharp
public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public int AccessLevel { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string ContactEmail { get; set; }  // Custom mapping
    public string Role { get; set; }  // Custom mapping
}

public class UserMappingProfile : MappingProfile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.ContactEmail, 
                opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Role, 
                opt => opt.MapFrom(src => 
                    src.AccessLevel >= 100 ? "Admin" : "User"))
            .IgnoreMember(x => x.Password);  // Security
    }
}
```

**Key Points:**
- ✅ `ForMember` handles custom mappings
- ✅ `IgnoreMember` excludes sensitive data
- ✅ Remaining properties map automatically

### ForMember with ExecuteAfterMapping

**Problem:** Use custom property mapping and post-mapping logic together.

```csharp
public class CreateOrderDto
{
    public string CustomerEmail { get; set; }
    public List<OrderItemDto> Items { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public string CustomerContact { get; set; }
    public List<OrderItem> Items { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public string OrderNumber { get; set; }
}

public class OrderMappingProfile : MappingProfile
{
    public OrderMappingProfile()
    {
        CreateMap<CreateOrderDto, Order>()
            .ForMember(dest => dest.CustomerContact, 
                opt => opt.MapFrom(src => src.CustomerEmail))
            .ExecuteAfterMapping((src, dest) =>
            {
                // Computed after initial mapping
                dest.Total = dest.Items.Sum(i => i.Price * i.Quantity);
                dest.CreatedAt = DateTime.UtcNow;
                dest.OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..20];
            });
    }
}
```

**Execution Order:**
1. Automatic property mapping (matching names/types)
2. Custom property mapping via `ForMember`
3. After-mapping action via `ExecuteAfterMapping`

### Complete Real-World Example

```csharp
public class Employee
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public decimal Salary { get; set; }
    public string Department { get; set; }
    public DateTime HireDate { get; set; }
    public bool IsActive { get; set; }
    public string SocialSecurityNumber { get; set; }
}

public class EmployeeDto
{
    public int EmployeeId { get; set; }
    public string FullName { get; set; }
    public string ContactEmail { get; set; }
    public string DepartmentName { get; set; }
    public string EmploymentDuration { get; set; }
    public string Status { get; set; }
    public bool IsEligibleForBonus { get; set; }
}

public class EmployeeMappingProfile : MappingProfile
{
    public EmployeeMappingProfile()
    {
        CreateMap<Employee, EmployeeDto>()
            // Custom property mappings
            .ForMember(dest => dest.EmployeeId, 
                opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.FullName, 
                opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.ContactEmail, 
                opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.DepartmentName, 
                opt => opt.MapFrom(src => src.Department))
            .ForMember(dest => dest.EmploymentDuration, 
                opt => opt.MapFrom(src => 
                    $"{(DateTime.UtcNow - src.HireDate).Days / 365} years, {(DateTime.UtcNow - src.HireDate).Days % 365} days"))
            .ForMember(dest => dest.Status, 
                opt => opt.MapFrom(src => src.IsActive ? "Active" : "Inactive"))
            // Ignore sensitive data
            .IgnoreMember(x => x.SocialSecurityNumber)
            .IgnoreMember(x => x.Salary)
            // Post-mapping computation
            .ExecuteAfterMapping((src, dest) =>
            {
                // Complex business logic
                int yearsEmployed = (DateTime.UtcNow - src.HireDate).Days / 365;
                dest.IsEligibleForBonus = src.IsActive && yearsEmployed >= 1;
            });
    }
}
```

---

## Usage Examples

### Map Collections

`ForMember` works seamlessly with collection mapping:

```csharp
public class UserMappingProfile : MappingProfile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.DisplayName, 
                opt => opt.MapFrom(src => src.Nickname ?? src.Username));
    }
}

// Usage
List<User> users = GetUsers();
IEnumerable<UserDto> dtos = mapper.Map<User, UserDto>(users);
// ForMember applies to each element
```

### Runtime Type Detection

`ForMember` works with dynamic source types:

```csharp
object source = GetUserFromCache();  // Runtime type
UserDto dto = mapper.Map<UserDto>(source);
// ForMember mappings are applied
```

### Existing Destination Object

`ForMember` populates existing objects:

```csharp
User user = GetUser();
UserDto existingDto = GetCachedDto();

mapper.Map(user, existingDto);
// ForMember updates the existing object
```

---

## Best Practices

### ✅ DO

1. **Use ForMember for Explicit Mappings**
   ```csharp
   .ForMember(dest => dest.FullName, 
       opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
   ```

2. **Keep Expressions Simple**
   ```csharp
   // ✅ Good - simple transformation
   .ForMember(dest => dest.Total, 
       opt => opt.MapFrom(src => src.Price * src.Quantity))
   ```

3. **Use ForMember with Other Features**
   ```csharp
   CreateMap<Source, Destination>()
       .ForMember(dest => dest.Custom, opt => opt.MapFrom(src => src.Other))
       .IgnoreMember(x => x.Sensitive)
       .ExecuteAfterMapping((src, dest) => dest.Timestamp = DateTime.UtcNow);
   ```

4. **Handle Null Values Explicitly**
   ```csharp
   .ForMember(dest => dest.Name, 
       opt => opt.MapFrom(src => src.Nickname ?? src.Username ?? "Unknown"))
   ```

### ❌ DON'T

1. **Don't Use ForMember for Automatic Mappings**
   ```csharp
   // ❌ Unnecessary - properties match automatically
   .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
   ```

2. **Don't Put Heavy Logic in ForMember**
   ```csharp
   // ❌ Bad - database calls, API calls
   .ForMember(dest => dest.Score, 
       opt => opt.MapFrom(src => CalculateScoreFromDatabase(src.Id)))
   
   // ✅ Good - use ExecuteAfterMapping for complex logic
   .ExecuteAfterMapping((src, dest) => 
   {
       dest.Score = CalculateScore(src);
   })
   ```

3. **Don't Configure Same Property Twice**
   ```csharp
   // ❌ Throws InvalidOperationException
   .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FirstName))
   .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.LastName))
   ```

4. **Don't Forget to Call MapFrom**
   ```csharp
   // ❌ Throws InvalidOperationException
   .ForMember(dest => dest.Name, opt => { })
   
   // ✅ Correct
   .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.DisplayName))
   ```

---

## Common Patterns

### Pattern 1: Rename Properties

```csharp
CreateMap<Source, Destination>()
    .ForMember(dest => dest.NewName, opt => opt.MapFrom(src => src.OldName));
```

### Pattern 2: Flatten Object Graph

```csharp
CreateMap<Order, OrderDto>()
    .ForMember(dest => dest.CustomerName, 
        opt => opt.MapFrom(src => $"{src.Customer.FirstName} {src.Customer.LastName}"))
    .ForMember(dest => dest.ShippingCity, 
        opt => opt.MapFrom(src => src.ShippingAddress.City));
```

### Pattern 3: Transform Values

```csharp
CreateMap<User, UserDto>()
    .ForMember(dest => dest.RoleDisplay, 
        opt => opt.MapFrom(src => src.Role.ToString().ToUpper()));
```

### Pattern 4: Combine Multiple Properties

```csharp
CreateMap<Address, AddressDto>()
    .ForMember(dest => dest.FullAddress, 
        opt => opt.MapFrom(src => $"{src.Street}, {src.City}, {src.State} {src.ZipCode}"));
```

### Pattern 5: Conditional Mapping

```csharp
CreateMap<Product, ProductDto>()
    .ForMember(dest => dest.DisplayPrice, 
        opt => opt.MapFrom(src => 
            src.IsOnSale 
                ? $"${src.SalePrice:N2} (Save ${src.Price - src.SalePrice:N2}!)" 
                : $"${src.Price:N2}"));
```

---

## Troubleshooting

### Problem: InvalidOperationException - Property Already Configured

**Symptoms:**
```
InvalidOperationException: A custom mapping for destination property 'Name' has already been configured.
```

**Cause:** Called `ForMember` twice for the same destination property.

**Solution:**
```csharp
// ❌ Wrong
.ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FirstName))
.ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.LastName))

// ✅ Correct - choose one
.ForMember(dest => dest.Name, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
```

### Problem: InvalidOperationException - MapFrom Not Called

**Symptoms:**
```
InvalidOperationException: MapFrom must be called within the options action for destination property 'Name'.
```

**Cause:** Empty options action without calling `MapFrom`.

**Solution:**
```csharp
// ❌ Wrong
.ForMember(dest => dest.Name, opt => { })

// ✅ Correct
.ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.DisplayName))
```

### Problem: Property Not Mapping

**Symptoms:** Custom mapped property remains null or default value.

**Possible Causes:**
1. Source expression throws exception (silently caught)
2. Source property is null
3. Destination property is read-only

**Solutions:**
```csharp
// Add null handling
.ForMember(dest => dest.Name, 
    opt => opt.MapFrom(src => src.DisplayName ?? "Unknown"))

// Ensure destination property has setter
public string Name { get; set; }  // Not { get; }
```

### Problem: Performance Issues

**Symptoms:** Mapping is slow with complex ForMember expressions.

**Cause:** Complex computations or nested property access in ForMember.

**Solution:** Move heavy logic to `ExecuteAfterMapping`:

```csharp
// ❌ Slower - complex logic in ForMember
.ForMember(dest => dest.Score, 
    opt => opt.MapFrom(src => ComplexCalculation(src)))

// ✅ Faster - simple mapping, complex logic after
.ForMember(dest => dest.RawData, opt => opt.MapFrom(src => src.Data))
.ExecuteAfterMapping((src, dest) =>
{
    dest.Score = ComplexCalculation(dest.RawData);
})
```

---

## Next Steps

- **[How to Create Mapping Profiles](_howto_mapping_profiles.md)** - Master all profile features
- **[API Reference](_reference_api.md)** - Explore complete API
- **[Collections Mapping](_howto_collections.md)** - Work with complex structures

---

## Related Documentation

- [Mapping Profiles Guide](_howto_mapping_profiles.md) - Profile basics
- [After-Mapping Actions](_howto_mapping_profiles.md#after-mapping-actions) - Post-mapping logic
- [Property Ignoring](_howto_mapping_profiles.md#ignoring-properties) - Exclude properties
