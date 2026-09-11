# How to Map Records, Init-Only Properties, and Required Members

**Document Type:** How-to Guide (Problem-Oriented)  
**Time to Complete:** 10 minutes  
**Difficulty:** Intermediate

## Problem

You want to use SimpleMapper with destination types that don't have a public parameterless constructor - for example:
- Positional `record` or `record struct` types
- Classes with `init`-only properties
- Classes with `required` members

Before version 2.0.0, SimpleMapper required `TDestination : class, new()`, which made these destination shapes impossible to use directly.

## Solution Overview

Starting with version 2.0.0, `TDestination` only requires `class`. SimpleMapper inspects each destination type's public constructors once, when the map is prepared, and picks an appropriate construction strategy automatically - no additional configuration is required. This guide covers:
1. Mapping to positional records
2. Mapping to `init`-only properties
3. Working with `required` members
4. What happens with ambiguous constructors
5. Mapping into an existing destination instance

## Prerequisites

- Completed [Getting Started Tutorial](_tutorial_getting_started.md)
- Understanding of [Mapping Profiles](_howto_mapping_profiles.md)

---

## Mapping to Positional Records

A positional record has exactly one constructor (the primary constructor) and no parameterless constructor. SimpleMapper detects this automatically and uses that constructor to create the instance, then maps properties over it as usual.

```csharp
public class User
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
}

public record UserDto(int Id, string Name, string Email);

public class UserMappingProfile : MappingProfile
{
	public UserMappingProfile()
	{
		CreateMap<User, UserDto>();
	}
}
```

```csharp
User user = GetUser();
UserDto dto = _mapper.Map<User, UserDto>(user);
```

**How it works:** SimpleMapper invokes `UserDto`'s primary constructor once with placeholder default values (e.g. `0`, `null`) purely to create the instance. Every property is then immediately overwritten by the normal property-mapping pipeline, so none of the placeholder values are ever observable in the result.

---

## Mapping to Init-Only Properties

`init`-only properties can be set as part of object initialization but not afterward. SimpleMapper writes to them the same way it writes to any other settable property, because the `init` restriction is enforced by the C# compiler, not by the runtime - so it does not affect reflection-based or expression-tree-based property assignment.

```csharp
public class UserDto
{
	public int Id { get; init; }
	public string Name { get; init; } = string.Empty;
}

CreateMap<User, UserDto>();
```

No special configuration is needed - `init`-only properties are mapped automatically as long as `UserDto` also has a parameterless constructor (or a single other public constructor, as described above).

---

## Working with Required Members

The `required` keyword (not the `[Required]` attribute) forces callers to set a member during object initialization. Because SimpleMapper constructs the destination object itself, it validates every `required` member when the map is prepared:

```csharp
public class UserDto
{
	public required string Name { get; set; }
	public int Id { get; set; }
}

CreateMap<User, UserDto>();
```

This works as long as `Name` can be resolved from a matching source property (`User.Name`) or from an explicit `ForMember` configuration:

```csharp
CreateMap<User, UserDto>()
	.ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.DisplayName));
```

**If a `required` member cannot be resolved** - because no matching source property exists and no `ForMember` configuration was provided - SimpleMapper throws a `SimpleMapperException` when the map is prepared (typically on first use), rather than silently leaving the member unset:

```csharp
public class UserDto
{
	public required string UnresolvableProperty { get; set; } // No matching source property
}

CreateMap<User, UserDto>(); // Prepared successfully...

_mapper.Map<User, UserDto>(user); // ...but throws SimpleMapperException here
```

**Fix:** Either add a matching source property, configure a `ForMember` mapping for it, or remove the `required` modifier if the member is genuinely optional.

---

## Ambiguous Constructors

SimpleMapper can only pick a construction strategy automatically when `TDestination` has:
- a public parameterless constructor, **or**
- exactly one other public constructor

If `TDestination` has multiple non-parameterless public constructors and no parameterless constructor, SimpleMapper cannot determine which one to use, and throws a `SimpleMapperException` when the map is prepared:

```csharp
public class Ambiguous
{
	public Ambiguous(int id) { }
	public Ambiguous(int id, string name) { } // Two public constructors, no parameterless one
}

CreateMap<User, Ambiguous>(); // Throws SimpleMapperException
```

**Fix:** Add a public parameterless constructor, or reduce the type to a single non-parameterless public constructor (as with a standard positional record).

---

## Mapping into an Existing Destination Instance

`Map(source, existingDestination)` works the same way for records, `init`-only properties, and `required` members as it does for ordinary classes - the existing instance's properties are overwritten with values from `source`:

```csharp
UserDto existing = new(0, "Old Name", "old@example.com");
UserDto updated = _mapper.Map(user, existing);
```

Since records are reference types, `updated` and `existing` refer to the same object, whose properties have been overwritten in place. Any `required` members on `existing` are already set (since the object was constructed), so no additional validation occurs at this point.

---

## Summary

| Destination shape | Supported? | Notes |
|---|---|---|
| Class with parameterless constructor | ✅ | Unchanged behavior |
| Positional record / class with single non-parameterless constructor | ✅ | Constructor invoked with placeholders, then overwritten |
| Class/record with `init`-only properties | ✅ | Written like any other settable property |
| Class/record with `required` members | ✅ | Validated at map-preparation time; throws if unresolvable |
| Class with multiple non-parameterless public constructors, no parameterless one | ❌ | Throws `SimpleMapperException` (ambiguous) |

## See Also

- [API Reference - Destination Construction](_reference_api.md#destination-construction)
- [Configuration Reference](_reference_configuration.md)
- [Architecture & Design - Why Constructor-Selection Instead of a Strict new() Constraint?](_explanation_architecture.md)
- [Troubleshooting](_howto_troubleshooting.md)
