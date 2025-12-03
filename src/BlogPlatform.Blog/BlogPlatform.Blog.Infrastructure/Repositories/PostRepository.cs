using AutoMapper;
using BlogPlatform.Blog.Core.Interfaces.Repositories;
using BlogPlatform.Blog.Core.Models;
using BlogPlatform.Blog.Infrastructure.Entities;
using BlogPlatform.Shared.Common.Models;
using Dapper;
using System.Data;

namespace BlogPlatform.Blog.Infrastructure.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly IDbConnection _connection;
        private readonly IMapper _mapper;

        public PostRepository(IDbConnection connection,
            IMapper mapper)
        {
            _connection = connection;
            _mapper = mapper;
        }

        public async Task<Post?> GetByIdAsync(Guid id,
            CancellationToken token = default)
        {
            const string sql = "SELECT * FROM get_post_by_id(@p_id)";

            var commandDefinition = new CommandDefinition(
                commandText: sql,
                parameters: new { p_id = id },
                cancellationToken: token
            );

            var result = await _connection.QueryAsync<PostEntity>(commandDefinition);
            return _mapper.Map<Post?>(result?.SingleOrDefault());
        }

        public async Task<PaginatedResult<Post>> GetByTagsAsync(IEnumerable<string> tags,
            int page = 1,
            int pageSize = 10,
            CancellationToken token = default)
        {
            if (tags?.Any() != true)
            {
                return new PaginatedResult<Post>
                {
                    Items = [],
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = 0
                };
            }

            const string sql = @"SELECT * FROM get_posts_by_tags(@limit_count, @offset_count, VARIADIC @post_tags)";

            var commandDefinition = new CommandDefinition(
                commandText: sql,
                parameters: new
                {
                    limit_count = pageSize,
                    offset_count = (page - 1) * pageSize,
                    post_tags = tags.ToArray()
                },
                cancellationToken: token
            );

            var result = await _connection.QueryAsync<PostEntity, int, EntityWithCount<PostEntity>>(
                commandDefinition,
                map: (post, totalCount) => new EntityWithCount<PostEntity>
                {
                    Entity = post,
                    TotalCount = totalCount
                },
                splitOn: "total_count");

            return new PaginatedResult<Post>
            {
                Items = _mapper.Map<IEnumerable<Post>>(result.Select(r => r.Entity)),
                TotalCount = (int)(result.FirstOrDefault()?.TotalCount ?? 0),
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PaginatedResult<Post>> GetByUserIdAsync(Guid userId,
            int page = 1,
            int pageSize = 10,
            CancellationToken token = default)
        {
            const string sql = "SELECT * FROM get_user_posts(@check_user_id, @limit_count, @offset_count)";

            var commandDefinition = new CommandDefinition(
                commandText: sql,
                parameters: new
                {
                    check_user_id = userId,
                    limit_count = pageSize,
                    offset_count = (page - 1) * pageSize
                },
                cancellationToken: token
            );

            var result = await _connection.QueryAsync<PostEntity, int, EntityWithCount<PostEntity>>(
                commandDefinition,
                map: (post, totalCount) => new EntityWithCount<PostEntity>
                {
                    Entity = post,
                    TotalCount = totalCount
                },
                splitOn: "total_count");

            return new PaginatedResult<Post>
            {
                Items = _mapper.Map<IEnumerable<Post>>(result.Select(r => r.Entity)),
                TotalCount = (int)(result.FirstOrDefault()?.TotalCount ?? 0),
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<Guid> CreateAsync(Post post,
            CancellationToken token = default)
        {
            const string sql = "SELECT create_post(@p_id, @p_title, @p_content, @p_user_id, @p_created_at)";

            var commandDefinition = new CommandDefinition(
                commandText: sql,
                parameters: new
                {
                    p_id = post.Id,
                    p_title = post.Title,
                    p_content = post.Content,
                    p_user_id = post.UserId,
                    p_created_at = post.CreatedAt
                },
                cancellationToken: token
            );

            var result = await _connection.ExecuteScalarAsync<Guid>(commandDefinition);
            return result;
        }

        public async Task<Guid> UpdateAsync(Post post,
            CancellationToken token = default)
        {
            const string sql = "SELECT update_post(@p_id, @p_title, @p_content, @p_updated_at)";

            var commandDefinition = new CommandDefinition(
                commandText: sql,
                parameters: new
                {
                    p_id = post.Id,
                    p_title = post.Title,
                    p_content = post.Content,
                    p_updated_at = post.UpdatedAt
                },
                cancellationToken: token
            );

            var result = await _connection.ExecuteScalarAsync<Guid>(commandDefinition);
            return result;
        }

        public async Task DeleteAsync(Guid id,
            CancellationToken token = default)
        {
            const string sql = "SELECT delete_post(@p_id)";

            var commandDefinition = new CommandDefinition(
                commandText: sql,
                parameters: new { p_id = id },
                cancellationToken: token
            );

            await _connection.ExecuteScalarAsync<Guid>(commandDefinition);
        }

        public async Task<bool> ExistsAsync(Guid id,
            CancellationToken token = default)
        {
            const string sql = "SELECT exists_post(@p_id)";

            var commandDefinition = new CommandDefinition(
                commandText: sql,
                parameters: new { p_id = id },
                cancellationToken: token
            );

            var result = await _connection.ExecuteScalarAsync<bool>(commandDefinition);
            return result;
        }

        public async Task<bool> IsUserPostAsync(Guid id,
            Guid userId,
            CancellationToken token = default)
        {
            const string sql = "SELECT is_user_post(@p_id, @p_user_id)";

            var commandDefinition = new CommandDefinition(
                commandText: sql,
                parameters: new
                {
                    p_id = id,
                    p_user_id = userId
                },
                cancellationToken: token
            );

            var result = await _connection.ExecuteScalarAsync<bool>(commandDefinition);
            return result;
        }
    }
}
