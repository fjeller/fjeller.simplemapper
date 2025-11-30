using Fjeller.SimpleMapper.Exceptions;

namespace Tests.Fjeller.SimpleMapper.Exceptions;

/// ======================================================================================================================
/// <summary>
/// Tests for the SimpleMapperException class
/// </summary>
/// ======================================================================================================================
public class SimpleMapperExceptionTests
{
	[Fact]
	public void Constructor_Should_CreateException_When_NoParametersProvided()
	{
		SimpleMapperException exception = new();

		Assert.NotNull(exception);
		Assert.IsType<SimpleMapperException>(exception);
	}

	[Fact]
	public void Constructor_Should_SetMessage_When_MessageProvided()
	{
		string expectedMessage = "Mapping error occurred";

		SimpleMapperException exception = new(expectedMessage);

		Assert.Equal(expectedMessage, exception.Message);
	}

	[Fact]
	public void Constructor_Should_SetMessageAndInnerException_When_BothProvided()
	{
		string expectedMessage = "Mapping failed";
		Exception innerException = new ArgumentException("Invalid argument");

		SimpleMapperException exception = new(expectedMessage, innerException);

		Assert.Equal(expectedMessage, exception.Message);
		Assert.Same(innerException, exception.InnerException);
	}

	[Fact]
	public void Exception_Should_InheritFromException_When_Created()
	{
		SimpleMapperException exception = new();

		Assert.IsAssignableFrom<Exception>(exception);
	}

	[Theory]
	[InlineData("")]
	[InlineData("Short message")]
	[InlineData("A very long exception message that describes in detail what went wrong during the mapping process")]
	public void Constructor_Should_HandleVariousMessages_When_DifferentMessagesProvided(string message)
	{
		SimpleMapperException exception = new(message);

		Assert.Equal(message, exception.Message);
	}

	[Fact]
	public void Exception_Should_BeThrowable_When_Created()
	{
		SimpleMapperException? caughtException = null;

		try
		{
			throw new SimpleMapperException("Test error");
		}
		catch (SimpleMapperException ex)
		{
			caughtException = ex;
		}

		Assert.NotNull(caughtException);
		Assert.Equal("Test error", caughtException.Message);
	}

	[Fact]
	public void Exception_Should_BeCatchableAsException_When_Thrown()
	{
		bool caught = false;

		try
		{
			throw new SimpleMapperException("Error");
		}
		catch (Exception)
		{
			caught = true;
		}

		Assert.True(caught);
	}

	[Fact]
	public void Exception_Should_BeCatchableAsSimpleMapperException_When_Thrown()
	{
		bool caught = false;

		try
		{
			throw new SimpleMapperException("Error");
		}
		catch (SimpleMapperException)
		{
			caught = true;
		}

		Assert.True(caught);
	}

	[Fact]
	public void InnerException_Should_BePreserved_When_ExceptionWrapped()
	{
		Exception originalException = new NullReferenceException("Null reference");
		SimpleMapperException wrappedException = new("Wrapper message", originalException);

		Assert.Same(originalException, wrappedException.InnerException);
		Assert.Equal("Null reference", wrappedException.InnerException.Message);
	}

	[Fact]
	public void Exception_Should_ContainStackTrace_When_Thrown()
	{
		SimpleMapperException? caughtException = null;

		try
		{
			throw new SimpleMapperException("Test");
		}
		catch (SimpleMapperException ex)
		{
			caughtException = ex;
		}

		Assert.NotNull(caughtException);
		Assert.NotNull(caughtException.StackTrace);
	}

	[Fact]
	public void Exception_Should_BeDifferentType_When_ComparedToMappingKeyException()
	{
		SimpleMapperException simpleMapperException = new();
		MappingKeyException mappingKeyException = new();

		Assert.NotEqual(simpleMapperException.GetType(), mappingKeyException.GetType());
	}
}
