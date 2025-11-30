namespace Fjeller.SimpleMapper
{
	/// ======================================================================================================================
	/// <summary>
	/// Interface for the SimpleMapper providing object-to-object mapping functionality
	/// </summary>
	/// ======================================================================================================================
	public interface ISimpleMapper
	{
		/// ======================================================================================================================
		/// <summary>
		/// Maps one object to another. Source and destination types must be provided, as well as the objects. The destination type must have
		/// a parameterless constructor and an object is automatically created if the destination object is null
		/// </summary>
		/// <typeparam name="TSource">The source type</typeparam>
		/// <typeparam name="TDestination">The destination type</typeparam>
		/// <param name="source">The source object</param>
		/// <param name="destination">The destination object</param>
		/// <returns>The destination object filled with the data from the source object</returns>
		/// ======================================================================================================================
		TDestination Map<TSource, TDestination>(TSource source, TDestination? destination)
			where TSource : class
			where TDestination : class, new();

		/// ======================================================================================================================
		/// <summary>
		/// Maps an object of the source type to a new object of the destination type
		/// </summary>
		/// <typeparam name="TSource">The source type</typeparam>
		/// <typeparam name="TDestination">The destination type</typeparam>
		/// <param name="source">The source object</param>
		/// <returns>A new object of the provided destination type with the mapped data</returns>
		/// ======================================================================================================================
		TDestination Map<TSource, TDestination>(TSource source)
			where TSource : class
			where TDestination : class, new();

		/// ======================================================================================================================
		/// <summary>
		/// Maps an object to an object of the destination type. The type of the source object is automatically obtained.
		/// In this case the destination object is known, which means that the properties of the existing destination
		/// object are filled with the values of the properties of the source object. If no map between the types exists 
		/// an exception is thrown. If the source object is null, the method returns null.
		/// </summary>
		/// <typeparam name="TDestination">The destination type</typeparam>
		/// <param name="source">The source object</param>
		/// <param name="destination">The destination object</param>
		/// <returns>The destination object filled with the data from the source object</returns>
		/// ======================================================================================================================
		TDestination? Map<TDestination>(object? source, TDestination? destination)
			where TDestination : class, new();

		/// ======================================================================================================================
		/// <summary>
		/// Maps an IEnumerable of source objects to an IEnumerable of destination objects
		/// </summary>
		/// <typeparam name="TSource">The source type</typeparam>
		/// <typeparam name="TDestination">The destination type</typeparam>
		/// <param name="source">the source object</param>
		/// <returns>An IEnumerable of destination objects with the mapped data of the source objects</returns>
		/// ======================================================================================================================
		IEnumerable<TDestination> Map<TSource, TDestination>(IEnumerable<TSource> source)
			where TSource : class
			where TDestination : class, new();

		/// ======================================================================================================================
		/// <summary>
		/// Maps an object to a new object of the destination type. If the source object is null, the method returns null.
		/// </summary>
		/// <typeparam name="TDestination">The destination type</typeparam>
		/// <param name="source">The source object</param>
		/// <returns>A new object of the destination type with the data of the source type</returns>
		/// ======================================================================================================================
		TDestination? Map<TDestination>(object? source)
			where TDestination : class, new();
	}
}
