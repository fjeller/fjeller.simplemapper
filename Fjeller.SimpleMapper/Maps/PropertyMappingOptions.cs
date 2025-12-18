using System;
using System.Linq.Expressions;

namespace Fjeller.SimpleMapper.Maps;

/// ======================================================================================================================
/// <summary>
/// Configuration options for custom property mapping using ForMember
/// </summary>
/// <typeparam name="TSource">The source type</typeparam>
/// <typeparam name="TDestination">The destination type</typeparam>
/// ======================================================================================================================
public class PropertyMappingOptions<TSource, TDestination>
	where TSource : class
	where TDestination : class, new()
{
	/// ======================================================================================================================
	/// <summary>
	/// The source expression that defines where to get the value from
	/// </summary>
	/// ======================================================================================================================
	internal Expression<Func<TSource, object>>? SourceExpression { get; private set; }

	/// ======================================================================================================================
	/// <summary>
	/// Specifies the source property or expression to map from
	/// </summary>
	/// <param name="sourceExpression">Expression that selects the source property or computes the value</param>
	/// ======================================================================================================================
	public void MapFrom( Expression<Func<TSource, object>> sourceExpression )
	{
		SourceExpression = sourceExpression;
	}
}
