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
	/// Dictionary of collection properties with their element types for deep mapping support
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
	/// The internally used method to create the valid properties
	/// </summary>
	/// ======================================================================================================================
	void CreateValidProperties();

	/// ======================================================================================================================
	/// <summary>
	/// Executes the after mapping action. if no action is defined, nothing is executed
	/// </summary>
	/// <param name="source">The source object for the mapping to use in the action</param>
	/// <param name="destination">The destination object for the mapping to use in the action</param>
	/// ======================================================================================================================
	void ExecuteAfterMapAction( object source, object destination );
}
