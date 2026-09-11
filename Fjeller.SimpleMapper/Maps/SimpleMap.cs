using Fjeller.SimpleMapper.Exceptions;
using Fjeller.SimpleMapper.Extensions;
using Fjeller.SimpleMapper.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Fjeller.SimpleMapper.Maps;

internal class SimpleMap<TSource, TDestination> : ISimpleMap<TSource, TDestination>
	where TSource : class
	where TDestination : class
{
	#region Fields

	private Action<TSource, TDestination>? _afterMappingAction;

	private bool _validPropertiesCreated;

	/// <summary>
	/// The constructor to invoke when the destination type has no public parameterless constructor
	/// (e.g. a positional record). Null when the destination type's public parameterless constructor is used.
	/// </summary>
	private ConstructorInfo? _nonDefaultConstructor;

	/// <summary>
	/// Placeholder argument values used to invoke <see cref="_nonDefaultConstructor"/>. These are always
	/// overwritten by the normal property-mapping pipeline immediately after construction.
	/// </summary>
	private object?[]? _nonDefaultConstructorArgs;

	private const BindingFlags _DEFAULT_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private readonly Dictionary<PropertyInfo, Type> _collectionProperties = new();

	private readonly Dictionary<PropertyInfo, Expression<Func<TSource, object>>> _customPropertyMappings = new();

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

	/// ======================================================================================================================
	/// <summary>
	/// Dictionary of collection properties with their element types for deep mapping support
	/// </summary>
	/// ======================================================================================================================
	public Dictionary<PropertyInfo, Type> CollectionProperties => _collectionProperties;

	/// ======================================================================================================================
	/// <summary>
	/// Dictionary of custom property mappings (destination property → source expression).
	/// Used by the compilation engine to generate custom mapping code.
	/// </summary>
	/// ======================================================================================================================
	public Dictionary<PropertyInfo, object> CustomPropertyMappings =>
		_customPropertyMappings.ToDictionary(
			kvp => kvp.Key,
			kvp => (object)kvp.Value
		);

	#endregion

	/// ======================================================================================================================
	/// <summary>
	/// Gets the element type from a collection type (handles arrays, List&lt;T&gt;, IEnumerable&lt;T&gt;, etc.)
	/// </summary>
	/// <param name="collectionType">The collection type to analyze</param>
	/// <returns>The element type if found, otherwise null</returns>
	/// ======================================================================================================================
	private Type? GetCollectionElementType( Type collectionType )
	{
		if ( collectionType.IsArray )
		{
			return collectionType.GetElementType();
		}

		if ( collectionType.IsGenericType )
		{
			Type[] genericArguments = collectionType.GetGenericArguments();
			if ( genericArguments.Length == 1 )
			{
				return genericArguments[0];
			}
		}

		Type? enumerableInterface = collectionType.GetInterfaces()
			.FirstOrDefault( i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof( IEnumerable<> ) );

		if ( enumerableInterface is not null )
		{
			return enumerableInterface.GetGenericArguments()[0];
		}

		return null;
	}

	/// ======================================================================================================================
	/// <summary>
	/// Gets the valid mapping property infos between source and destination types, including collection properties
	/// </summary>
	/// <param name="sourceType">The source type</param>
	/// <param name="destinationType">The destination type</param>
	/// <returns>List of valid properties that can be mapped</returns>
	/// ======================================================================================================================
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
				Type? sourceElementType = GetCollectionElementType( sourcePropertyInfo.PropertyType );
				if ( sourceElementType is null )
				{
					continue;
				}

				// Match by name only - the destination collection kind (List<T>, T[], IEnumerable<T>,
				// ICollection<T>, IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, HashSet<T>, ISet<T>,
				// IReadOnlySet<T>) may differ from the source, as may the element type, as long as a map
				// between the element types exists (or the element types are identical).
				PropertyInfo? destinationPropertyInfo = destinationProperties.FirstOrDefault( p =>
					p.Name == sourcePropertyInfo.Name &&
					p.PropertyType != typeof( string ) &&
					typeof( System.Collections.IEnumerable ).IsAssignableFrom( p.PropertyType ) );

				if ( destinationPropertyInfo is null )
				{
					continue;
				}

				if ( _customPropertyMappings.ContainsKey( destinationPropertyInfo ) )
				{
					continue;
				}

				Type? destinationElementType = GetCollectionElementType( destinationPropertyInfo.PropertyType );
				if ( destinationElementType is null )
				{
					continue;
				}

				bool elementTypesMatch = sourceElementType == destinationElementType;
				bool hasRegisteredElementMap = !elementTypesMatch && SimpleMapCache.GetMap( sourceElementType, destinationElementType ) is not null;

				if ( !elementTypesMatch && !hasRegisteredElementMap )
				{
					// No exact type match and no registered mapping between the element types - skip silently,
					// consistent with how mismatched scalar properties are handled.
					continue;
				}

				_collectionProperties[sourcePropertyInfo] = destinationElementType;
				result.Add( sourcePropertyInfo );
				continue;
			}

			PropertyInfo? destinationProperty = destinationProperties.FirstOrDefault(
				p => p.Name == sourcePropertyInfo.Name && p.PropertyType == sourcePropertyInfo.PropertyType );

			if ( destinationProperty is null )
			{
				continue;
			}

			if ( _customPropertyMappings.ContainsKey( destinationProperty ) )
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
	/// Configures explicit mapping for a destination property from a source expression.
	/// This overrides any automatic name-based mapping for the destination property and automatically excludes
	/// the source property from automatic mapping. Calling ForMember multiple times for the same destination
	/// property will throw an exception.
	/// </summary>
	/// <param name="destinationMember">Expression selecting the destination property to configure</param>
	/// <param name="options">Configuration action where MapFrom should be called to specify the source</param>
	/// <returns>The SimpleMap object for method chaining</returns>
	/// ======================================================================================================================
	public ISimpleMap<TSource, TDestination> ForMember(
		Expression<Func<TDestination, object>> destinationMember,
		Action<PropertyMappingOptions<TSource, TDestination>> options )
	{
		PropertyInfo? destProperty = destinationMember.FindProperty();
		if ( destProperty is null )
		{
			throw new ArgumentException( "Could not extract property information from the destination expression", nameof( destinationMember ) );
		}

		if ( _customPropertyMappings.ContainsKey( destProperty ) )
		{
			throw new InvalidOperationException(
				$"A custom mapping for destination property '{destProperty.Name}' has already been configured. " +
				$"Multiple ForMember calls for the same destination property are not allowed." );
		}

		PropertyMappingOptions<TSource, TDestination> mappingOptions = new();
		options( mappingOptions );

		if ( mappingOptions.SourceExpression is null )
		{
			throw new InvalidOperationException(
				$"MapFrom must be called within the options action for destination property '{destProperty.Name}'. " +
				$"Example: .ForMember(dest => dest.{destProperty.Name}, opt => opt.MapFrom(src => src.SourceProperty))" );
		}

		_customPropertyMappings[destProperty] = mappingOptions.SourceExpression;

		try
		{
			PropertyInfo? sourceProperty = mappingOptions.SourceExpression.FindProperty();
			if ( sourceProperty is not null )
			{
				IgnoredSourceProperties.AddIfNotContains( sourceProperty );
			}
		}
		catch
		{
		}

		return this;
	}

	/// ======================================================================================================================
	/// <summary>
	/// Created the valid properties. Also determines the destination type's construction strategy and validates
	/// that any <c>required</c> destination members can be resolved.
	/// </summary>
	/// ======================================================================================================================
	void ISimpleMap.CreateValidProperties()
	{
		if ( !this._validPropertiesCreated )
		{
			this._validPropertiesCreated = true;
			this.ValidProperties = GetValidMappingPropertyInfos( typeof( TSource ), typeof( TDestination ) );
			DetermineConstructionStrategy();
			ValidateRequiredMembers();
		}
	}

	/// ======================================================================================================================
	/// <summary>
	/// Determines how instances of the destination type should be constructed: via the public parameterless
	/// constructor if one exists (unchanged, existing behavior), or - to support positional records and similar
	/// types - via the destination type's single public constructor, invoked with placeholder default values for
	/// each parameter. Those placeholder values are always overwritten by the regular property-mapping pipeline
	/// immediately after construction. Throws when the destination type has neither a parameterless constructor
	/// nor exactly one other public constructor, since guessing which constructor to use would be unsafe.
	/// </summary>
	/// ======================================================================================================================
	private void DetermineConstructionStrategy()
	{
		Type destinationType = typeof( TDestination );

		ConstructorInfo[] publicConstructors = destinationType.GetConstructors( BindingFlags.Public | BindingFlags.Instance );

		ConstructorInfo? parameterlessConstructor = Array.Find( publicConstructors, c => c.GetParameters().Length == 0 );
		if ( parameterlessConstructor is not null )
		{
			// Existing, unchanged behavior: parameterless construction via `new TDestination()`.
			_nonDefaultConstructor = null;
			_nonDefaultConstructorArgs = null;
			return;
		}

		if ( publicConstructors.Length != 1 )
		{
			throw new SimpleMapperException(
				$"Cannot determine how to construct destination type '{destinationType.FullName}': it has no public parameterless " +
				$"constructor and {publicConstructors.Length} other public constructors. SimpleMapper can only construct destination " +
				"types that either have a public parameterless constructor, or exactly one other public constructor (e.g. a positional record)." );
		}

		ConstructorInfo constructor = publicConstructors[0];
		ParameterInfo[] parameters = constructor.GetParameters();
		object?[] args = new object?[parameters.Length];

		for ( int i = 0; i < parameters.Length; i++ )
		{
			Type parameterType = parameters[i].ParameterType;
			args[i] = parameterType.IsValueType ? Activator.CreateInstance( parameterType ) : null;
		}

		_nonDefaultConstructor = constructor;
		_nonDefaultConstructorArgs = args;
	}

	/// ======================================================================================================================
	/// <summary>
	/// Validates that every destination property marked with the <c>required</c> modifier can be resolved either
	/// from a matching source property (already reflected in <see cref="ValidProperties"/>) or from a custom
	/// <c>ForMember</c> mapping. Throws a <see cref="SimpleMapperException"/> immediately (at registration/preparation
	/// time) for any <c>required</c> member that cannot be resolved, rather than silently leaving it unset.
	/// </summary>
	/// ======================================================================================================================
	private void ValidateRequiredMembers()
	{
		PropertyInfo[] destinationProperties = typeof( TDestination ).GetProperties( _DEFAULT_FLAGS );

		foreach ( PropertyInfo destinationProperty in destinationProperties )
		{
			bool isRequired = destinationProperty.GetCustomAttributesData()
				.Any( a => a.AttributeType.FullName == "System.Runtime.CompilerServices.RequiredMemberAttribute" );

			if ( !isRequired )
			{
				continue;
			}

			if ( _customPropertyMappings.ContainsKey( destinationProperty ) )
			{
				continue;
			}

			bool isResolvedFromSource = ValidProperties.Any( sourceProperty => sourceProperty.Name == destinationProperty.Name );
			if ( isResolvedFromSource )
			{
				continue;
			}

			throw new SimpleMapperException(
				$"The required member '{destinationProperty.Name}' on destination type '{typeof( TDestination ).FullName}' could not be " +
				$"resolved. Add a source property named '{destinationProperty.Name}' on '{typeof( TSource ).FullName}', or configure " +
				$".ForMember(dest => dest.{destinationProperty.Name}, opt => opt.MapFrom(src => src.SourceProperty))." );
		}
	}

	/// ======================================================================================================================
	/// <summary>
	/// Creates a new, empty instance of the destination type using the construction strategy determined by
	/// <see cref="DetermineConstructionStrategy"/>.
	/// </summary>
	/// <returns>A new, empty instance of the destination type</returns>
	/// ======================================================================================================================
	object ISimpleMap.CreateDestination()
	{
		if ( _nonDefaultConstructor is null )
		{
			return Activator.CreateInstance( typeof( TDestination ) )!;
		}

		return _nonDefaultConstructor.Invoke( _nonDefaultConstructorArgs )!;
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
		if ( this._afterMappingAction is null )
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
