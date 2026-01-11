namespace Lumora.Application.Mappings;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<bool?, bool>().ConvertUsing((src, dest) => src ?? dest);
        CreateMap<int?, int>().ConvertUsing((src, dest) => src ?? dest);
        CreateMap<decimal?, decimal>().ConvertUsing((src, dest) => src ?? dest);
        CreateMap<List<DnsRecord>?, List<DnsRecord>>().ConvertUsing((src, dest) => src ?? dest);
        CreateMap<Dictionary<string, string>?, Dictionary<string, string>>().ConvertUsing((src, dest) => src ?? dest);
        CreateMap<string?[], string?[]>().ConvertUsing((src, dest) => src ?? dest);

        // GoogleUserInfoResponse -> ExternalRegisterDto
        CreateMap<GoogleUserInfoResponse, ExternalRegisterDto>()
            .ForMember(dest => dest.UserInfo, opt => opt.MapFrom(src => src))
            .ForMember(dest => dest.Provider, opt => opt.MapFrom(_ => "Google"))
            .ForMember(dest => dest.ProviderKey, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ProviderDisplayName, opt => opt.MapFrom(_ => "Google Account"));

        CreateMap<GoogleUserInfoResponse, UserInfo>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.ConfirmedEmail, opt => opt.MapFrom(src => src.VerifiedEmail));

        CreateMap<DateTimeOffset, DateTimeOffset>().ConvertUsing(new DateTimeOffsetToUtcConverter());
        CreateMap<DateTimeOffset?, DateTimeOffset?>().ConvertUsing(new DateTimeOffsetToUtcConverter());
        CreateMap<DateTimeOffset?, DateTimeOffset>().ConvertUsing(new DateTimeOffsetToUtcConverter());

        // EmailTemplate
        CreateMap<EmailTemplateCreateDto, EmailTemplate>().ReverseMap();

        // EmailGroup
        CreateMap<EmailGroupCreateDto, EmailGroup>().ReverseMap();

        // Domain
        CreateMap<WebDomain, DomainCreateDto>().ReverseMap();

        // Activity log
        CreateMap<Unsubscribe, UnsubscribeDto>().ReverseMap();

        // Exam 
        CreateMap<TestQuestion, ExamCreateDto>().ReverseMap();
    }
}

public class DateTimeOffsetToUtcConverter :
    ITypeConverter<DateTimeOffset, DateTimeOffset>,
    ITypeConverter<DateTimeOffset?, DateTimeOffset?>,
    ITypeConverter<DateTimeOffset?, DateTimeOffset>
{
    public DateTimeOffset Convert(DateTimeOffset source, DateTimeOffset destination, ResolutionContext context)
    {
        return source.ToUniversalTime();
    }

    public DateTimeOffset? Convert(DateTimeOffset? source, DateTimeOffset? destination, ResolutionContext context)
    {
        if (source == null)
        {
            return destination;
        }

        return Convert(source.Value, destination ?? DateTimeOffset.MinValue, context);
    }

    public DateTimeOffset Convert(DateTimeOffset? source, DateTimeOffset destination, ResolutionContext context)
    {
        if (source == null)
        {
            return destination;
        }

        return Convert(source.Value, destination, context);
    }
}
