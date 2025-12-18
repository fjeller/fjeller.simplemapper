using Fjeller.SimpleMapper.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Fjeller.SimpleMapper.Maps;

public interface ISimpleMap<TSource, TDestination> : ISimpleMap
	where TSource : class
	where TDestination : class, new()
{
	/// ======================================================================================================================
	/// <summary>
	/// Method to ignore a specific member. The member needs to be part of the source. This method does not throw an exception
	/// if the member does not exist.
	/// </summary>
	/// <param name="memberName">The name of the member to ignore. If that member doesn't exist, nothing happens.</param>
	/// <returns>The SimpleMap object</returns>
	/// ======================================================================================================================
	ISimpleMap<TSource, TDestination> IgnoreMember( string memberName );

	/// ======================================================================================================================
	/// <summary>
	/// Method to ignore a specific member. The member needs to be part of the source. This method does not throw an exception
	/// if the member does not exist.
	/// </summary>
	/// <param name="sourceMember">The member as a linq expression</param>
	/// <returns>The SimpleMap object</returns>
	/// ======================================================================================================================
	ISimpleMap<TSource, TDestination> IgnoreMember( Expression<Func<TSource, object>> sourceMember );

	/// ======================================================================================================================
	/// <summary>
	/// Method to ignore multiple members. The member names are separated by comma and need to be part of the source.
	/// This method does not throw an exception if the member does not exist.
	/// </summary>
	/// <param name="memberNames">The names of the members</param>
	/// <returns>The SimpleMap object</returns>
	/// ======================================================================================================================
	ISimpleMap<TSource, TDestination> IgnoreMembers( params string[] memberNames );

	/// ======================================================================================================================
	/// <summary>
	/// Method to define an action that should be executed after the mapping.
	/// </summary>
	/// <param name="action">The action to be executed after the mapping, usually as a linq construct</param>
	/// <returns>The SimpleMap object</returns>
	/// ======================================================================================================================
	ISimpleMap<TSource, TDestination> ExecuteAfterMapping( Action<TSource, TDestination> action );

	/// ======================================================================================================================
	/// <summary>
	/// Configures explicit mapping for a destination property from a source expression.
	/// This overrides any automatic name-based mapping for the destination property and automatically excludes
	/// the source property from automatic mapping. Calling ForMember multiple times for the same destination
	/// property will throw an exception.
	/// </summary>
	/// <param name="destinationMember">Expression selecting the destination property to configure</param>
	/// <param name="options">Configuration action where MapFrom should be called to specify the source</param>
	/// <returns>The SimpleMap object for method chaining</returns>
	/// ======================================================================================================================
	ISimpleMap<TSource, TDestination> ForMember(
		Expression<Func<TDestination, object>> destinationMember,
		Action<PropertyMappingOptions<TSource, TDestination>> options );
}
