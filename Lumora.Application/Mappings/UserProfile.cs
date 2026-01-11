namespace Lumora.Application.Mappings
{
    public class UserProfile : AutoMapper.Profile
    {
        public UserProfile()
        {
            CreateMap<User, RegisterDto>().ReverseMap();
            CreateMap<RegisterDto, User>();
            CreateMap<User, UserUpdateDto>().ForAllMembers(m => m.Condition(MappingConditions.PropertyNeedsMapping));
            CreateMap<UserUpdateDto, User>().ForAllMembers(m => m.Condition(MappingConditions.PropertyNeedsMapping));
            CreateMap<User, UserProfileDto>().ForAllMembers(m => m.Condition(MappingConditions.PropertyNeedsMapping));
            CreateMap<CompleteUserDataDto, User>().ForAllMembers(m => m.Condition(MappingConditions.PropertyNeedsMapping));

            // UserInfo
            CreateMap<User, UserInfo>().ReverseMap();
            CreateMap<User, UserInfo>().ForAllMembers(m => m.Condition(MappingConditions.PropertyNeedsMapping));
            CreateMap<UserInfo, User>().ForAllMembers(m => m.Condition(MappingConditions.PropertyNeedsMapping));
        }
    }
}
