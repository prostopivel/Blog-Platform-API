using BlogPlatform.Blog.Infrastructure.Entities;
using BlogPlatform.Shared.Data;
using Dapper;

namespace BlogPlatform.Blog.Infrastructure
{
    public static class SqlMapperConfigurator
    {
        public static void ConfigureEntities()
        {
            SqlMapper.AddTypeHandler(new EntityWithCountTypeHandler<PostEntity>());
            SqlMapper.AddTypeHandler(new EntityWithCountTypeHandler<CommentEntity>());
        }
    }
}
