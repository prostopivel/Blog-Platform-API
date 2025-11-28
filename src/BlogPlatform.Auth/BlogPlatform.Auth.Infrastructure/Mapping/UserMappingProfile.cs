using AutoMapper;
using BlogPlatform.Auth.Core.Entities;
using BlogPlatform.Auth.Infrastructure.Entities;

namespace BlogPlatform.Auth.Infrastructure.Mapping
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<User, UserEntity>();
            CreateMap<UserEntity, User>();
        }
    }
}
