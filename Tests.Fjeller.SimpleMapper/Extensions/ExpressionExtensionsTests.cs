using Fjeller.SimpleMapper.Extensions;
using System.Linq.Expressions;
using System.Reflection;
using Tests.Fjeller.SimpleMapper.TestInfrastructure;

namespace Tests.Fjeller.SimpleMapper.Extensions;

/// ======================================================================================================================
/// <summary>
/// Tests for expression extension methods used for property extraction
/// </summary>
/// ======================================================================================================================
public class ExpressionExtensionsTests
{
	[Fact]
	public void FindProperty_Should_ExtractProperty_When_SimplePropertyExpression()
	{
		Expression<Func<SourceModel, object>> expression = x => x.Name;

		PropertyInfo? property = expression.FindProperty();

		Assert.NotNull(property);
		Assert.Equal(nameof(SourceModel.Name), property.Name);
	}

	[Fact]
	public void FindProperty_Should_ExtractProperty_When_IntPropertyExpression()
	{
		Expression<Func<SourceModel, object>> expression = x => x.Id;

		PropertyInfo? property = expression.FindProperty();

		Assert.NotNull(property);
		Assert.Equal(nameof(SourceModel.Id), property.Name);
	}

	[Fact]
	public void FindProperty_Should_ThrowException_When_NestedPropertyAccessed()
	{
		Expression<Func<SourceEntity, object>> expression = x => x.Name.Length;

		Assert.Throws<ArgumentException>(() => expression.FindProperty());
	}

	[Fact]
	public void FindProperty_Should_HandleConversion_When_ValueTypeExpression()
	{
		Expression<Func<SourceModel, object>> expression = x => x.Id;

		PropertyInfo? property = expression.FindProperty();

		Assert.NotNull(property);
		Assert.Equal(nameof(SourceModel.Id), property.Name);
		Assert.Equal(typeof(int), property.PropertyType);
	}

	[Theory]
	[InlineData(nameof(SourceModel.Id))]
	[InlineData(nameof(SourceModel.Name))]
	[InlineData(nameof(SourceModel.Email))]
	public void FindProperty_Should_ExtractCorrectProperty_When_DifferentProperties(string propertyName)
	{
		Expression<Func<SourceModel, object>> expression = propertyName switch
		{
			nameof(SourceModel.Id) => x => x.Id,
			nameof(SourceModel.Name) => x => x.Name,
			nameof(SourceModel.Email) => x => x.Email,
			_ => throw new ArgumentException("Unknown property")
		};

		PropertyInfo? property = expression.FindProperty();

		Assert.NotNull(property);
		Assert.Equal(propertyName, property.Name);
	}

	[Fact]
	public void FindMember_Should_ExtractMember_When_MemberAccessExpression()
	{
		Expression<Func<SourceModel, object>> expression = x => x.Name;

		MemberInfo member = expression.FindMember();

		Assert.NotNull(member);
		Assert.Equal(nameof(SourceModel.Name), member.Name);
	}

	[Fact]
	public void FindMember_Should_HandleLambda_When_LambdaExpression()
	{
		Expression<Func<SourceModel, object>> expression = x => x.Id;

		MemberInfo member = expression.FindMember();

		Assert.NotNull(member);
		Assert.IsAssignableFrom<PropertyInfo>(member);
	}

	[Fact]
	public void FindMember_Should_ThrowException_When_NonMemberExpression()
	{
		Expression<Func<SourceModel, object>> expression = x => x.Id + 1;

		Assert.Throws<Exception>(() => expression.FindMember());
	}

	[Fact]
	public void FindProperty_Should_ThrowException_When_NotPropertyExpression()
	{
		Expression<Func<SourceModel, object>> expression = x => x.Id + 10;

		Assert.Throws<Exception>(() => expression.FindProperty());
	}

	[Fact]
	public void FindProperty_Should_WorkWithBooleanProperties_When_Accessed()
	{
		Expression<Func<SourceModel, object>> expression = x => x.IsActive;

		PropertyInfo? property = expression.FindProperty();

		Assert.NotNull(property);
		Assert.Equal(nameof(SourceModel.IsActive), property.Name);
		Assert.Equal(typeof(bool), property.PropertyType);
	}

	[Fact]
	public void FindMember_Should_ReturnPropertyInfo_When_PropertyAccessed()
	{
		Expression<Func<SourceModel, object>> expression = x => x.Email;

		MemberInfo member = expression.FindMember();

		Assert.NotNull(member);
		PropertyInfo? propertyInfo = member as PropertyInfo;
		Assert.NotNull(propertyInfo);
		Assert.Equal(typeof(string), propertyInfo.PropertyType);
	}

	[Fact]
	public void FindProperty_Should_HandleConversion_When_BoxingRequired()
	{
		Expression<Func<SourceModel, object>> expression = x => x.Age;

		PropertyInfo? property = expression.FindProperty();

		Assert.NotNull(property);
		Assert.Equal(nameof(SourceModel.Age), property.Name);
		Assert.Equal(typeof(int), property.PropertyType);
	}
}
