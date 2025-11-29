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
	/// Maps one object to another. Source and destination types must be provided, as well as the objects. The destination type must have
	/// a parameterless constructor and an object is automatically created if the destination object is null
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

		if ( propertyMap == null )
		{
			string exceptionMessage = $"There is no mapping available between the types {sourceType.FullName} and {destinationType.FullName}";
			throw new ArgumentException( exceptionMessage );
		}

		foreach ( PropertyInfo property in propertyMap.ValidProperties )
		{
			object? value = property.GetValue( source, _BINDINGFLAGS_GETPROPERTY, null, null, null );
			PropertyInfo? destinationProperty = destinationType.GetProperty( property.Name );
			destinationProperty?.SetValue( destination, value, _BINDINGFLAGS_SETPROPERTY, null, null, null );
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

		if ( source == null )
		{
			return null;
		}

		destination ??= new TDestination();

		Type destinationType = typeof( TDestination );
		Type currentSourceType = source.GetCorrectSourceType();
		Type? sourceType = SimpleMapCache.GetMatchingSourceType( destinationType, source );

		if ( sourceType == null )
		{
			throw new SimpleMapperException( $"There is no matching map for the types {currentSourceType} and {destinationType} or one of the implemented interfaces" );
		}

		ISimpleMap? propertyMap = SimpleMapCache.GetMap( sourceType, destinationType );

		if ( propertyMap == null )
		{
			string exceptionMessage = $"There is no mapping available between the types {sourceType.FullName} and {destinationType.FullName}";
			throw new SimpleMapperException( exceptionMessage );
		}

		foreach ( PropertyInfo property in propertyMap.ValidProperties )
		{
			object? value = property.GetValue( source, _BINDINGFLAGS_GETPROPERTY, null, null, null );
			PropertyInfo? destinationProperty = destinationType.GetProperty( property.Name );
			if ( destinationProperty != null && destinationProperty.CanWrite )
			{
				destinationProperty.SetValue( destination, value, _BINDINGFLAGS_SETPROPERTY, null, null, null );
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

}
