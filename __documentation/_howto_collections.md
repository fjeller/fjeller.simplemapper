# How to Map Collections and Nested Objects

**Document Type:** How-to Guide (Problem-Oriented)  
**Time to Complete:** 10 minutes  
**Difficulty:** Intermediate

## Problem

You need to map collections (lists, arrays) and nested objects between types while maintaining proper structure and relationships.

## Solution Overview

SimpleMapper automatically handles:
- Collections (`List<T>`, `T[]`, `IEnumerable<T>`)
- Nested objects (properties that are complex types)
- Deep mapping (nested collections of complex objects)

## Prerequisites

- Completed [Getting Started Tutorial](_tutorial_getting_started.md)
- Understanding of [Mapping Profiles](_howto_mapping_profiles.md)

---

## Simple Collections

### List to List

```csharp
// Models
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; }
}

// Profile
public class UserMappingProfile : MappingProfile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>();
    }
}

// Usage
List<User> users = GetUsersFromDatabase();
IEnumerable<UserDto> dtos = _mapper.Map<User, UserDto>(users);
List<UserDto> dtoList = dtos.ToList();
```

### Array to Array

```csharp
User[] users = GetUsersArray();
IEnumerable<UserDto> dtos = _mapper.Map<User, UserDto>(users);
UserDto[] dtoArray = dtos.ToArray();
```

### IEnumerable Conversions

```csharp
IEnumerable<User> users = GetUsers();
IEnumerable<UserDto> dtos = _mapper.Map<User, UserDto>(users);

// Convert to specific collection type
List<UserDto> list = dtos.ToList();
UserDto[] array = dtos.ToArray();
HashSet<UserDto> set = dtos.ToHashSet();
```

---

## Nested Objects

### Simple Nesting

```csharp
// Models
public class Order
{
    public int Id { get; set; }
    public Customer Customer { get; set; }  // Nested object
    public DateTime OrderDate { get; set; }
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

public class OrderDto
{
    public int Id { get; set; }
    public CustomerDto Customer { get; set; }  // Nested DTO
    public DateTime OrderDate { get; set; }
}

public class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

// Profiles
public class OrderMappingProfile : MappingProfile
{
    public OrderMappingProfile()
    {
        CreateMap<Order, OrderDto>();
    }
}

public class CustomerMappingProfile : MappingProfile
{
    public CustomerMappingProfile()
    {
        CreateMap<Customer, CustomerDto>();
    }
}

// Usage - Nested object automatically mapped
Order order = GetOrder();
OrderDto dto = _mapper.Map<Order, OrderDto>(order);
// dto.Customer is automatically mapped!
```

---

## Collections with Complex Elements

### Entity with Collection Property

```csharp
// Models
public class Order
{
    public int Id { get; set; }
    public Customer Customer { get; set; }
    public List<OrderItem> Items { get; set; }  // Collection of complex objects
}

public class OrderItem
{
    public int Id { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public class OrderDto
{
    public int Id { get; set; }
    public CustomerDto Customer { get; set; }
    public List<OrderItemDto> Items { get; set; }  // Collection of DTOs
}

public class OrderItemDto
{
    public int Id { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

// Profiles
public class OrderMappingProfile : MappingProfile
{
    public OrderMappingProfile()
    {
        CreateMap<Order, OrderDto>();
        CreateMap<OrderItem, OrderItemDto>();  // Required for collection mapping
    }
}

// Usage
Order order = GetOrderWithItems();
OrderDto dto = _mapper.Map<Order, OrderDto>(order);
// dto.Items collection is automatically mapped with all items!
```

### Array Properties

```csharp
public class Product
{
    public int Id { get; set; }
    public string[] Tags { get; set; }  // Array of strings
    public Image[] Images { get; set; }  // Array of complex objects
}

public class ProductDto
{
    public int Id { get; set; }
    public string[] Tags { get; set; }
    public ImageDto[] Images { get; set; }
}

// Profile
public class ProductMappingProfile : MappingProfile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, ProductDto>();
        CreateMap<Image, ImageDto>();  // Required for complex array
    }
}
```

---

## Deep Nesting

### Hierarchical Structures

```csharp
// Models - Nested hierarchy
public class Company
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Department> Departments { get; set; }
}

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Employee> Employees { get; set; }
}

public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Address Address { get; set; }
}

public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
}

// Corresponding DTOs
public class CompanyDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<DepartmentDto> Departments { get; set; }
}

public class DepartmentDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<EmployeeDto> Employees { get; set; }
}

public class EmployeeDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public AddressDto Address { get; set; }
}

public class AddressDto
{
    public string Street { get; set; }
    public string City { get; set; }
}

// Profiles - One for each type
public class CompanyMappingProfile : MappingProfile
{
    public CompanyMappingProfile()
    {
        CreateMap<Company, CompanyDto>();
        CreateMap<Department, DepartmentDto>();
        CreateMap<Employee, EmployeeDto>();
        CreateMap<Address, AddressDto>();
    }
}

// Usage - Entire hierarchy mapped automatically!
Company company = GetCompanyWithFullHierarchy();
CompanyDto dto = _mapper.Map<Company, CompanyDto>(company);
// All departments, employees, and addresses are mapped recursively!
```

---

## Common Scenarios

### Scenario 1: User with Roles

```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<UserRole> Roles { get; set; }
}

public class UserRole
{
    public int Id { get; set; }
    public string RoleName { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<string> RoleNames { get; set; }  // Flattened
}

// Profile with flattening
public class UserMappingProfile : MappingProfile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>()
            .ExecuteAfterMapping((src, dest) =>
            {
                dest.RoleNames = src.Roles.Select(r => r.RoleName).ToList();
            });
    }
}
```

### Scenario 2: Product with Categories

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Category> Categories { get; set; }
}

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<CategoryDto> Categories { get; set; }
}

// Profile
public class ProductMappingProfile : MappingProfile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, ProductDto>();
        CreateMap<Category, CategoryDto>();
    }
}
```

### Scenario 3: Empty Collections

```csharp
// SimpleMapper handles null collections gracefully
User user = new User { Id = 1, Name = "John", Roles = null };
UserDto dto = _mapper.Map<User, UserDto>(user);
// dto.Roles will be empty list, not null

// Or initialize in after-mapping
CreateMap<User, UserDto>()
    .ExecuteAfterMapping((src, dest) =>
    {
        dest.Roles ??= new List<RoleDto>();
    });
```

---

## Performance Considerations

### Collection Mapping Performance

Collection mapping uses reflection for flexibility:
- **Simple properties**: ~80-90ns per object (compiled expressions)
- **Collection elements**: Uses reflection for dynamic type handling
- **Trade-off**: Flexibility over speed

### Optimization Tips

1. **Avoid Unnecessary Nesting**
   ```csharp
   // ✅ Flat when possible
   public class OrderDto
   {
       public int CustomerId { get; set; }
       public string CustomerName { get; set; }
   }
   
   // ⚠️ Nested when necessary
   public class OrderDto
   {
       public CustomerDto Customer { get; set; }
   }
   ```

2. **Consider Pagination**
   ```csharp
   // ❌ Map 10,000 items at once
   var allItems = _mapper.Map<Item, ItemDto>(allItems).ToList();
   
   // ✅ Map page by page
   var page = items.Skip(offset).Take(pageSize);
   var dtos = _mapper.Map<Item, ItemDto>(page).ToList();
   ```

3. **Profile Collections Once**
   ```csharp
   // Collection mapping reuses compiled expressions for elements
   List<User> users = GetThousandUsers();
   var dtos = _mapper.Map<User, UserDto>(users);  // First user compiles, rest reuse
   ```

---

## Best Practices

### ✅ DO

1. **Create Profiles for All Types**
   ```csharp
   CreateMap<Order, OrderDto>();
   CreateMap<OrderItem, OrderItemDto>();  // ✅ Required
   ```

2. **Use Consistent Collection Types**
   ```csharp
   public List<OrderItem> Items { get; set; }  // ✅ Consistent
   ```

3. **Handle Null Collections**
   ```csharp
   .ExecuteAfterMapping((src, dest) => dest.Items ??= new List<ItemDto>())
   ```

### ❌ DON'T

1. **Don't Forget Nested Profiles**
   ```csharp
   CreateMap<Order, OrderDto>();
   // ❌ Forgot OrderItem mapping - will fail at runtime
   ```

2. **Don't Map Huge Collections Without Pagination**
   ```csharp
   var million = _mapper.Map<Item, ItemDto>(millionItems);  // ❌ Memory issue
   ```

---

## Troubleshooting

### Problem: Collection Not Mapping

**Symptoms:** Collection property is null or empty after mapping

**Solutions:**
1. Verify mapping profile exists for element type
2. Check property names match
3. Ensure collection is not null in source
4. Verify profile is registered

### Problem: Nested Object is Null

**Symptoms:** Nested property not mapped

**Solutions:**
1. Create mapping profile for nested type
2. Verify nested object exists in source
3. Check property accessibility (public)

---

## Next Steps

- **[Troubleshooting Guide](_howto_troubleshooting.md)** - Fix issues
- **[Performance Explanation](_explanation_performance.md)** - Understand characteristics
- **[API Reference](_reference_api.md)** - Explore methods
