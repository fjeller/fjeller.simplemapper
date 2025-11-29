using System;
using System.Collections.Generic;
using System.Text;

namespace Fjeller.SimpleMapper.Extensions;

internal static class ObjectExtensions
{
	private const string _EF_PROXY_NAMESPACE = "System.Data.Entity.DynamicProxies";

	/// ========================================================================================================================================================= 
	/// <summary>
	/// Gets the correct Data Type in case of dynamically created types (e.g. in case of Entity Framework)
	/// </summary>
	/// <param name="currentObject">The object to get the correct type from</param>
	/// <returns>The correct type (in case of dynamically created types vthe base type)</returns>
	/// =========================================================================================================================================================
	internal static Type GetCorrectSourceType( this object currentObject )
	{
		Type currentType = currentObject.GetType();
		Type? result = null;

		if ( _EF_PROXY_NAMESPACE.Equals(currentType.Namespace))
		{
			result = currentType.BaseType;
		}
		return result ?? currentType;
	}
}
