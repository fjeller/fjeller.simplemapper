using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Text;

namespace Fjeller.SimpleMapper.Extensions;

internal static class TupleExtensions
{
	internal static string CreateMapKey(this (Type SourceType, Type DestinationType) types)
	{
		string result = $"{types.SourceType.FullName}_{types.DestinationType.FullName}";

		return result;
	}
}
