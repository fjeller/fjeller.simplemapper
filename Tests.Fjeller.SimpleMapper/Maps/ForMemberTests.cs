using Fjeller.SimpleMapper;
using Tests.Fjeller.SimpleMapper.TestInfrastructure;

namespace Tests.Fjeller.SimpleMapper.Maps;

/// ======================================================================================================================
/// <summary>
/// Tests for ForMember custom property mapping functionality
/// </summary>
/// ======================================================================================================================
public class ForMemberTests : IDisposable
{
	public ForMemberTests()
	{
		TestHelper.ResetMapperCache();
	}

	public void Dispose()
	{
		TestHelper.ResetMapperCache();
		GC.SuppressFinalize(this);
	}

	private class ForMemberBasicProfile : MappingProfile
	{
		public ForMemberBasicProfile()
		{
			CreateMap<SourceWithDisplayName, DestinationWithCustomMapping>()
				.ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.DisplayName));
		}
	}

	private class ForMemberComputedProfile : MappingProfile
	{
		public ForMemberComputedProfile()
		{
			CreateMap<SourceWithDisplayName, DestinationWithCustomMapping>()
				.ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
		}
	}

	private class ForMemberMultipleProfile : MappingProfile
	{
		public ForMemberMultipleProfile()
		{
			CreateMap<SourceWithDisplayName, DestinationWithCustomMapping>()
				.ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.DisplayName))
				.ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
		}
	}

	private class ForMemberWithAfterMappingProfile : MappingProfile
	{
		public ForMemberWithAfterMappingProfile()
		{
			CreateMap<SourceWithDisplayName, DestinationWithCustomMapping>()
				.ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.DisplayName))
				.ExecuteAfterMapping((src, dest) => dest.FullName = $"{src.FirstName} {src.LastName}");
		}
	}

	private class ForMemberWithIgnoreProfile : MappingProfile
	{
		public ForMemberWithIgnoreProfile()
		{
			CreateMap<SourceModel, DestinationModel>()
				.ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Email))
				.IgnoreMember(x => x.Age);
		}
	}

	private class ForMemberIntegerProfile : MappingProfile
	{
		public ForMemberIntegerProfile()
		{
			CreateMap<SourceModel, DestinationModel>()
				.ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.Id));
		}
	}

	[Fact]
	public void ForMember_Should_MapFromDifferentProperty_When_Configured()
	{
		new ForMemberBasicProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		SourceWithDisplayName source = new()
		{
			Id = 1,
			Name = "John",
			DisplayName = "Johnny"
		};

		DestinationWithCustomMapping result = mapper.Map<SourceWithDisplayName, DestinationWithCustomMapping>(source);

		Assert.NotNull(result);
		Assert.Equal(1, result.Id);
		Assert.Equal("Johnny", result.Name);
	}

	[Fact]
	public void ForMember_Should_NotAutoMap_When_CustomMappingExists()
	{
		new ForMemberBasicProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		SourceWithDisplayName source = new()
		{
			Id = 1,
			Name = "SHOULD_NOT_APPEAR",
			DisplayName = "Johnny"
		};

		DestinationWithCustomMapping result = mapper.Map<SourceWithDisplayName, DestinationWithCustomMapping>(source);

		Assert.NotEqual("SHOULD_NOT_APPEAR", result.Name);
		Assert.Equal("Johnny", result.Name);
	}

	[Fact]
	public void ForMember_Should_MapComputedValue_When_ExpressionProvided()
	{
		new ForMemberComputedProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		SourceWithDisplayName source = new()
		{
			Id = 1,
			FirstName = "John",
			LastName = "Doe"
		};

		DestinationWithCustomMapping result = mapper.Map<SourceWithDisplayName, DestinationWithCustomMapping>(source);

		Assert.Equal("John Doe", result.FullName);
	}

	[Fact]
	public void ForMember_Should_SupportMultipleMappings_When_ConfiguredForDifferentProperties()
	{
		new ForMemberMultipleProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		SourceWithDisplayName source = new()
		{
			Id = 1,
			Name = "IGNORED",
			DisplayName = "Johnny",
			FirstName = "John",
			LastName = "Doe"
		};

		DestinationWithCustomMapping result = mapper.Map<SourceWithDisplayName, DestinationWithCustomMapping>(source);

		Assert.Equal(1, result.Id);
		Assert.Equal("Johnny", result.Name);
		Assert.Equal("John Doe", result.FullName);
	}

	private class ForMemberDuplicateProfile : MappingProfile
	{
		public ForMemberDuplicateProfile()
		{
			CreateMap<SourceWithDisplayName, DestinationWithCustomMapping>()
				.ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.DisplayName))
				.ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FirstName));
		}
	}

	[Fact]
	public void ForMember_Should_ThrowException_When_CalledTwiceForSameProperty()
	{
		InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
		{
			new ForMemberDuplicateProfile();
		});

		Assert.Contains("already been configured", exception.Message);
		Assert.Contains("Name", exception.Message);
	}

	private class ForMemberNoMapFromProfile : MappingProfile
	{
		public ForMemberNoMapFromProfile()
		{
			CreateMap<SourceWithDisplayName, DestinationWithCustomMapping>()
				.ForMember(dest => dest.Name, opt => { });
		}
	}

	[Fact]
	public void ForMember_Should_ThrowException_When_MapFromNotCalled()
	{
		InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
		{
			new ForMemberNoMapFromProfile();
		});

		Assert.Contains("MapFrom must be called", exception.Message);
	}

	[Fact]
	public void ForMember_Should_WorkWithExecuteAfterMapping_When_BothConfigured()
	{
		new ForMemberWithAfterMappingProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		SourceWithDisplayName source = new()
		{
			Id = 1,
			DisplayName = "Johnny",
			FirstName = "John",
			LastName = "Doe"
		};

		DestinationWithCustomMapping result = mapper.Map<SourceWithDisplayName, DestinationWithCustomMapping>(source);

		Assert.Equal("Johnny", result.Name);
		Assert.Equal("John Doe", result.FullName);
	}

	[Fact]
	public void ForMember_Should_WorkWithIgnoreMember_When_BothConfigured()
	{
		new ForMemberWithIgnoreProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		SourceModel source = new()
		{
			Id = 1,
			Name = "IGNORED",
			Email = "test@example.com",
			Age = 30
		};

		DestinationModel result = mapper.Map<SourceModel, DestinationModel>(source);

		Assert.Equal(1, result.Id);
		Assert.Equal("test@example.com", result.Name);
		Assert.Equal(0, result.Age);
	}

	[Fact]
	public void ForMember_Should_HandleNullValues_When_SourcePropertyIsNull()
	{
		new ForMemberBasicProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		SourceWithDisplayName source = new()
		{
			Id = 1,
			DisplayName = null!
		};

		DestinationWithCustomMapping result = mapper.Map<SourceWithDisplayName, DestinationWithCustomMapping>(source);

		Assert.Equal(1, result.Id);
		Assert.Null(result.Name);
	}

	[Fact]
	public void ForMember_Should_MapCollections_When_CustomMappingUsed()
	{
		new ForMemberBasicProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		List<SourceWithDisplayName> sources = new()
		{
			new SourceWithDisplayName { Id = 1, DisplayName = "First" },
			new SourceWithDisplayName { Id = 2, DisplayName = "Second" }
		};

		IEnumerable<DestinationWithCustomMapping> results = mapper.Map<SourceWithDisplayName, DestinationWithCustomMapping>(sources);

		List<DestinationWithCustomMapping> resultList = results.ToList();
		Assert.Equal(2, resultList.Count);
		Assert.Equal("First", resultList[0].Name);
		Assert.Equal("Second", resultList[1].Name);
	}

	[Fact]
	public void ForMember_Should_ExcludeSourceProperty_When_CustomMappingConfigured()
	{
		new ForMemberBasicProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		SourceWithDisplayName source = new()
		{
			Id = 1,
			Name = "OriginalName",
			DisplayName = "CustomName"
		};

		DestinationWithCustomMapping result = mapper.Map<SourceWithDisplayName, DestinationWithCustomMapping>(source);

		Assert.Equal("CustomName", result.Name);
		Assert.NotEqual("OriginalName", result.Name);
	}

	[Fact]
	public void ForMember_Should_HandleIntegerProperties_When_Mapped()
	{
		new ForMemberIntegerProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		SourceModel source = new()
		{
			Id = 42,
			Age = 25
		};

		DestinationModel result = mapper.Map<SourceModel, DestinationModel>(source);

		Assert.Equal(42, result.Age);
	}

	[Fact]
	public void ForMember_Should_WorkWithRuntimeTypeDetection_When_ObjectSource()
	{
		new ForMemberBasicProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		object source = new SourceWithDisplayName
		{
			Id = 1,
			DisplayName = "Test"
		};

		DestinationWithCustomMapping? result = mapper.Map<DestinationWithCustomMapping>(source);

		Assert.NotNull(result);
		Assert.Equal("Test", result.Name);
	}

	[Theory]
	[InlineData("", "")]
	[InlineData("Test", "Test")]
	[InlineData("Long Display Name", "Long Display Name")]
	public void ForMember_Should_HandleVariousStringValues_When_Mapping(string displayName, string expected)
	{
		new ForMemberBasicProfile();

		ISimpleMapper mapper = new global::Fjeller.SimpleMapper.SimpleMapper();
		SourceWithDisplayName source = new()
		{
			Id = 1,
			DisplayName = displayName
		};

		DestinationWithCustomMapping result = mapper.Map<SourceWithDisplayName, DestinationWithCustomMapping>(source);

		Assert.Equal(expected, result.Name);
	}
}
