using Lumora.DTOs.Authentication;
using Lumora.DTOs.Token;
using static Lumora.Configuration.MappingProfiles.MappingConditions;

namespace Lumora.Configuration.MappingProfiles
{
    public class UserProfile : AutoMapper.Profile
    {
        public UserProfile()
        {
            CreateMap<User, RegisterDto>().ReverseMap();
            CreateMap<RegisterDto, User>();
            CreateMap<User, UserUpdateDto>().ForAllMembers(m => m.Condition(PropertyNeedsMapping));
            CreateMap<UserUpdateDto, User>().ForAllMembers(m => m.Condition(PropertyNeedsMapping));
            CreateMap<User, UserProfileDto>().ForAllMembers(m => m.Condition(PropertyNeedsMapping));
            CreateMap<CompleteUserDataDto, User>().ForAllMembers(m => m.Condition(PropertyNeedsMapping));

            // UserInfo
            CreateMap<User, UserInfo>().ReverseMap();
            CreateMap<User, UserInfo>().ForAllMembers(m => m.Condition(PropertyNeedsMapping));
            CreateMap<UserInfo, User>().ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        }   
    }
}
