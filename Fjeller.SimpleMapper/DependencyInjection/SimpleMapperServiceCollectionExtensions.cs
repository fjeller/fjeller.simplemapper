using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;
using System.Reflection;

namespace Fjeller.SimpleMapper.DependencyInjection;

/// ======================================================================================================================
/// <summary>
/// Extension methods for configuring SimpleMapper in dependency injection containers
/// </summary>
/// ======================================================================================================================
public static class SimpleMapperServiceCollectionExtensions
{
	/// ======================================================================================================================
	/// <summary>
	/// Adds SimpleMapper services to the specified IServiceCollection with the provided mapping profiles
	/// </summary>
	/// <param name="services">The IServiceCollection to add services to</param>
	/// <param name="profiles">Mapping profiles to register</param>
	/// <returns>The IServiceCollection for method chaining</returns>
	/// <example>
	/// <code>
	/// builder.Services.AddSimpleMapper(
	///     new UserMappingProfile(),
	///     new ProductMappingProfile()
	/// );
	/// </code>
	/// </example>
	/// ======================================================================================================================
	public static IServiceCollection AddSimpleMapper(
		this IServiceCollection services,
		params MappingProfile[] profiles )
	{
		return services.AddSimpleMapper( options =>
		{
			options.AddProfiles( profiles );
		} );
	}

	/// ======================================================================================================================
	/// <summary>
	/// Adds SimpleMapper services to the specified IServiceCollection with configuration
	/// </summary>
	/// <param name="services">The IServiceCollection to add services to</param>
	/// <param name="configureOptions">Action to configure SimpleMapper options</param>
	/// <returns>The IServiceCollection for method chaining</returns>
	/// <example>
	/// <code>
	/// builder.Services.AddSimpleMapper(options =>
	/// {
	///     options.AddProfiles(typeof(Program).Assembly);
	/// });
	/// </code>
	/// </example>
	/// ======================================================================================================================
	public static IServiceCollection AddSimpleMapper(
		this IServiceCollection services,
		Action<SimpleMapperOptions>? configureOptions )
	{
		SimpleMapperOptions options = new();
		configureOptions?.Invoke( options );

		services.TryAddSingleton<ISimpleMapper>( serviceProvider =>
		{
			SimpleMapper mapper = new();

			foreach ( MappingProfile profile in options.Profiles )
			{
				profile.GetType()
					.GetConstructor( Type.EmptyTypes )?
					.Invoke( null );
			}

			return mapper;
		} );

		return services;
	}

	/// ======================================================================================================================
	/// <summary>
	/// Adds SimpleMapper services by scanning the specified assemblies for mapping profiles
	/// </summary>
	/// <param name="services">The IServiceCollection to add services to</param>
	/// <param name="assemblies">Assemblies to scan for MappingProfile types</param>
	/// <returns>The IServiceCollection for method chaining</returns>
	/// <example>
	/// <code>
	/// builder.Services.AddSimpleMapper(
	///     typeof(Program).Assembly,
	///     typeof(DataLayer).Assembly
	/// );
	/// </code>
	/// </example>
	/// ======================================================================================================================
	public static IServiceCollection AddSimpleMapper(
		this IServiceCollection services,
		params Assembly[] assemblies )
	{
		return services.AddSimpleMapper( options =>
		{
			options.AddProfiles( assemblies );
		} );
	}

	/// ======================================================================================================================
	/// <summary>
	/// Adds SimpleMapper services by scanning assemblies that contain the specified types for mapping profiles
	/// </summary>
	/// <param name="services">The IServiceCollection to add services to</param>
	/// <param name="markerTypes">Types whose assemblies will be scanned for MappingProfile types</param>
	/// <returns>The IServiceCollection for method chaining</returns>
	/// <example>
	/// <code>
	/// builder.Services.AddSimpleMapperFromAssemblyContaining(
	///     typeof(Program),
	///     typeof(DataLayer)
	/// );
	/// </code>
	/// </example>
	/// ======================================================================================================================
	public static IServiceCollection AddSimpleMapperFromAssemblyContaining(
		this IServiceCollection services,
		params Type[] markerTypes )
	{
		Assembly[] assemblies = markerTypes
			.Select( t => t.Assembly )
			.Distinct()
			.ToArray();

		return services.AddSimpleMapper( assemblies );
	}

	/// ======================================================================================================================
	/// <summary>
	/// Adds SimpleMapper services by scanning the assembly that contains the specified type for mapping profiles
	/// </summary>
	/// <typeparam name="TMarker">Type whose assembly will be scanned for MappingProfile types</typeparam>
	/// <param name="services">The IServiceCollection to add services to</param>
	/// <returns>The IServiceCollection for method chaining</returns>
	/// <example>
	/// <code>
	/// builder.Services.AddSimpleMapperFromAssemblyContaining&lt;Program&gt;();
	/// </code>
	/// </example>
	/// ======================================================================================================================
	public static IServiceCollection AddSimpleMapperFromAssemblyContaining<TMarker>(
		this IServiceCollection services )
	{
		return services.AddSimpleMapper( typeof( TMarker ).Assembly );
	}

	/// ======================================================================================================================
	/// <summary>
	/// Adds SimpleMapper services by scanning the calling assembly for mapping profiles
	/// </summary>
	/// <param name="services">The IServiceCollection to add services to</param>
	/// <returns>The IServiceCollection for method chaining</returns>
	/// <example>
	/// <code>
	/// builder.Services.AddSimpleMapperFromCallingAssembly();
	/// </code>
	/// </example>
	/// ======================================================================================================================
	public static IServiceCollection AddSimpleMapperFromCallingAssembly(
		this IServiceCollection services )
	{
		Assembly callingAssembly = Assembly.GetCallingAssembly();
		return services.AddSimpleMapper( callingAssembly );
	}
}
