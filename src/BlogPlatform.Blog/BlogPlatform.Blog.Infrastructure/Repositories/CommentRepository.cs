using AutoMapper;
using BlogPlatform.Blog.Core.Interfaces.Repositories;
using BlogPlatform.Blog.Core.Models;
using BlogPlatform.Blog.Infrastructure.Entities;
using BlogPlatform.Shared.Common.Models;
using Dapper;
using System.Data;

namespace BlogPlatform.Blog.Infrastructure.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly IDbConnection _connection;
        private readonly IMapper _mapper;

        public CommentRepository(IDbConnection connection,
            IMapper mapper)
        {
            _connection = connection;
            _mapper = mapper;
        }

        public async Task<Comment> GetByIdAsync(Guid id,
            CancellationToken token = default)
        {
            const string sql = "SELECT * FROM get_comment_by_id(@p_id)";

            var commandDefinition = new CommandDefinition(
                commandText: sql,
                parameters: new { p_id = id },
                cancellationToken: token
            );

            var result = await _connection.QueryAsync<CommentEntity>(commandDefinition);
            return _mapper.Map<Comment>(result?.SingleOrDefault());
        }

        public async Task<PaginatedResult<Comment>> GetByPostIdAsync(Guid postId,
            int page = 1,
            int pageSize = 20,
            CancellationToken token = default)
        {
            const string sql = "SELECT * FROM get_post_comments(@check_post_id, @limit_count, @offset_count)";

            var commandDefinition = new CommandDefinition(
                commandText: sql,
                parameters: new
                {
                    check_post_id = postId,
                    limit_count = pageSize,
                    offset_count = (page - 1) * pageSize
                },
                cancellationToken: token
            );

            var result = await _connection.QueryAsync<CommentEntity, long, EntityWithCount<CommentEntity>>(
                commandDefinition,
                map: (comment, totalCount) => new EntityWithCount<CommentEntity>
                {
                    Entity = comment,
                    TotalCount = totalCount
                },
                splitOn: "total_count");

            return new PaginatedResult<Comment>
            {
                Items = _mapper.Map<IEnumerable<Comment>>(result.Select(r => r.Entity)),
                TotalCount = (int)(result.FirstOrDefault()?.TotalCount ?? 0),
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<Guid> CreateAsync(Comment comment,
            CancellationToken token = default)
        {
            const string sql = "SELECT create_comment(@p_id, @p_post_id, @p_user_id, @p_content, @p_created_at)";

            var commandDefinition = new CommandDefinition(
                commandText: sql,
                parameters: new
                {
                    p_id = comment.Id,
                    p_post_id = comment.PostId,
                    p_user_id = comment.UserId,
                    p_content = comment.Content,
                    p_created_at = comment.CreatedAt
                },
                cancellationToken: token
            );

            var result = await _connection.ExecuteScalarAsync<Guid>(commandDefinition);
            return result;
        }

        public async Task DeleteAsync(Guid id,
            CancellationToken token = default)
        {
            const string sql = "SELECT delete_comment(@p_id)";

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
            const string sql = "SELECT exists_comment(@p_id)";

            var commandDefinition = new CommandDefinition(
                commandText: sql,
                parameters: new { p_id = id },
                cancellationToken: token
            );

            var result = await _connection.ExecuteScalarAsync<bool>(commandDefinition);
            return result;
        }
    }
}
