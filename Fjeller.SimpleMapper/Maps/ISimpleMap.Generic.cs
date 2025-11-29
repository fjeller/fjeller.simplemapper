using Fjeller.SimpleMapper.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Fjeller.SimpleMapper.Maps;

public interface ISimpleMap<TSource, out TDestination> : ISimpleMap
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
}
