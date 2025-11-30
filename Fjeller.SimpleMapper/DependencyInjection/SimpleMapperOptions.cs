using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fjeller.SimpleMapper.DependencyInjection;

/// ======================================================================================================================
/// <summary>
/// Configuration options for SimpleMapper dependency injection
/// </summary>
/// ======================================================================================================================
public class SimpleMapperOptions
{
	/// ======================================================================================================================
	/// <summary>
	/// Gets the list of registered mapping profiles
	/// </summary>
	/// ======================================================================================================================
	internal List<MappingProfile> Profiles { get; } = new();

	/// ======================================================================================================================
	/// <summary>
	/// Adds mapping profiles to the configuration
	/// </summary>
	/// <param name="profiles">Mapping profiles to add</param>
	/// <returns>The SimpleMapperOptions for method chaining</returns>
	/// ======================================================================================================================
	public SimpleMapperOptions AddProfiles( params MappingProfile[] profiles )
	{
		Profiles.AddRange( profiles );
		return this;
	}

	/// ======================================================================================================================
	/// <summary>
	/// Adds mapping profiles from the specified assemblies by scanning for types that inherit from MappingProfile
	/// </summary>
	/// <param name="assemblies">Assemblies to scan for MappingProfile types</param>
	/// <returns>The SimpleMapperOptions for method chaining</returns>
	/// ======================================================================================================================
	public SimpleMapperOptions AddProfiles( params Assembly[] assemblies )
	{
		foreach ( Assembly assembly in assemblies )
		{
			IEnumerable<Type> profileTypes = assembly.GetTypes()
				.Where( t => t.IsClass
					&& !t.IsAbstract
					&& t.IsSubclassOf( typeof( MappingProfile ) )
					&& t.GetConstructor( Type.EmptyTypes ) is not null );

			foreach ( Type profileType in profileTypes )
			{
				MappingProfile? profile = Activator.CreateInstance( profileType ) as MappingProfile;
				if ( profile is not null )
				{
					Profiles.Add( profile );
				}
			}
		}

		return this;
	}

	/// ======================================================================================================================
	/// <summary>
	/// Adds mapping profiles from assemblies that contain the specified types
	/// </summary>
	/// <param name="markerTypes">Types whose assemblies will be scanned for MappingProfile types</param>
	/// <returns>The SimpleMapperOptions for method chaining</returns>
	/// ======================================================================================================================
	public SimpleMapperOptions AddProfilesFromAssembliesContaining( params Type[] markerTypes )
	{
		Assembly[] assemblies = markerTypes
			.Select( t => t.Assembly )
			.Distinct()
			.ToArray();

		return AddProfiles( assemblies );
	}

	/// ======================================================================================================================
	/// <summary>
	/// Adds mapping profiles from the assembly that contains the specified type
	/// </summary>
	/// <typeparam name="TMarker">Type whose assembly will be scanned for MappingProfile types</typeparam>
	/// <returns>The SimpleMapperOptions for method chaining</returns>
	/// ======================================================================================================================
	public SimpleMapperOptions AddProfilesFromAssemblyContaining<TMarker>()
	{
		return AddProfiles( typeof( TMarker ).Assembly );
	}
}
