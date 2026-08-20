using Fjeller.SimpleMapper;
using Tests.Fjeller.SimpleMapper.TestInfrastructure;

namespace Tests.Fjeller.SimpleMapper;

/// ======================================================================================================================
/// <summary>
/// Tests for mapping collection properties whose element types differ between source and destination
/// (e.g. List&lt;C&gt; -> List&lt;D&gt;) as long as a mapping between the element types is registered.
/// Covers List&lt;T&gt;, arrays, IEnumerable&lt;T&gt;, IReadOnlyCollection&lt;T&gt;, IReadOnlyList&lt;T&gt;,
/// ICollection&lt;T&gt;, IList&lt;T&gt; and HashSet&lt;T&gt; destination shapes.
/// </summary>
/// ======================================================================================================================
public class InnerListMappingTests : IDisposable
{
	public InnerListMappingTests()
	{
		TestHelper.ResetMapperCache();
	}

	public void Dispose()
	{
		TestHelper.ResetMapperCache();
		GC.SuppressFinalize(this);
	}

	private class ElementSource
	{
		public int Id { get; set; }
		public string Label { get; set; } = string.Empty;
	}

	private class ElementDestination
	{
		public int Id { get; set; }
		public string Label { get; set; } = string.Empty;
	}

	private class ContainerSource
	{
		public int Id { get; set; }
		public List<ElementSource> ListItems { get; set; } = new();
		public ElementSource[] ArrayItems { get; set; } = Array.Empty<ElementSource>();
		public IEnumerable<ElementSource> EnumerableItems { get; set; } = new List<ElementSource>();
		public IReadOnlyCollection<ElementSource> ReadOnlyCollectionItems { get; set; } = new List<ElementSource>();
		public IReadOnlyList<ElementSource> ReadOnlyListItems { get; set; } = new List<ElementSource>();
		public ICollection<ElementSource> CollectionItems { get; set; } = new List<ElementSource>();
		public IList<ElementSource> ListInterfaceItems { get; set; } = new List<ElementSource>();
		public HashSet<ElementSource> HashSetItems { get; set; } = new();
	}

	private class ContainerDestination
	{
		public int Id { get; set; }
		public List<ElementDestination> ListItems { get; set; } = new();
		public ElementDestination[] ArrayItems { get; set; } = Array.Empty<ElementDestination>();
		public IEnumerable<ElementDestination> EnumerableItems { get; set; } = new List<ElementDestination>();
		public IReadOnlyCollection<ElementDestination> ReadOnlyCollectionItems { get; set; } = new List<ElementDestination>();
		public IReadOnlyList<ElementDestination> ReadOnlyListItems { get; set; } = new List<ElementDestination>();
		public ICollection<ElementDestination> CollectionItems { get; set; } = new List<ElementDestination>();
		public IList<ElementDestination> ListInterfaceItems { get; set; } = new List<ElementDestination>();
		public HashSet<ElementDestination> HashSetItems { get; set; } = new();
	}

	/// ======================================================================================================================
	/// <summary>
	/// Profile intentionally registers the outer (container) map BEFORE the inner (element) map, to prove that
	/// mapping registration order does not affect whether cross-type collection properties are recognized.
	/// </summary>
	/// ======================================================================================================================
	private class CrossTypeCollectionProfile : MappingProfile
	{
		public CrossTypeCollectionProfile()
		{
			CreateMap<ContainerSource, ContainerDestination>();
			CreateMap<ElementSource, ElementDestination>();
		}
	}

	private static ContainerSource CreateSourceWithItems()
	{
		List<ElementSource> items = new()
		{
			new ElementSource { Id = 1, Label = "First" },
			new ElementSource { Id = 2, Label = "Second" }
		};

		return new ContainerSource
		{
			Id = 1,
			ListItems = new List<ElementSource>(items),
			ArrayItems = items.ToArray(),
			EnumerableItems = new List<ElementSource>(items),
			ReadOnlyCollectionItems = new List<ElementSource>(items),
			ReadOnlyListItems = new List<ElementSource>(items),
			CollectionItems = new List<ElementSource>(items),
			ListInterfaceItems = new List<ElementSource>(items),
			HashSetItems = new HashSet<ElementSource>(items)
		};
	}

	[Fact]
	public void Map_Should_MapListProperty_When_ElementTypesDiffer()
	{
		new CrossTypeCollectionProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		ContainerSource source = CreateSourceWithItems();

		ContainerDestination result = mapper.Map<ContainerSource, ContainerDestination>(source);

		Assert.NotNull(result.ListItems);
		Assert.Equal(2, result.ListItems.Count);
		Assert.Equal(1, result.ListItems[0].Id);
		Assert.Equal("First", result.ListItems[0].Label);
	}

	[Fact]
	public void Map_Should_MapArrayProperty_When_ElementTypesDiffer()
	{
		new CrossTypeCollectionProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		ContainerSource source = CreateSourceWithItems();

		ContainerDestination result = mapper.Map<ContainerSource, ContainerDestination>(source);

		Assert.NotNull(result.ArrayItems);
		Assert.Equal(2, result.ArrayItems.Length);
		Assert.Equal(2, result.ArrayItems[1].Id);
		Assert.Equal("Second", result.ArrayItems[1].Label);
	}

	[Fact]
	public void Map_Should_MapIEnumerableProperty_When_ElementTypesDiffer()
	{
		new CrossTypeCollectionProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		ContainerSource source = CreateSourceWithItems();

		ContainerDestination result = mapper.Map<ContainerSource, ContainerDestination>(source);

		List<ElementDestination> enumerableResult = result.EnumerableItems.ToList();
		Assert.Equal(2, enumerableResult.Count);
		Assert.Contains(enumerableResult, e => e is { Id: 1, Label: "First" });
	}

	[Fact]
	public void Map_Should_MapIReadOnlyCollectionProperty_When_ElementTypesDiffer()
	{
		new CrossTypeCollectionProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		ContainerSource source = CreateSourceWithItems();

		ContainerDestination result = mapper.Map<ContainerSource, ContainerDestination>(source);

		Assert.Equal(2, result.ReadOnlyCollectionItems.Count);
	}

	[Fact]
	public void Map_Should_MapIReadOnlyListProperty_When_ElementTypesDiffer()
	{
		new CrossTypeCollectionProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		ContainerSource source = CreateSourceWithItems();

		ContainerDestination result = mapper.Map<ContainerSource, ContainerDestination>(source);

		Assert.Equal(2, result.ReadOnlyListItems.Count);
		Assert.Equal(1, result.ReadOnlyListItems[0].Id);
	}

	[Fact]
	public void Map_Should_MapICollectionProperty_When_ElementTypesDiffer()
	{
		new CrossTypeCollectionProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		ContainerSource source = CreateSourceWithItems();

		ContainerDestination result = mapper.Map<ContainerSource, ContainerDestination>(source);

		Assert.Equal(2, result.CollectionItems.Count);
	}

	[Fact]
	public void Map_Should_MapIListProperty_When_ElementTypesDiffer()
	{
		new CrossTypeCollectionProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		ContainerSource source = CreateSourceWithItems();

		ContainerDestination result = mapper.Map<ContainerSource, ContainerDestination>(source);

		Assert.Equal(2, result.ListInterfaceItems.Count);
		Assert.Equal(2, result.ListInterfaceItems[1].Id);
	}

	[Fact]
	public void Map_Should_MapHashSetProperty_When_ElementTypesDiffer()
	{
		new CrossTypeCollectionProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		ContainerSource source = CreateSourceWithItems();

		ContainerDestination result = mapper.Map<ContainerSource, ContainerDestination>(source);

		Assert.Equal(2, result.HashSetItems.Count);
		Assert.Contains(result.HashSetItems, e => e.Id == 1 && e.Label == "First");
	}

	[Fact]
	public void Map_Should_MapCrossTypeCollection_When_OuterMapRegisteredBeforeInnerElementMap()
	{
		// CrossTypeCollectionProfile registers CreateMap<ContainerSource, ContainerDestination>() BEFORE
		// CreateMap<ElementSource, ElementDestination>(). Validity is only resolved lazily (on first Map call,
		// after all profiles have run), so this ordering must not prevent the collection property from being mapped.
		new CrossTypeCollectionProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		ContainerSource source = CreateSourceWithItems();

		ContainerDestination result = mapper.Map<ContainerSource, ContainerDestination>(source);

		Assert.Equal(2, result.ListItems.Count);
		Assert.Equal(1, result.ListItems[0].Id);
	}

	private class NoMapElementSource
	{
		public int Value { get; set; }
	}

	private class NoMapElementDestination
	{
		public int Value { get; set; }
	}

	private class NoMapContainerSource
	{
		public int Id { get; set; }
		public List<NoMapElementSource> Items { get; set; } = new();
	}

	private class NoMapContainerDestination
	{
		public int Id { get; set; }
		public List<NoMapElementDestination> Items { get; set; } = new() { new NoMapElementDestination { Value = 999 } };
	}

	private class NoElementMapProfile : MappingProfile
	{
		public NoElementMapProfile()
		{
			// Intentionally does NOT register CreateMap<NoMapElementSource, NoMapElementDestination>().
			CreateMap<NoMapContainerSource, NoMapContainerDestination>();
		}
	}

	[Fact]
	public void Map_Should_SkipCollectionProperty_When_NoElementMapIsRegistered()
	{
		new NoElementMapProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		NoMapContainerSource source = new()
		{
			Id = 1,
			Items = new List<NoMapElementSource> { new() { Value = 1 }, new() { Value = 2 } }
		};

		NoMapContainerDestination result = mapper.Map<NoMapContainerSource, NoMapContainerDestination>(source);

		// The property is skipped entirely (no registered element map), so the destination's own default value
		// - set by its parameterless constructor - is left untouched rather than being overwritten or cleared.
		Assert.Single(result.Items);
		Assert.Equal(999, result.Items[0].Value);
	}

	private class DedupElementSource
	{
		public int Id { get; set; }
		public string Label { get; set; } = string.Empty;
	}

	private class DedupElementDestination
	{
		public int Id { get; set; }
		public string Label { get; set; } = string.Empty;

		public override bool Equals(object? obj)
		{
			return obj is DedupElementDestination other && Id == other.Id && Label == other.Label;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Id, Label);
		}
	}

	private class DedupContainerSource
	{
		public int Id { get; set; }
		public List<DedupElementSource> Items { get; set; } = new();
	}

	private class DedupContainerDestination
	{
		public int Id { get; set; }
		public HashSet<DedupElementDestination> Items { get; set; } = new();
	}

	private class DedupProfile : MappingProfile
	{
		public DedupProfile()
		{
			CreateMap<DedupContainerSource, DedupContainerDestination>();
			CreateMap<DedupElementSource, DedupElementDestination>();
		}
	}

	[Fact]
	public void Map_Should_DeduplicateItems_When_MappedHashSetElementsCompareEqual()
	{
		// HashSet<T> destinations use the destination element type's default Equals/GetHashCode for
		// deduplication, exactly like any other HashSet<T>. Two source items that map to equal destination
		// elements will therefore collapse into a single entry - this is expected, standard HashSet behavior.
		new DedupProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		DedupContainerSource source = new()
		{
			Id = 1,
			Items = new List<DedupElementSource>
			{
				new() { Id = 1, Label = "A" },
				new() { Id = 1, Label = "A" }, // duplicate of the first once mapped
				new() { Id = 2, Label = "B" }
			}
		};

		DedupContainerDestination result = mapper.Map<DedupContainerSource, DedupContainerDestination>(source);

		Assert.Equal(2, result.Items.Count);
	}
}
