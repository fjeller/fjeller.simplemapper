using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Fjeller.SimpleMapper.Maps;

public interface ISimpleMap
{
	/// ======================================================================================================================
	/// <summary>
	/// The key for this mapping configuration
	/// </summary>
	/// ======================================================================================================================
	string MappingKey { get; }

	/// ======================================================================================================================
	/// <summary>
	/// The list of valid properties for the mapping
	/// </summary>
	/// ======================================================================================================================
	List<PropertyInfo> ValidProperties { get; set; }

	/// ======================================================================================================================
	/// <summary>
	/// Dictionary of collection properties with their destination element types for deep mapping support.
	/// The destination element type may differ from the source element type as long as a mapping between
	/// the two element types has been registered (or the element types are identical).
	/// </summary>
	/// ======================================================================================================================
	Dictionary<PropertyInfo, Type> CollectionProperties { get; }

	/// ======================================================================================================================
	/// <summary>
	/// Dictionary of custom property mappings (destination property → source expression).
	/// Used by the compilation engine to generate custom mapping code.
	/// </summary>
	/// ======================================================================================================================
	Dictionary<PropertyInfo, object> CustomPropertyMappings { get; }

	/// ======================================================================================================================
	/// <summary>
	/// The internally used method to create the valid properties. This also determines how the destination type
	/// is constructed (parameterless constructor, or a single public constructor with parameters, e.g. for
	/// positional records) and validates that any <c>required</c> destination members can be resolved from the
	/// source type, a custom <c>ForMember</c> mapping. Throws a <see cref="Fjeller.SimpleMapper.Exceptions.SimpleMapperException"/>
	/// if the destination type's constructor is ambiguous, or if a <c>required</c> member cannot be resolved.
	/// </summary>
	/// ======================================================================================================================
	void CreateValidProperties();

	/// ======================================================================================================================
	/// <summary>
	/// Creates a new, empty instance of the destination type using the constructor strategy determined by
	/// <see cref="CreateValidProperties"/> (parameterless constructor when available, otherwise the destination
	/// type's single public constructor invoked with default values for its parameters). All destination properties -
	/// including ones supplied via the constructor - are still fully populated afterwards through the normal
	/// property-mapping pipeline, so constructor-supplied placeholder values are always overwritten with mapped data.
	/// </summary>
	/// <returns>A new, empty instance of the destination type</returns>
	/// ======================================================================================================================
	object CreateDestination();

	/// ======================================================================================================================
	/// <summary>
	/// Executes the after mapping action. if no action is defined, nothing is executed
	/// </summary>
	/// <param name="source">The source object for the mapping to use in the action</param>
	/// <param name="destination">The destination object for the mapping to use in the action</param>
	/// ======================================================================================================================
	void ExecuteAfterMapAction( object source, object destination );
}
