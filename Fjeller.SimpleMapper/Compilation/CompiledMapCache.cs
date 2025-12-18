using Fjeller.SimpleMapper.Maps;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Fjeller.SimpleMapper.Compilation;

/// ======================================================================================================================
/// <summary>
/// Cache for compiled mapping functions using expression trees to eliminate reflection overhead
/// </summary>
/// ======================================================================================================================
internal static class CompiledMapCache
{
	private static readonly ConcurrentDictionary<string, Delegate> _compiledMappers = new();

	/// ======================================================================================================================
	/// <summary>
	/// Gets or creates a compiled mapper for the specified types. The compiled mapper uses expression trees
	/// to generate IL code at runtime that directly accesses properties without reflection.
	/// </summary>
	/// <typeparam name="TSource">The source type</typeparam>
	/// <typeparam name="TDestination">The destination type</typeparam>
	/// <param name="map">The map configuration containing property information</param>
	/// <returns>A compiled function that maps source to destination</returns>
	/// ======================================================================================================================
	internal static Func<TSource, TDestination, TDestination> GetOrCreateMapper<TSource, TDestination>(
		ISimpleMap map)
		where TSource : class
		where TDestination : class, new()
	{
		string key = $"{map.MappingKey}_compiled";

		return (Func<TSource, TDestination, TDestination>)_compiledMappers.GetOrAdd(
			key,
			_ => CreateCompiledMapper<TSource, TDestination>(map));
	}

	/// ======================================================================================================================
	/// <summary>
	/// Creates a compiled mapper using expression trees. This generates IL code that directly accesses
	/// properties without reflection, providing near-manual mapping performance.
	/// </summary>
	/// <typeparam name="TSource">The source type</typeparam>
	/// <typeparam name="TDestination">The destination type</typeparam>
	/// <param name="map">The map configuration containing property information</param>
	/// <returns>A compiled function that maps properties from source to destination</returns>
	/// ======================================================================================================================
	private static Func<TSource, TDestination, TDestination> CreateCompiledMapper<TSource, TDestination>(
		ISimpleMap map)
		where TSource : class
		where TDestination : class, new()
	{
		ParameterExpression sourceParam = Expression.Parameter(typeof(TSource), "source");
		ParameterExpression destParam = Expression.Parameter(typeof(TDestination), "dest");

		List<Expression> expressions = new();

		// 1. Process custom property mappings FIRST (absolute precedence)
		foreach (KeyValuePair<PropertyInfo, object> customMapping in map.CustomPropertyMappings)
		{
			PropertyInfo destProp = customMapping.Key;
			
			if (customMapping.Value is Expression<Func<TSource, object>> sourceExpr)
			{
				Expression sourceExpression = ReplaceParameter(sourceExpr.Body, sourceExpr.Parameters[0], sourceParam);
				
				if (sourceExpression.Type == typeof(object) && sourceExpression.NodeType == ExpressionType.Convert)
				{
					sourceExpression = ((UnaryExpression)sourceExpression).Operand;
				}
				
				if (sourceExpression.Type != destProp.PropertyType)
				{
					sourceExpression = Expression.Convert(sourceExpression, destProp.PropertyType);
				}
				
				MemberExpression destPropertyExpr = Expression.Property(destParam, destProp);
				expressions.Add(Expression.Assign(destPropertyExpr, sourceExpression));
			}
		}

		// 2. Process automatic property mappings (only if no custom mapping exists)
		foreach (PropertyInfo sourceProp in map.ValidProperties)
		{
			if (!map.CollectionProperties.ContainsKey(sourceProp))
			{
				PropertyInfo? sourceProperty = typeof(TSource).GetProperty(sourceProp.Name);
				PropertyInfo? destProp = typeof(TDestination).GetProperty(sourceProp.Name);
				
				if (sourceProperty is not null && destProp is not null && destProp.CanWrite)
				{
					if (map.CustomPropertyMappings.ContainsKey(destProp))
					{
						continue;
					}
					
					MemberExpression sourcePropertyExpr = Expression.Property(sourceParam, sourceProperty);
					MemberExpression destPropertyExpr = Expression.Property(destParam, destProp);
					expressions.Add(Expression.Assign(destPropertyExpr, sourcePropertyExpr));
				}
			}
		}

		expressions.Add(destParam);

		BlockExpression block = Expression.Block(expressions);

		return Expression.Lambda<Func<TSource, TDestination, TDestination>>(
			block, sourceParam, destParam).Compile();
	}

	/// ======================================================================================================================
	/// <summary>
	/// Replaces a parameter in an expression tree with a new parameter
	/// </summary>
	/// <param name="expression">The expression to process</param>
	/// <param name="oldParam">The parameter to replace</param>
	/// <param name="newParam">The new parameter to use</param>
	/// <returns>The expression with replaced parameters</returns>
	/// ======================================================================================================================
	private static Expression ReplaceParameter(
		Expression expression,
		ParameterExpression oldParam,
		ParameterExpression newParam)
	{
		return new ParameterReplacer(oldParam, newParam).Visit(expression);
	}

	/// ======================================================================================================================
	/// <summary>
	/// Clears the compiled mapper cache. Useful for testing or when mappings are dynamically changed.
	/// </summary>
	/// ======================================================================================================================
	internal static void ClearCache()
	{
		_compiledMappers.Clear();
	}

	/// ======================================================================================================================
	/// <summary>
	/// Gets the current count of compiled mappers in the cache.
	/// </summary>
	/// <returns>The number of compiled mappers cached</returns>
	/// ======================================================================================================================
	internal static int GetCacheCount()
	{
		return _compiledMappers.Count;
	}
}
