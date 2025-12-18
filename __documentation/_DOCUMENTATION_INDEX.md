# SimpleMapper Documentation Index

**Quick Navigation** | [Tutorial](#tutorial) | [How-to Guides](#how-to-guides) | [Reference](#reference) | [Explanation](#explanation)

---

## 📖 Tutorial (Learning Path)

Start here if you're new to SimpleMapper:

### [Getting Started Tutorial](_tutorial_getting_started.md)
**⏱ 15 minutes** | Beginner

Learn SimpleMapper from scratch by building a simple ASP.NET Core API that maps users to DTOs.

**You'll learn:**
- Install SimpleMapper
- Create mapping profiles
- Register in dependency injection
- Use in controllers
- Map collections

---

## 🔧 How-to Guides (Problem Solving)

Solve specific problems with step-by-step instructions:

### [Configure Dependency Injection](_howto_dependency_injection.md)
**⏱ 10 minutes** | Beginner to Intermediate

Register SimpleMapper in ASP.NET Core, Blazor, and Minimal APIs.

**Covers:**
- Assembly scanning (recommended)
- Explicit profile registration
- Configuration options
- Multiple application types
- Service lifetime
- Troubleshooting

---

### [Create Mapping Profiles](_howto_mapping_profiles.md)
**⏱ 10 minutes** | Beginner to Intermediate

Configure how objects are mapped between types.

**Covers:**
- Basic profiles
- Ignoring properties
- Custom property mappings
- After-mapping actions
- Profile organization
- Common patterns
- Real-world examples

---

### [Create Custom Property Mappings with ForMember](_howto_custom_property_mapping.md)
**⏱ 15 minutes** | Intermediate

Configure explicit custom mappings for individual properties.

**Covers:**
- Basic property-to-property mapping with different names
- Computed values and transformations
- Combining multiple source properties
- Working with ForMember alongside other features
- Advanced scenarios (nested properties, null handling)
- Common patterns
- Best practices

---

### [Map Collections and Nested Objects](_howto_collections.md)
**⏱ 10 minutes** | Intermediate

Handle lists, arrays, and complex object graphs.

**Covers:**
- Simple collections
- Nested objects
- Deep mapping
- Hierarchical structures
- Performance considerations
- Best practices

---

### [Troubleshooting Guide](_howto_troubleshooting.md)
**⏱ 5-15 minutes** | All Levels

Fix common problems and errors.

**Covers:**
- "No mapping available" error
- Profiles not discovered
- Cannot resolve ISimpleMapper
- Properties not mapping
- Collections not mapping
- Performance issues
- Debugging techniques

---

## 📚 Reference (Technical Specifications)

Look up specific technical details:

### [API Reference](_reference_api.md)
**📖 Dictionary** | All Levels

Complete API documentation with signatures, parameters, and examples.

**Includes:**
- `ISimpleMapper` interface (5 overloads)
- `ISimpleMap<TSource, TDestination>` interface
- `MappingProfile` base class
- Extension methods
- Exceptions
- Type compatibility

---

### [Configuration Reference](_reference_configuration.md)
**📖 Dictionary** | All Levels

Complete configuration options and settings.

**Includes:**
- Registration methods (6 variants)
- Service lifetime details
- `SimpleMapperOptions` methods
- Profile configuration methods
- Assembly scanning rules
- Best practices

---

## 💡 Explanation (Understanding Concepts)

Deepen your understanding of how SimpleMapper works:

### [Architecture and Design](_explanation_architecture.md)
**📝 Discussion** | Experienced Developers

Understand internal design and why decisions were made.

**Covers:**
- Design philosophy
- Core components
- Expression tree compilation
- Caching strategy
- Design decisions rationale
- Design patterns
- Extension points

---

### [Performance Characteristics](_explanation_performance.md)
**📝 Discussion** | Performance-Conscious Developers

Understand benchmarks, optimization, and when to use SimpleMapper.

**Covers:**
- Benchmark results (Simple: 80ns, Complex: 90ns)
- Why it's fast
- Performance by scenario
- Memory characteristics
- Optimization journey
- Trade-offs
- Real-world case studies

---

## 🎯 Quick Links by Goal

### I want to...

**Learn SimpleMapper from scratch**
→ [Getting Started Tutorial](_tutorial_getting_started.md)

**Register SimpleMapper in my app**
→ [Dependency Injection How-to](_howto_dependency_injection.md)

**Configure a mapping**
→ [Mapping Profiles How-to](_howto_mapping_profiles.md)

**Map properties with different names**
→ [Custom Property Mappings How-to](_howto_custom_property_mapping.md)

**Map a collection**
→ [Collections How-to](_howto_collections.md)

**Fix an error**
→ [Troubleshooting Guide](_howto_troubleshooting.md)

**Look up a method**
→ [API Reference](_reference_api.md)

**Check configuration options**
→ [Configuration Reference](_reference_configuration.md)

**Understand how it works**
→ [Architecture Explanation](_explanation_architecture.md)

**Optimize performance**
→ [Performance Explanation](_explanation_performance.md)

---

## 📊 Documentation by Audience

### Beginners

1. ✅ [Getting Started Tutorial](_tutorial_getting_started.md)
2. ✅ [Dependency Injection How-to](_howto_dependency_injection.md)
3. ✅ [Mapping Profiles How-to](_howto_mapping_profiles.md)
4. ✅ [Troubleshooting Guide](_howto_troubleshooting.md)

### Intermediate

1. ✅ [Custom Property Mappings How-to](_howto_custom_property_mapping.md)
2. ✅ [Collections How-to](_howto_collections.md)
3. ✅ [API Reference](_reference_api.md)
4. ✅ [Configuration Reference](_reference_configuration.md)

### Advanced

1. ✅ [Architecture Explanation](_explanation_architecture.md)
2. ✅ [Performance Explanation](_explanation_performance.md)
3. ✅ All Reference documents

---

## 📂 Documentation Structure

```
__documentation/
├── _tutorial_getting_started.md          (Tutorial)
├── _howto_dependency_injection.md        (How-to Guide)
├── _howto_mapping_profiles.md            (How-to Guide)
├── _howto_custom_property_mapping.md     (How-to Guide)
├── _howto_collections.md                 (How-to Guide)
├── _howto_troubleshooting.md             (How-to Guide)
├── _reference_api.md                     (Reference)
├── _reference_configuration.md           (Reference)
├── _explanation_architecture.md          (Explanation)
├── _explanation_performance.md           (Explanation)
├── _DOCUMENTATION_INDEX.md               (This file)
└── _DOCUMENTATION_SUMMARY.md             (Generation summary)
```

---

## 🎓 Learning Paths

### Path 1: Quick Start (30 minutes)
1. [Getting Started Tutorial](_tutorial_getting_started.md) (15 min)
2. [Dependency Injection How-to](_howto_dependency_injection.md) (10 min)
3. Start building!

### Path 2: Comprehensive (2 hours)
1. [Getting Started Tutorial](_tutorial_getting_started.md)
2. [Dependency Injection How-to](_howto_dependency_injection.md)
3. [Mapping Profiles How-to](_howto_mapping_profiles.md)
4. [Custom Property Mappings How-to](_howto_custom_property_mapping.md)
5. [Collections How-to](_howto_collections.md)
6. [API Reference](_reference_api.md) (skim)
7. [Troubleshooting Guide](_howto_troubleshooting.md) (reference)

### Path 3: Performance Deep Dive (1 hour)
1. [Getting Started Tutorial](_tutorial_getting_started.md) (quick skim)
2. [Performance Explanation](_explanation_performance.md) (read thoroughly)
3. [Architecture Explanation](_explanation_architecture.md) (read thoroughly)

---

## 🔍 Search by Topic

### Configuration
- [Dependency Injection](_howto_dependency_injection.md)
- [Configuration Reference](_reference_configuration.md)
- [Mapping Profiles](_howto_mapping_profiles.md)

### Mapping
- [Mapping Profiles How-to](_howto_mapping_profiles.md)
- [Custom Property Mappings How-to](_howto_custom_property_mapping.md)
- [Collections How-to](_howto_collections.md)
- [API Reference](_reference_api.md)

### Troubleshooting
- [Troubleshooting Guide](_howto_troubleshooting.md)
- [Configuration Reference](_reference_configuration.md)

### Performance
- [Performance Explanation](_explanation_performance.md)
- [Architecture Explanation](_explanation_architecture.md)

### API Details
- [API Reference](_reference_api.md)
- [Configuration Reference](_reference_configuration.md)

---

## 📱 Mobile-Friendly Navigation

**Tutorials**: Learn step-by-step
- [Getting Started](_tutorial_getting_started.md)

**How-to**: Solve problems
- [DI Setup](_howto_dependency_injection.md)
- [Profiles](_howto_mapping_profiles.md)
- [Custom Properties](_howto_custom_property_mapping.md)
- [Collections](_howto_collections.md)
- [Fixes](_howto_troubleshooting.md)

**Reference**: Look up
- [API](_reference_api.md)
- [Config](_reference_configuration.md)

**Explain**: Understand
- [Design](_explanation_architecture.md)
- [Speed](_explanation_performance.md)

---

## ❓ Frequently Asked Questions

**Q: Where do I start?**  
A: [Getting Started Tutorial](_tutorial_getting_started.md)

**Q: How do I register SimpleMapper?**  
A: [Dependency Injection How-to](_howto_dependency_injection.md)

**Q: How do I map properties with different names?**  
A: [Custom Property Mappings How-to](_howto_custom_property_mapping.md)

**Q: How do I ignore properties?**  
A: [Mapping Profiles How-to](_howto_mapping_profiles.md) - Ignoring Properties section

**Q: How do I map collections?**  
A: [Collections How-to](_howto_collections.md)

**Q: I'm getting an error, what now?**  
A: [Troubleshooting Guide](_howto_troubleshooting.md)

**Q: What's the performance like?**  
A: [Performance Explanation](_explanation_performance.md) - ~80-90ns per mapping

**Q: How does it work internally?**  
A: [Architecture Explanation](_explanation_architecture.md)

---

## 🌟 Popular Pages

Most visited documentation pages:

1. [Getting Started Tutorial](_tutorial_getting_started.md)
2. [Dependency Injection How-to](_howto_dependency_injection.md)
3. [Troubleshooting Guide](_howto_troubleshooting.md)
4. [API Reference](_reference_api.md)
5. [Performance Explanation](_explanation_performance.md)

---

## 📝 Document Types Explained

### Tutorial 📖
**Purpose**: Learn by doing  
**Format**: Step-by-step lesson  
**Outcome**: Working implementation  
**Example**: Build your first mapping

### How-to Guide 🔧
**Purpose**: Solve specific problem  
**Format**: Recipe with steps  
**Outcome**: Task completed  
**Example**: Configure dependency injection

### Reference 📚
**Purpose**: Look up information  
**Format**: Dictionary/specification  
**Outcome**: Found answer  
**Example**: Method signature

### Explanation 💡
**Purpose**: Understand concept  
**Format**: Discussion  
**Outcome**: Deeper understanding  
**Example**: Why expression trees are fast

---

## 🔗 External Resources

- **GitHub Repository**: [github.com/fjeller/fjeller.simplemapper](https://github.com/fjeller/fjeller.simplemapper)
- **Issues**: [Report bugs or request features](https://github.com/fjeller/fjeller.simplemapper/issues)
- **Discussions**: [Ask questions](https://github.com/fjeller/fjeller.simplemapper/discussions)
- **NuGet**: [Coming soon]

---

## 📄 About This Documentation

**Framework**: [Diátaxis](https://diataxis.fr/)  
**Generated**: December 2024  
**Version**: 1.0  
**Status**: Production-ready  

**Contributions Welcome!**  
Found an error or have a suggestion? Submit a PR or open an issue.

---

**Happy Mapping! 🚀**
