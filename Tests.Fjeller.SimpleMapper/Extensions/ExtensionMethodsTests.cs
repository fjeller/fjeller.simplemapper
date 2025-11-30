using Fjeller.SimpleMapper.Extensions;
using Tests.Fjeller.SimpleMapper.TestInfrastructure;

namespace Tests.Fjeller.SimpleMapper.Extensions;

/// ======================================================================================================================
/// <summary>
/// Tests for extension methods in the Extensions namespace
/// </summary>
/// ======================================================================================================================
public class ExtensionMethodsTests
{
	[Fact]
	public void WhereNotNull_Should_FilterNulls_When_CollectionContainsNulls()
	{
		List<string?> items = new() { "A", null, "B", null, "C" };

		IEnumerable<string> result = items.WhereNotNull();

		Assert.NotNull(result);
		List<string> resultList = result.ToList();
		Assert.Equal(3, resultList.Count);
		Assert.Equal("A", resultList[0]);
		Assert.Equal("B", resultList[1]);
		Assert.Equal("C", resultList[2]);
	}

	[Fact]
	public void WhereNotNull_Should_ReturnEmpty_When_AllItemsAreNull()
	{
		List<string?> items = new() { null, null, null };

		IEnumerable<string> result = items.WhereNotNull();

		Assert.NotNull(result);
		Assert.Empty(result);
	}

	[Fact]
	public void WhereNotNull_Should_ReturnAll_When_NoNullsExist()
	{
		List<string?> items = new() { "A", "B", "C" };

		IEnumerable<string> result = items.WhereNotNull();

		Assert.NotNull(result);
		Assert.Equal(3, result.Count());
	}

	[Fact]
	public void AddIfNotNull_Should_AddItem_When_ItemIsNotNull()
	{
		List<string> list = new();
		string item = "test";

		list.AddIfNotNull(item);

		Assert.Single(list);
		Assert.Equal("test", list[0]);
	}

	[Fact]
	public void AddIfNotNull_Should_NotAdd_When_ItemIsNull()
	{
		List<string> list = new();
		string? item = null;

		list.AddIfNotNull(item);

		Assert.Empty(list);
	}

	[Fact]
	public void AddIfNotContains_Should_AddItem_When_ItemNotInList()
	{
		List<int> list = new() { 1, 2, 3 };

		list.AddIfNotContains(4);

		Assert.Equal(4, list.Count);
		Assert.Contains(4, list);
	}

	[Fact]
	public void AddIfNotContains_Should_NotAdd_When_ItemAlreadyInList()
	{
		List<int> list = new() { 1, 2, 3 };

		list.AddIfNotContains(2);

		Assert.Equal(3, list.Count);
	}

	[Fact]
	public void AddIfNotContains_Should_NotAdd_When_ItemIsNull()
	{
		List<string> list = new() { "A", "B" };
		string? item = null;

		list.AddIfNotContains(item);

		Assert.Equal(2, list.Count);
	}

	[Fact]
	public void GetPropertyInfo_Should_ReturnProperty_When_PropertyExists()
	{
		Type type = typeof(SourceModel);

		System.Reflection.PropertyInfo? property = type.GetPropertyInfo(nameof(SourceModel.Name));

		Assert.NotNull(property);
		Assert.Equal(nameof(SourceModel.Name), property.Name);
	}

	[Fact]
	public void GetPropertyInfo_Should_ReturnNull_When_PropertyDoesNotExist()
	{
		Type type = typeof(SourceModel);

		System.Reflection.PropertyInfo? property = type.GetPropertyInfo("NonExistent");

		Assert.Null(property);
	}

	[Fact]
	public void GetPropertyInfos_Should_ReturnAllProperties_When_Called()
	{
		Type type = typeof(SourceModel);

		IEnumerable<System.Reflection.PropertyInfo> properties = type.GetPropertyInfos();

		Assert.NotNull(properties);
		Assert.NotEmpty(properties);
		Assert.Contains(properties, p => p.Name == nameof(SourceModel.Id));
		Assert.Contains(properties, p => p.Name == nameof(SourceModel.Name));
	}

	[Fact]
	public void GetPropertyInfos_Should_ReturnMultipleProperties_When_NamesProvided()
	{
		Type type = typeof(SourceModel);
		string[] names = new[] { nameof(SourceModel.Id), nameof(SourceModel.Name) };

		IEnumerable<System.Reflection.PropertyInfo> properties = type.GetPropertyInfos(names);

		Assert.NotNull(properties);
		Assert.Equal(2, properties.Count());
	}

	[Fact]
	public void GetPropertyInfos_Should_SkipNonExistent_When_InvalidNameProvided()
	{
		Type type = typeof(SourceModel);
		string[] names = new[] { nameof(SourceModel.Id), "NonExistent", nameof(SourceModel.Name) };

		IEnumerable<System.Reflection.PropertyInfo> properties = type.GetPropertyInfos(names);

		Assert.NotNull(properties);
		Assert.Equal(2, properties.Count());
	}

	[Fact]
	public void CreateMapKey_Should_GenerateKey_When_TypesProvided()
	{
		Type sourceType = typeof(SourceModel);
		Type destinationType = typeof(DestinationModel);

		string key = (sourceType, destinationType).CreateMapKey();

		Assert.NotNull(key);
		Assert.Contains(sourceType.FullName!, key);
		Assert.Contains(destinationType.FullName!, key);
		Assert.Contains("_", key);
	}

	[Theory]
	[InlineData(typeof(int), typeof(string))]
	[InlineData(typeof(SourceModel), typeof(DestinationModel))]
	[InlineData(typeof(List<int>), typeof(List<string>))]
	public void CreateMapKey_Should_GenerateUniqueKey_When_DifferentTypesProvided(Type sourceType, Type destinationType)
	{
		string key = (sourceType, destinationType).CreateMapKey();

		Assert.NotNull(key);
		Assert.NotEmpty(key);
	}

	[Fact]
	public void GetCorrectSourceType_Should_ReturnOriginalType_When_NotEntityFrameworkProxy()
	{
		SourceModel obj = new();

		Type result = obj.GetCorrectSourceType();

		Assert.Equal(typeof(SourceModel), result);
	}

	[Theory]
	[InlineData(new[] { 1, 2, 3 }, 3)]
	[InlineData(new[] { 1 }, 1)]
	[InlineData(new int[] { }, 0)]
	public void WhereNotNull_Should_HandleDifferentSizes_When_CollectionProvided(int[] values, int expectedCount)
	{
		List<int?> nullableValues = values.Select(v => (int?)v).ToList();

		int resultCount = nullableValues.WhereNotNull().Count();

		Assert.Equal(expectedCount, resultCount);
	}

	[Fact]
	public void AddIfNotNull_Should_HandleComplexTypes_When_ObjectsProvided()
	{
		List<SourceModel> list = new();
		SourceModel item = new() { Id = 1, Name = "Test" };

		list.AddIfNotNull(item);

		Assert.Single(list);
		Assert.Equal(1, list[0].Id);
	}

	[Fact]
	public void AddIfNotContains_Should_UseDefaultEquality_When_NoComparerProvided()
	{
		List<string> list = new() { "A", "B", "C" };

		list.AddIfNotContains("A");
		list.AddIfNotContains("D");

		Assert.Equal(4, list.Count);
		Assert.Equal("D", list[3]);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(1)]
	[InlineData(5)]
	[InlineData(100)]
	public void AddIfNotNull_Should_HandleMultipleAdditions_When_ItemsNotNull(int count)
	{
		List<int> list = new();

		for (int i = 0; i < count; i++)
		{
			list.AddIfNotNull(i);
		}

		Assert.Equal(count, list.Count);
	}

	[Fact]
	public void GetPropertyInfos_Should_ReturnEmpty_When_NoMatchingNames()
	{
		Type type = typeof(SourceModel);
		string[] names = new[] { "NonExistent1", "NonExistent2" };

		IEnumerable<System.Reflection.PropertyInfo> properties = type.GetPropertyInfos(names);

		Assert.NotNull(properties);
		Assert.Empty(properties);
	}

	[Fact]
	public void CreateMapKey_Should_BeConsistent_When_CalledMultipleTimes()
	{
		Type sourceType = typeof(SourceModel);
		Type destinationType = typeof(DestinationModel);

		string key1 = (sourceType, destinationType).CreateMapKey();
		string key2 = (sourceType, destinationType).CreateMapKey();

		Assert.Equal(key1, key2);
	}
}
