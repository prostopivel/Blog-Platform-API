using AutoMapper;
using BlogPlatform.Blog.Core.Interfaces.Repositories;
using Dapper;
using System.Data;

namespace BlogPlatform.Blog.Infrastructure.Repositories
{
    public class LikeRepository : ILikeRepository
    {
        private readonly IDbConnection _connection;
        private readonly IMapper _mapper;

        public LikeRepository(IDbConnection connection,
            IMapper mapper)
        {
            _connection = connection;
            _mapper = mapper;
        }

        public async Task<bool> ToggleLikeAsync(Guid postId,
            Guid userId,
            CancellationToken token = default)
        {
            const string sql = "SELECT change_like_post_by_user(@post_id, @user_id)";

            var commandDefinition = new CommandDefinition(
                commandText: sql,
                parameters: new
                {
                    post_id = postId,
                    user_id = userId
                },
                cancellationToken: token
            );

            var result = await _connection.ExecuteScalarAsync<bool>(commandDefinition);
            return result;
        }

        public async Task<bool> IsLikedAsync(Guid postId,
            Guid userId,
            CancellationToken token = default)
        {
            const string sql = "SELECT is_user_like_post(@post_id, @user_id)";

            var commandDefinition = new CommandDefinition(
                commandText: sql,
                parameters: new
                {
                    post_id = postId,
                    user_id = userId
                },
                cancellationToken: token
            );

            var result = await _connection.ExecuteScalarAsync<bool>(commandDefinition);
            return result;
        }

        public async Task<int> GetLikesCountAsync(Guid postId,
            CancellationToken token = default)
        {
            const string sql = "SELECT get_post_likes_count(@post_id)";

            var commandDefinition = new CommandDefinition(
                commandText: sql,
                parameters: new { post_id = postId },
                cancellationToken: token
            );

            var result = await _connection.ExecuteScalarAsync<int>(commandDefinition);
            return result;
        }
    }
}
