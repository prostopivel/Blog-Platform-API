using AutoMapper;
using BlogPlatform.Blog.Core.Models;
using BlogPlatform.Blog.Infrastructure.Entities;

namespace BlogPlatform.Blog.Infrastructure.Mapping
{
    public class BlogMappingProfile : Profile
    {
        public BlogMappingProfile()
        {
            CreateMap<Tag, TagEntity>();
            CreateMap<TagEntity, Tag>();

            CreateMap<CommentEntity, Comment>();
            CreateMap<Comment, CommentEntity>();

            CreateMap<PostEntity, Post>();
            CreateMap<Post, PostEntity>();
        }
    }
}
