namespace Fjeller.SimpleMapper.Exceptions;

public class MappingKeyException : Exception
{
	public MappingKeyException()
	{
	}

	public MappingKeyException(string message) : base(message)
	{
	}

	public MappingKeyException(string message, Exception inner) : base(message, inner)
	{
	}
}

