using Fjeller.SimpleMapper;

namespace Tests.Fjeller.SimpleMapper.TestInfrastructure;

/// ======================================================================================================================
/// <summary>
/// Base test mapping profile for standard mapping scenarios
/// </summary>
/// ======================================================================================================================
public class TestMappingProfile : MappingProfile
{
	public TestMappingProfile()
	{
		CreateMap<SourceModel, DestinationModel>();

		CreateMap<SourceForComputation, DestinationWithComputation>()
			.ExecuteAfterMapping((src, dest) =>
			{
				dest.ComputedValue = src.Value1 + src.Value2;
			});

		CreateMap<SourceWithExtraProperties, DestinationWithFewerProperties>()
			.IgnoreMember(x => x.Password);

		CreateMap<IEntity, DestinationEntity>();

		CreateMap<SourceEntity, DestinationEntity>();

		CreateMap<SourceWithCollections, DestinationWithCollections>();
	}
}
