using Lumora.DTOs.Email;
using Lumora.DTOs.Token;
namespace Lumora.Configuration;

public class AutoMapperProfiles : AutoMapper.Profile
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
            .ForMember(dest => dest.UserInfo, opt => opt.MapFrom(src => new UserInfo
            {
                Id = src.Id,
                Email = src.Email,
                Name = src.Name,
                ConfirmedEmail = src.VerifiedEmail
            }))
            .ForMember(dest => dest.Provider, opt => opt.MapFrom(_ => "Google"))
            .ForMember(dest => dest.ProviderKey, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ProviderDisplayName, opt => opt.MapFrom(_ => "Google Account"))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<DateTimeOffset, DateTimeOffset>().ConvertUsing(new DateTimeOffsetToUtcConverter());
        CreateMap<DateTimeOffset?, DateTimeOffset?>().ConvertUsing(new DateTimeOffsetToUtcConverter());
        CreateMap<DateTimeOffset?, DateTimeOffset>().ConvertUsing(new DateTimeOffsetToUtcConverter());

        // EmailTemplate
        CreateMap<EmailTemplateCreateDto, EmailTemplate>().ReverseMap();

        // EmailGroup
        CreateMap<EmailGroupCreateDto, EmailGroup>().ReverseMap();

        // Domain
        CreateMap<Domain, DomainCreateDto>().ReverseMap();

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
