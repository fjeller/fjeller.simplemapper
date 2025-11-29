using System;
using System.Collections.Generic;
using System.Text;

namespace Fjeller.SimpleMapper.Exceptions;

public class SimpleMapperException : Exception
{
	public SimpleMapperException()
	{
	}

	public SimpleMapperException( string message ) : base( message )
	{
	}

	public SimpleMapperException( string message, Exception inner ) : base( message, inner )
	{
	}
}
