# Changelog

All notable changes to this project are documented in this file.

## [2.0.0]

### Added

- Support for destination types that do not have a public parameterless constructor, such as positional
  `record`/`record struct` types. SimpleMapper now inspects the destination type's public constructors and:
  - Uses the parameterless constructor if one exists (unchanged, existing behavior).
  - Otherwise, if the destination type has exactly one other public constructor, invokes it with placeholder
	default values for its parameters to create the instance. Those placeholder values are always overwritten
	immediately afterwards by the normal property-mapping pipeline, so no data from the constructor call ends up
	in the final result.
  - Throws a `SimpleMapperException` at map-preparation time if the destination type has neither a parameterless
	constructor nor exactly one other public constructor, since the correct constructor to use would be ambiguous.
- Support for `init`-only destination properties and positional record properties as mapping targets.
- Validation of `required` destination members (the C# `required` keyword): if a `required` property cannot be
  resolved from a matching source property or a `ForMember`/`MapFrom` configuration, a `SimpleMapperException` is
  thrown at map-preparation time instead of silently leaving the member unset.
- New `ISimpleMap.CreateDestination()` member exposing the resolved construction strategy for a given map.

### Changed

- **Breaking change:** The `TDestination` generic constraint was relaxed from `class, new()` to `class` across the
  public API (`ISimpleMapper`, `SimpleMapper`, `MappingProfile.CreateMap`, `ISimpleMap<TSource, TDestination>`,
  `ISimpleMap.Generic`, `PropertyMappingOptions<TSource, TDestination>`). This is source-compatible for existing
  callers, but is a **binary-breaking change**: consumers must recompile against the new version.
- Destination object creation now goes through the map's constructor-selection logic
  (`ISimpleMap.CreateDestination()`) instead of calling `new TDestination()` or `Activator.CreateInstance` directly,
  both for top-level mapping and for complex collection elements.

### Notes

- Existing mappings that use destination types with a public parameterless constructor are unaffected; behavior for
  those destination types is unchanged.
- The compiled expression-tree mapping pipeline (`CompiledMapCache`) already supported writing to `init`-only and
  record properties via `Expression.Assign`/`PropertyInfo.SetValue`, since the C# `init` restriction is enforced
  only by the C# compiler, not by the CLR or expression trees. No rewrite of the compiled mapping codegen was
  required to support this.
- Destination construction (`ISimpleMap.CreateDestination()`) compiles and caches a factory delegate
  (`Expression.New(...)`) per map instead of calling `Activator.CreateInstance`/`ConstructorInfo.Invoke` on every
  mapping call, so introducing constructor-based construction does not add reflection overhead to the hot path.

## [1.1.1]

### Security

- Added an explicit `Microsoft.Build.Tasks.Git` package reference to address a reported vulnerability in a
  transitive dependency.

## [1.1.0]

> **Note:** The Git tag `1.1.0` was created for this release, but the `<Version>` element in
> `Fjeller.SimpleMapper.csproj` was not updated at the time and remained `1.0.0`.

### Added

- Automatic (deep) mapping support for collection-valued properties, including `List<T>`, `IEnumerable<T>`,
  `IReadOnlyCollection<T>`, `HashSet<T>`, and arrays. Collection elements are mapped individually using the
  configured map for their element types, so nested complex object graphs inside collections are properly
  translated between source and destination shapes instead of being shallow-copied or skipped.

## [1.0.0]

- Initial release of SimpleMapper with compiled expression trees, dependency injection support, and collection
  mapping.
