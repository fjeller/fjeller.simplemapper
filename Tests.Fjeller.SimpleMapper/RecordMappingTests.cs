using Fjeller.SimpleMapper;
using Fjeller.SimpleMapper.Exceptions;
using Tests.Fjeller.SimpleMapper.TestInfrastructure;

namespace Tests.Fjeller.SimpleMapper;

/// ======================================================================================================================
/// <summary>
/// Tests for mapping to destination types without a parameterless constructor (records), init-only properties,
/// and required members.
/// </summary>
/// ======================================================================================================================
public class RecordMappingTests : IDisposable
{
	public RecordMappingTests()
	{
		TestHelper.ResetMapperCache();
	}

	public void Dispose()
	{
		TestHelper.ResetMapperCache();
		GC.SuppressFinalize( this );
	}

	private class RecordProfile : MappingProfile
	{
		public RecordProfile()
		{
			CreateMap<SourceModel, DestinationRecord>();
		}
	}

	private class InitOnlyProfile : MappingProfile
	{
		public InitOnlyProfile()
		{
			CreateMap<SourceModel, DestinationWithInitOnlyProperties>();
		}
	}

	private class RequiredMemberProfile : MappingProfile
	{
		public RequiredMemberProfile()
		{
			CreateMap<SourceModel, DestinationWithRequiredMember>();
		}
	}

	private class UnresolvableRequiredMemberProfile : MappingProfile
	{
		public UnresolvableRequiredMemberProfile()
		{
			CreateMap<SourceModel, DestinationWithUnresolvableRequiredMember>();
		}
	}

	private class AmbiguousConstructorProfile : MappingProfile
	{
		public AmbiguousConstructorProfile()
		{
			CreateMap<SourceModel, DestinationWithAmbiguousConstructors>();
		}
	}

	[Fact]
	public void Map_Should_MapToPositionalRecord_When_NoParameterlessConstructorExists()
	{
		new RecordProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		SourceModel source = new() { Id = 1, Name = "John", Email = "john@example.com" };

		DestinationRecord result = mapper.Map<SourceModel, DestinationRecord>( source );

		Assert.Equal( 1, result.Id );
		Assert.Equal( "John", result.Name );
		Assert.Equal( "john@example.com", result.Email );
	}

	[Fact]
	public void Map_Should_MapToInitOnlyProperties_When_DestinationHasInitOnlyMembers()
	{
		new InitOnlyProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		SourceModel source = new() { Id = 2, Name = "Jane", Email = "jane@example.com" };

		DestinationWithInitOnlyProperties result = mapper.Map<SourceModel, DestinationWithInitOnlyProperties>( source );

		Assert.Equal( 2, result.Id );
		Assert.Equal( "Jane", result.Name );
	}

	[Fact]
	public void Map_Should_MapRequiredMember_When_ResolvableFromSourceProperty()
	{
		new RequiredMemberProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		SourceModel source = new() { Id = 3, Name = "Required", Email = "required@example.com" };

		DestinationWithRequiredMember result = mapper.Map<SourceModel, DestinationWithRequiredMember>( source );

		Assert.Equal( 3, result.Id );
		Assert.Equal( "Required", result.Name );
	}

	[Fact]
	public void CreateMap_Should_ThrowSimpleMapperException_When_RequiredMemberCannotBeResolved()
	{
		new UnresolvableRequiredMemberProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		SourceModel source = new() { Id = 4, Name = "X", Email = "x@example.com" };

		Assert.Throws<SimpleMapperException>( () => mapper.Map<SourceModel, DestinationWithUnresolvableRequiredMember>( source ) );
	}

	[Fact]
	public void CreateMap_Should_ThrowSimpleMapperException_When_DestinationConstructorIsAmbiguous()
	{
		new AmbiguousConstructorProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		SourceModel source = new() { Id = 5, Name = "Ambiguous", Email = "ambiguous@example.com" };

		Assert.Throws<SimpleMapperException>( () => mapper.Map<SourceModel, DestinationWithAmbiguousConstructors>( source ) );
	}

	[Fact]
	public void Map_Should_OverwriteExistingRecordDestination_When_ExistingDestinationProvided()
	{
		new RecordProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		SourceModel source = new() { Id = 6, Name = "Updated", Email = "updated@example.com" };
		DestinationRecord existing = new( 0, "Old", "old@example.com" );

		DestinationRecord result = mapper.Map( source, existing );

		Assert.Equal( 6, result.Id );
		Assert.Equal( "Updated", result.Name );
		Assert.Equal( "updated@example.com", result.Email );
	}
}
