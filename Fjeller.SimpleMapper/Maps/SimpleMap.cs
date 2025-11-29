using Fjeller.SimpleMapper.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Fjeller.SimpleMapper.Maps;

internal class SimpleMap<TSource, TDestination> : ISimpleMap<TSource, TDestination>
	where TSource : class
	where TDestination : class, new()
{
	#region Fields

	private Action<TSource, TDestination>? _afterMappingAction;

	private bool _validPropertiesCreated;

	private const BindingFlags _DEFAULT_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	#endregion

	#region Properties

	/// ======================================================================================================================
	/// <summary>
	/// The ignored source properties
	/// </summary>
	/// ======================================================================================================================
	private List<PropertyInfo> IgnoredSourceProperties { get; set; }

	/// ======================================================================================================================
	/// <summary>
	/// The valid properties
	/// </summary>
	/// ======================================================================================================================
	public List<PropertyInfo> ValidProperties { get; set; }

	/// ======================================================================================================================
	/// <summary>
	/// The mapping key
	/// </summary>
	/// ======================================================================================================================
	public string MappingKey { get; private set; } = null!;

	#endregion

	private List<PropertyInfo> GetValidMappingPropertyInfos( Type sourceType, Type destinationType )
	{
		PropertyInfo[] sourcePropertyInfos = sourceType.GetProperties( _DEFAULT_FLAGS );
		PropertyInfo[] destinationProperties = destinationType.GetProperties( _DEFAULT_FLAGS );

		if ( IgnoredSourceProperties.Any() )
		{
			sourcePropertyInfos = sourcePropertyInfos.Except( IgnoredSourceProperties ).ToArray();
		}

		List<PropertyInfo> result = [];

		foreach ( PropertyInfo sourcePropertyInfo in sourcePropertyInfos )
		{
			if ( sourcePropertyInfo.PropertyType != typeof( string ) && typeof( System.Collections.IEnumerable ).IsAssignableFrom( sourcePropertyInfo.PropertyType ) )
			{
				continue;
			}

			PropertyInfo? destinationPropertyInfo = destinationProperties.FirstOrDefault( p => p.Name == sourcePropertyInfo.Name && p.PropertyType == sourcePropertyInfo.PropertyType );
			if ( destinationPropertyInfo == null )
			{
				continue;
			}
			result.Add( sourcePropertyInfo );
		}

		return result;
	}

	/// ======================================================================================================================
	/// <summary>
	/// Creates a map for the mapping between two objects
	/// </summary>
	/// <returns>The Map object</returns>
	/// ======================================================================================================================
	public static SimpleMap<TSource, TDestination> Create()
	{
		SimpleMap<TSource, TDestination> result = new();

		string sourceTypeName = typeof( TSource ).FullName!;
		string destinationTypeName = typeof( TDestination ).FullName!;

		result.MappingKey = $"{sourceTypeName}_{destinationTypeName}";

		return result;
	}

	/// ======================================================================================================================
	/// <summary>
	/// Method to ignore a specific member. The member needs to be part of the source. This method does not throw an exception
	/// if the member does not exist.
	/// </summary>
	/// <param name="memberName">The name of the member to ignore. If that member doesn't exist, nothing happens.</param>
	/// <returns>The SimpleMap object</returns>
	/// ======================================================================================================================
	public ISimpleMap<TSource, TDestination> IgnoreMember( string memberName )
	{
		PropertyInfo? propertyInfo = typeof( TSource ).GetPropertyInfo( memberName );
		IgnoredSourceProperties.AddIfNotContains( propertyInfo );
		return this;
	}

	/// ======================================================================================================================
	/// <summary>
	/// Method to ignore a specific member. The member needs to be part of the source. This method does not throw an exception
	/// if the member does not exist.
	/// </summary>
	/// <param name="sourceMember">The member as a linq expression</param>
	/// <returns>The SimpleMap object</returns>
	/// ======================================================================================================================
	public ISimpleMap<TSource, TDestination> IgnoreMember( Expression<Func<TSource, object>> sourceMember )
	{
		PropertyInfo? propertyInfo = sourceMember.FindProperty();
		IgnoredSourceProperties.AddIfNotContains( propertyInfo );

		return this;
	}

	/// ======================================================================================================================
	/// <summary>
	/// Method to ignore multiple members. The member names are separated by comma and need to be part of the source.
	/// This method does not throw an exception if the member does not exist.
	/// </summary>
	/// <param name="memberNames">The names of the members</param>
	/// <returns>The SimpleMap object</returns>
	/// ======================================================================================================================
	public ISimpleMap<TSource, TDestination> IgnoreMembers( params string[] memberNames )
	{
		IEnumerable<PropertyInfo> propertyInfos = typeof( TSource ).GetPropertyInfos( memberNames );
		propertyInfos.ToList().ForEach( this.IgnoredSourceProperties.Add );
		return this;
	}

	/// ======================================================================================================================
	/// <summary>
	/// Method to define an action that should be executed after the mapping.
	/// </summary>
	/// <param name="action">The action to be executed after the mapping, usually as a linq construct</param>
	/// <returns>The SimpleMap object</returns>
	/// ======================================================================================================================
	public ISimpleMap<TSource, TDestination> ExecuteAfterMapping( Action<TSource, TDestination> action )
	{
		this._afterMappingAction = action;
		return this;
	}

	/// ======================================================================================================================
	/// <summary>
	/// Created the valid properties
	/// </summary>
	/// ======================================================================================================================
	void ISimpleMap.CreateValidProperties()
	{
		if ( !this._validPropertiesCreated )
		{
			this._validPropertiesCreated = true;
			this.ValidProperties = GetValidMappingPropertyInfos( typeof( TSource ), typeof( TDestination ) );
		}
	}

	/// ======================================================================================================================
	/// <summary>
	/// Method to execute the action that should be executed after the mapping
	/// </summary>
	/// <param name="source">The source object for the mapping</param>
	/// <param name="destination">The destination object for the mapping</param>
	/// ======================================================================================================================
	void ISimpleMap.ExecuteAfterMapAction( object source, object destination )
	{
		if ( this._afterMappingAction == null )
		{
			return;
		}

		if ( source is not TSource sourceObject || destination is not TDestination destinationObject )
		{
			return;
		}

		this._afterMappingAction( sourceObject, destinationObject );
	}

	/// ======================================================================================================================
	/// <summary>
	/// The private constructor
	/// </summary>
	/// ======================================================================================================================
	private SimpleMap()
	{
		this.IgnoredSourceProperties = new List<PropertyInfo>();
		this.ValidProperties = new List<PropertyInfo>();
	}
}
