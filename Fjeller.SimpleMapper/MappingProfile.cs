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
	/// Creates a map between the source and destination types
	/// </summary>
	/// <typeparam name="TSource">The source type</typeparam>
	/// <typeparam name="TDestination">The destination type</typeparam>
	/// <returns>A Map-object</returns>
	/// ========================================================================================================================================================= 
	protected ISimpleMap<TSource, TDestination> CreateMap<TSource, TDestination>()
		where TSource : class
		where TDestination : class, new()
	{
		SimpleMap<TSource, TDestination> map = SimpleMap<TSource, TDestination>.Create();
		SimpleMapCache.AddMap( map );
		return map;
	}
}
