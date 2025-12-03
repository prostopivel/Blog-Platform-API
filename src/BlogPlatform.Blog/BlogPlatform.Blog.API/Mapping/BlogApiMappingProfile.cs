using AutoMapper;
using BlogPlatform.Blog.API.DTOs;
using BlogPlatform.Blog.Core.Models;

namespace BlogPlatform.Blog.API.Mapping
{
    public class BlogApiMappingProfile : Profile
    {
        public BlogApiMappingProfile()
        {
            CreateMap<CreateCommentRequest, Comment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt =>
                    opt.MapFrom(_ => DateTime.Now));

            CreateMap<CreatePostRequest, Post>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt =>
                    opt.MapFrom(_ => DateTime.Now))
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Tags, opt =>
                    opt.MapFrom(src => src.Tags
                        .Distinct()
                        .Select(tag => new Tag(Guid.Empty, tag))))
                .ForMember(dest => dest.Comments, opt => opt.Ignore())
                .ForMember(dest => dest.LikesCount, opt => opt.Ignore());

            CreateMap<UpdatePostRequest, Post>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt =>
                    opt.MapFrom(_ => DateTime.Now))
                .ForMember(dest => dest.Tags, opt =>
                    opt.MapFrom(src => src.Tags
                        .Distinct()
                        .Select(tag => new Tag(Guid.Empty, tag))))
                .ForMember(dest => dest.Comments, opt => opt.Ignore())
                .ForMember(dest => dest.LikesCount, opt => opt.Ignore());
        }
    }
}
