using Fjeller.SimpleMapper.Exceptions;

namespace Tests.Fjeller.SimpleMapper.Exceptions;

/// ======================================================================================================================
/// <summary>
/// Tests for the MappingKeyException class
/// </summary>
/// ======================================================================================================================
public class MappingKeyExceptionTests
{
	[Fact]
	public void Constructor_Should_CreateException_When_NoParametersProvided()
	{
		MappingKeyException exception = new();

		Assert.NotNull(exception);
		Assert.IsType<MappingKeyException>(exception);
	}

	[Fact]
	public void Constructor_Should_SetMessage_When_MessageProvided()
	{
		string expectedMessage = "Test exception message";

		MappingKeyException exception = new(expectedMessage);

		Assert.Equal(expectedMessage, exception.Message);
	}

	[Fact]
	public void Constructor_Should_SetMessageAndInnerException_When_BothProvided()
	{
		string expectedMessage = "Outer exception";
		Exception innerException = new InvalidOperationException("Inner exception");

		MappingKeyException exception = new(expectedMessage, innerException);

		Assert.Equal(expectedMessage, exception.Message);
		Assert.Same(innerException, exception.InnerException);
	}

	[Fact]
	public void Exception_Should_InheritFromException_When_Created()
	{
		MappingKeyException exception = new();

		Assert.IsAssignableFrom<Exception>(exception);
	}

	[Theory]
	[InlineData("")]
	[InlineData("A")]
	[InlineData("This is a longer exception message for testing purposes")]
	public void Constructor_Should_HandleVariousMessages_When_DifferentMessagesProvided(string message)
	{
		MappingKeyException exception = new(message);

		Assert.Equal(message, exception.Message);
	}

	[Fact]
	public void Exception_Should_BeThrowable_When_Created()
	{
		MappingKeyException? caughtException = null;

		try
		{
			throw new MappingKeyException("Test");
		}
		catch (MappingKeyException ex)
		{
			caughtException = ex;
		}

		Assert.NotNull(caughtException);
		Assert.Equal("Test", caughtException.Message);
	}

	[Fact]
	public void Exception_Should_BeCatchableAsException_When_Thrown()
	{
		bool caught = false;

		try
		{
			throw new MappingKeyException("Test");
		}
		catch (Exception)
		{
			caught = true;
		}

		Assert.True(caught);
	}

	[Fact]
	public void Exception_Should_BeCatchableAsMappingKeyException_When_Thrown()
	{
		bool caught = false;

		try
		{
			throw new MappingKeyException("Test");
		}
		catch (MappingKeyException)
		{
			caught = true;
		}

		Assert.True(caught);
	}

	[Fact]
	public void InnerException_Should_BePreserved_When_ExceptionWrapped()
	{
		Exception originalException = new InvalidOperationException("Original");
		MappingKeyException wrappedException = new("Wrapped", originalException);

		Assert.Same(originalException, wrappedException.InnerException);
		Assert.Equal("Original", wrappedException.InnerException.Message);
	}
}
