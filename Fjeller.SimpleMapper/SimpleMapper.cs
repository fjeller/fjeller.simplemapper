using Fjeller.SimpleMapper.Compilation;
using Fjeller.SimpleMapper.Exceptions;
using Fjeller.SimpleMapper.Extensions;
using Fjeller.SimpleMapper.Maps;
using Fjeller.SimpleMapper.Storage;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Fjeller.SimpleMapper;

public class SimpleMapper : ISimpleMapper
{
	#region constants

	private const BindingFlags _BINDINGFLAGS_GETPROPERTY = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty;
	private const BindingFlags _BINDINGFLAGS_SETPROPERTY = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty;

	#endregion

	#region fields

	private static readonly List<ISimpleMap> _maps = [];

	#endregion

	private void Prepare()
	{
		SimpleMapCache.Prepare();
	}

	/// ========================================================================================================================================================= 
	/// <summary>
	/// Maps a collection property with deep mapping support for complex element types
	/// </summary>
	/// <param name="source">The source object containing the collection</param>
	/// <param name="destination">The destination object where the collection will be mapped</param>
	/// <param name="sourceProperty">The source property info for the collection</param>
	/// <param name="destinationType">The destination type</param>
	/// <param name="elementType">The element type of the collection</param>
	/// ========================================================================================================================================================= 
	private void MapCollectionProperty(
		object source,
		object destination,
		PropertyInfo sourceProperty,
		Type destinationType,
		Type elementType )
	{
		object? sourceCollection = sourceProperty.GetValue( source, _BINDINGFLAGS_GETPROPERTY, null, null, null );

		if ( sourceCollection is null )
		{
			return;
		}

		PropertyInfo? destinationProperty = destinationType.GetProperty( sourceProperty.Name );
		if ( destinationProperty is null || !destinationProperty.CanWrite )
		{
			return;
		}

		Type destinationPropertyType = destinationProperty.PropertyType;

		if ( destinationPropertyType.IsGenericType &&
		     destinationPropertyType.GetGenericTypeDefinition() == typeof( List<> ) )
		{
			MapToList( sourceCollection, destination, destinationProperty, elementType );
		}
		else if ( destinationPropertyType.IsArray )
		{
			MapToArray( sourceCollection, destination, destinationProperty, elementType );
		}
		else if ( typeof( System.Collections.IEnumerable ).IsAssignableFrom( destinationPropertyType ) )
		{
			MapToList( sourceCollection, destination, destinationProperty, elementType );
		}
	}

	/// ========================================================================================================================================================= 
	/// <summary>
	/// Maps a collection to a List&lt;T&gt; with deep mapping support
	/// </summary>
	/// <param name="sourceCollection">The source collection</param>
	/// <param name="destination">The destination object</param>
	/// <param name="destinationProperty">The destination property info</param>
	/// <param name="elementType">The element type</param>
	/// ========================================================================================================================================================= 
	private void MapToList(
		object sourceCollection,
		object destination,
		PropertyInfo destinationProperty,
		Type elementType )
	{
		Type listType = typeof( List<> ).MakeGenericType( elementType );
		System.Collections.IList? list = Activator.CreateInstance( listType ) as System.Collections.IList;

		if ( list is null )
		{
			return;
		}

		bool isComplexType = IsComplexType( elementType );

		foreach ( object? item in (System.Collections.IEnumerable)sourceCollection )
		{
			if ( item is null )
			{
				continue;
			}

			if ( isComplexType )
			{
				ISimpleMap? itemMap = SimpleMapCache.GetMap( item.GetType(), elementType );
				if ( itemMap is not null )
				{
					object? mappedItem = Activator.CreateInstance( elementType );
					if ( mappedItem is not null )
					{
						MapObject( item, mappedItem, item.GetType(), elementType );
						list.Add( mappedItem );
					}
				}
				else
				{
					list.Add( item );
				}
			}
			else
			{
				list.Add( item );
			}
		}

		destinationProperty.SetValue( destination, list, _BINDINGFLAGS_SETPROPERTY, null, null, null );
	}

	/// ========================================================================================================================================================= 
	/// <summary>
	/// Maps a collection to an array with deep mapping support
	/// </summary>
	/// <param name="sourceCollection">The source collection</param>
	/// <param name="destination">The destination object</param>
	/// <param name="destinationProperty">The destination property info</param>
	/// <param name="elementType">The element type</param>
	/// ========================================================================================================================================================= 
	private void MapToArray(
		object sourceCollection,
		object destination,
		PropertyInfo destinationProperty,
		Type elementType )
	{
		List<object> items = new();
		bool isComplexType = IsComplexType( elementType );

		foreach ( object? item in (System.Collections.IEnumerable)sourceCollection )
		{
			if ( item is null )
			{
				continue;
			}

			if ( isComplexType )
			{
				ISimpleMap? itemMap = SimpleMapCache.GetMap( item.GetType(), elementType );
				if ( itemMap is not null )
				{
					object? mappedItem = Activator.CreateInstance( elementType );
					if ( mappedItem is not null )
					{
						MapObject( item, mappedItem, item.GetType(), elementType );
						items.Add( mappedItem );
					}
				}
				else
				{
					items.Add( item );
				}
			}
			else
			{
				items.Add( item );
			}
		}

		Array array = Array.CreateInstance( elementType, items.Count );
		for ( int i = 0; i < items.Count; i++ )
		{
			array.SetValue( items[i], i );
		}

		destinationProperty.SetValue( destination, array, _BINDINGFLAGS_SETPROPERTY, null, null, null );
	}

	/// ========================================================================================================================================================= 
	/// <summary>
	/// Determines if a type is complex and requires deep mapping (not a primitive or string)
	/// </summary>
	/// <param name="type">The type to check</param>
	/// <returns>True if the type is complex, false otherwise</returns>
	/// ========================================================================================================================================================= 
	private bool IsComplexType( Type type )
	{
		return !type.IsPrimitive && type != typeof( string ) && type != typeof( decimal ) && type != typeof( DateTime ) && type != typeof( Guid );
	}

	/// ========================================================================================================================================================= 
	/// <summary>
	/// Maps properties from source to destination object using cached mapping configuration
	/// </summary>
	/// <param name="source">The source object</param>
	/// <param name="destination">The destination object</param>
	/// <param name="sourceType">The source type</param>
	/// <param name="destinationType">The destination type</param>
	/// ========================================================================================================================================================= 
	private void MapObject( object source, object destination, Type sourceType, Type destinationType )
	{
		ISimpleMap? propertyMap = SimpleMapCache.GetMap( sourceType, destinationType );

		if ( propertyMap is null )
		{
			return;
		}

		foreach ( PropertyInfo property in propertyMap.ValidProperties )
		{
			if ( propertyMap.CollectionProperties.ContainsKey( property ) )
			{
				MapCollectionProperty( source, destination, property, destinationType, propertyMap.CollectionProperties[property] );
			}
			else
			{
				object? value = property.GetValue( source, _BINDINGFLAGS_GETPROPERTY, null, null, null );
				PropertyInfo? destinationProperty = destinationType.GetProperty( property.Name );
				destinationProperty?.SetValue( destination, value, _BINDINGFLAGS_SETPROPERTY, null, null, null );
			}
		}

		propertyMap.ExecuteAfterMapAction( source, destination );
	}

	/// ========================================================================================================================================================= 
	/// <summary>
	/// Maps one object to another. Source and destination types must be provided, as well as the objects. The destination type must have
	/// a parameterless constructor and an object is automatically created if the destination object is null.
	/// Uses compiled expression trees for non-collection properties for optimal performance.
	/// </summary>
	/// <typeparam name="TSource">The source type</typeparam>
	/// <typeparam name="TDestination">The destination type</typeparam>
	/// <param name="source">The source object</param>
	/// <param name="destination">The destination object</param>
	/// <returns>The destination object filled with the data from the source object</returns>
	/// ========================================================================================================================================================= 
	public TDestination Map<TSource, TDestination>( TSource source, TDestination? destination )
		where TSource : class
		where TDestination : class, new()
	{
		Prepare();

		destination ??= new TDestination();

		Type sourceType = typeof( TSource );
		Type destinationType = typeof( TDestination );

		ISimpleMap? propertyMap = SimpleMapCache.GetMap( sourceType, destinationType );

		if ( propertyMap is null )
		{
			string exceptionMessage = $"There is no mapping available between the types {sourceType.FullName} and {destinationType.FullName}";
			throw new ArgumentException( exceptionMessage );
		}

		Func<TSource, TDestination, TDestination> compiledMapper = CompiledMapCache.GetOrCreateMapper<TSource, TDestination>( propertyMap );
		destination = compiledMapper( source, destination );

		foreach ( PropertyInfo property in propertyMap.CollectionProperties.Keys )
		{
			MapCollectionProperty( source, destination, property, destinationType, propertyMap.CollectionProperties[property] );
		}

		propertyMap.ExecuteAfterMapAction( source, destination );

		return destination;
	}

	/// ========================================================================================================================================================= 
	/// <summary>
	/// Maps an object of the source type to a new object of the destination type
	/// </summary>
	/// <typeparam name="TSource">The source type</typeparam>
	/// <typeparam name="TDestination">The destination type</typeparam>
	/// <param name="source">The source object</param>
	/// <returns>A new object of the provided destination type with the mapped data</returns>
	/// ========================================================================================================================================================= 
	public TDestination Map<TSource, TDestination>( TSource source )
		where TSource : class
		where TDestination : class, new()
	{
		Prepare();

		TDestination result = new();

		return Map( source, result );
	}

	/// ========================================================================================================================================================= 
	/// <summary>
	/// Maps an object to an object of the destination type. The type of the source object is automatically obtained.
	/// In this case the destination object is known, which menas that the properties of the existing destination
	/// object are filled with the values of the properties of the source object. If no map between the types exists 
	/// an exception is thrown. If the source object is null, the method returns null.
	/// </summary>
	/// <typeparam name="TDestination">The destination type</typeparam>
	/// <param name="source">The source object</param>
	/// <param name="destination">The destination object</param>
	/// <returns>The destination object filled with the data from the source object</returns>
	/// ========================================================================================================================================================= 
	public TDestination? Map<TDestination>( object? source, TDestination? destination )
		where TDestination : class, new()
	{
		Prepare();

		if ( source is null )
		{
			return null;
		}

		destination ??= new TDestination();

		Type destinationType = typeof( TDestination );
		Type currentSourceType = source.GetCorrectSourceType();
		Type? sourceType = SimpleMapCache.GetMatchingSourceType( destinationType, source );

		if ( sourceType is null )
		{
			throw new SimpleMapperException( $"There is no matching map for the types {currentSourceType} and {destinationType} or one of the implemented interfaces" );
		}

		ISimpleMap? propertyMap = SimpleMapCache.GetMap( sourceType, destinationType );

		if ( propertyMap is null )
		{
			string exceptionMessage = $"There is no mapping available between the types {sourceType.FullName} and {destinationType.FullName}";
			throw new SimpleMapperException( exceptionMessage );
		}

		foreach ( PropertyInfo property in propertyMap.ValidProperties )
		{
			if ( propertyMap.CollectionProperties.ContainsKey( property ) )
			{
				MapCollectionProperty( source, destination, property, destinationType, propertyMap.CollectionProperties[property] );
			}
			else
			{
				object? value = property.GetValue( source, _BINDINGFLAGS_GETPROPERTY, null, null, null );
				PropertyInfo? destinationProperty = destinationType.GetProperty( property.Name );
				if ( destinationProperty is not null && destinationProperty.CanWrite )
				{
					destinationProperty.SetValue( destination, value, _BINDINGFLAGS_SETPROPERTY, null, null, null );
				}
			}
		}

		propertyMap.ExecuteAfterMapAction( source, destination );

		return destination;
	}

	/// ========================================================================================================================================================= 
	/// <summary>
	/// Maps an IEnumerable of source objects to an IEnumerable of destination objects
	/// </summary>
	/// <typeparam name="TSource">The source type</typeparam>
	/// <typeparam name="TDestination">The destination type</typeparam>
	/// <param name="source">the source object</param>
	/// <returns>An IEnumerable of destination objects with the mapped data of the source objects</returns>
	/// ========================================================================================================================================================= 
	public IEnumerable<TDestination> Map<TSource, TDestination>( IEnumerable<TSource> source )
		where TSource : class
		where TDestination : class, new()
	{
		Prepare();

		return source.Select( Map<TDestination> ).WhereNotNull();
	}

	/// ========================================================================================================================================================= 
	/// <summary>
	/// Maps an object to a new object of the destination type. If the source object is null, the method returns null.
	/// </summary>
	/// <typeparam name="TDestination">The destination type</typeparam>
	/// <param name="source">The source object</param>
	/// <returns>A new object of the destination type with the data of the source type</returns>
	/// ========================================================================================================================================================= 
	public TDestination? Map<TDestination>( object? source )
		where TDestination : class, new()
	{
		if ( source is null )
		{
			return null;
		}

		Prepare();

		TDestination destination = new();
		TDestination? result = Map( source, destination );
		return result;
	}

	/// ========================================================================================================================================================= 
	/// <summary>
	/// Maps an IEnumerable of objects with runtime type discovery to an IEnumerable of destination objects.
	/// Each source object's type is determined at runtime and mapped to the destination type using the appropriate
	/// registered mapping profile. Null source elements are automatically filtered out from the result.
	/// </summary>
	/// <typeparam name="TDestination">The destination type</typeparam>
	/// <param name="source">The source collection containing objects of potentially different types</param>
	/// <returns>An IEnumerable of destination objects with the mapped data of the source objects, excluding null elements</returns>
	/// ========================================================================================================================================================= 
	public IEnumerable<TDestination> Map<TDestination>(IEnumerable<object?> source)
		where TDestination : class, new()
	{
		Prepare();

		return source.Select(Map<TDestination>).WhereNotNull();
	}

}
