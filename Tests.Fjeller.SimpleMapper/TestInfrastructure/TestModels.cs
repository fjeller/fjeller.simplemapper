namespace Tests.Fjeller.SimpleMapper.TestInfrastructure;

/// ======================================================================================================================
/// <summary>
/// Source model for testing basic mapping scenarios
/// </summary>
/// ======================================================================================================================
public class SourceModel
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public int Age { get; set; }
	public bool IsActive { get; set; }
}

/// ======================================================================================================================
/// <summary>
/// Destination model for testing basic mapping scenarios
/// </summary>
/// ======================================================================================================================
public class DestinationModel
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public int Age { get; set; }
	public bool IsActive { get; set; }
}

/// ======================================================================================================================
/// <summary>
/// Source model with additional properties for testing ignored members
/// </summary>
/// ======================================================================================================================
public class SourceWithExtraProperties
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string SecretData { get; set; } = string.Empty;
	public string Password { get; set; } = string.Empty;
}

/// ======================================================================================================================
/// <summary>
/// Destination model with fewer properties for testing ignored members
/// </summary>
/// ======================================================================================================================
public class DestinationWithFewerProperties
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string SecretData { get; set; } = string.Empty;
	public string Password { get; set; } = string.Empty;
}

/// ======================================================================================================================
/// <summary>
/// Source model for testing computed properties
/// </summary>
/// ======================================================================================================================
public class SourceForComputation
{
	public int Value1 { get; set; }
	public int Value2 { get; set; }
}

/// ======================================================================================================================
/// <summary>
/// Destination model with computed property
/// </summary>
/// ======================================================================================================================
public class DestinationWithComputation
{
	public int Value1 { get; set; }
	public int Value2 { get; set; }
	public int ComputedValue { get; set; }
}

/// ======================================================================================================================
/// <summary>
/// Interface for testing interface-based mapping
/// </summary>
/// ======================================================================================================================
public interface IEntity
{
	int Id { get; set; }
	string Name { get; set; }
}

/// ======================================================================================================================
/// <summary>
/// Source model implementing interface for testing interface-based mapping
/// </summary>
/// ======================================================================================================================
public class SourceEntity : IEntity
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
}

/// ======================================================================================================================
/// <summary>
/// Destination model for interface-based mapping
/// </summary>
/// ======================================================================================================================
public class DestinationEntity
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
}

/// ======================================================================================================================
/// <summary>
/// Source model with collection properties for testing collection exclusion
/// </summary>
/// ======================================================================================================================
public class SourceWithCollections
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public List<string> Tags { get; set; } = new();
	public int[] Numbers { get; set; } = Array.Empty<int>();
}

/// ======================================================================================================================
/// <summary>
/// Destination model with collection properties
/// </summary>
/// ======================================================================================================================
public class DestinationWithCollections
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public List<string> Tags { get; set; } = new();
	public int[] Numbers { get; set; } = Array.Empty<int>();
}

/// ======================================================================================================================
/// <summary>
/// Source model with private properties for testing private property mapping
/// </summary>
/// ======================================================================================================================
public class SourceWithPrivateProperties
{
	public int Id { get; set; }
	private string PrivateData { get; set; } = string.Empty;

	public void SetPrivateData(string value)
	{
		PrivateData = value;
	}

	public string GetPrivateData()
	{
		return PrivateData;
	}
}

/// ======================================================================================================================
/// <summary>
/// Destination model with private properties
/// </summary>
/// ======================================================================================================================
public class DestinationWithPrivateProperties
{
	public int Id { get; set; }
	private string PrivateData { get; set; } = string.Empty;

	public string GetPrivateData()
	{
		return PrivateData;
	}
}

/// ======================================================================================================================
/// <summary>
/// Model with incompatible property types for testing type mismatch scenarios
/// </summary>
/// ======================================================================================================================
public class IncompatibleDestination
{
	public int Id { get; set; }
	public int Name { get; set; }
}
