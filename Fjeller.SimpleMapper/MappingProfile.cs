using Fjeller.SimpleMapper.Maps;
using Fjeller.SimpleMapper.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fjeller.SimpleMapper;

public class MappingProfile
{
	/// ========================================================================================================================================================= 
	/// <summary>
	/// Creates a map between the source and destination types. The destination type must have either a public
	/// parameterless constructor, or exactly one other public constructor (e.g. a positional record); an ambiguous
	/// constructor situation throws a <see cref="Fjeller.SimpleMapper.Exceptions.SimpleMapperException"/> as soon as
	/// the map is prepared. Any <c>required</c> destination member that cannot be resolved from a source property or
	/// a <c>ForMember</c> mapping also throws at that point.
	/// </summary>
	/// <typeparam name="TSource">The source type</typeparam>
	/// <typeparam name="TDestination">The destination type</typeparam>
	/// <returns>A Map-object</returns>
	/// ========================================================================================================================================================= 
	protected ISimpleMap<TSource, TDestination> CreateMap<TSource, TDestination>()
		where TSource : class
		where TDestination : class
	{
		SimpleMap<TSource, TDestination> map = SimpleMap<TSource, TDestination>.Create();
		SimpleMapCache.AddMap( map );
		return map;
	}
}
