using BlogPlatform.Blog.Core.Interfaces.Repositories;
using BlogPlatform.Blog.Infrastructure.Entities;
using BlogPlatform.Shared.Grpc.Models;
using Dapper;
using Google.Protobuf.WellKnownTypes;
using System.Data;

namespace BlogPlatform.Blog.Infrastructure.Repositories
{
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly IDbConnection _connection;

        public AnalyticsRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<PostsByDateItem>> GetPostsByDateRangeAsync(
            DateTime startDate,
            DateTime endDate,
            CancellationToken token = default)
        {
            const string sql = "SELECT * FROM get_posts_by_date_range(@start_date, @end_date)";

            var commandDefinition = new CommandDefinition(
                commandText: sql,
                parameters: new
                {
                    start_date = startDate,
                    end_date = endDate
                },
                cancellationToken: token
            );

            var result = await _connection.QueryAsync<PostsByDateItemEntity>(commandDefinition);
            return result.Select(p => new PostsByDateItem()
            {
                Id = p.Id.ToString(),
                CreatedAt = Timestamp.FromDateTime(p.CreatedAt),
                CommentCount = p.CommentCount,
                LikeCount = p.LikeCount
            });
        }

        public async Task<IEnumerable<PostsByDateItem>> GetUserActivityPostsAsync(
            Guid userId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken token = default)
        {
            const string sql = "SELECT * FROM get_user_activity_posts(@user_id, @start_date, @end_date)";

            var commandDefinition = new CommandDefinition(
                commandText: sql,
                parameters: new
                {
                    user_id = userId,
                    start_date = startDate,
                    end_date = endDate
                },
                cancellationToken: token
            );

            var result = await _connection.QueryAsync<PostsByDateItemEntity>(commandDefinition);
            return result.Select(p => new PostsByDateItem()
            {
                Id = p.Id.ToString(),
                CreatedAt = Timestamp.FromDateTime(p.CreatedAt),
                CommentCount = p.CommentCount,
                LikeCount = p.LikeCount
            });
        }

        public async Task<IEnumerable<IdsByDateItem>> GetUserActivityCommentsAsync(
            Guid userId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken token = default)
        {
            const string sql = "SELECT * FROM get_user_activity_comments(@user_id, @start_date, @end_date)";

            var commandDefinition = new CommandDefinition(
                commandText: sql,
                parameters: new
                {
                    user_id = userId,
                    start_date = startDate,
                    end_date = endDate
                },
                cancellationToken: token
            );

            var result = await _connection.QueryAsync<IdsByDateItemEntity>(commandDefinition);
            return result.Select(i => new IdsByDateItem()
            {
                Id = i.Id.ToString(),
                CreatedAt = Timestamp.FromDateTime(i.CreatedAt)
            });
        }

        public async Task<IEnumerable<IdsByDateItem>> GetUserActivityLikesAsync(
            Guid userId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken token = default)
        {
            const string sql = "SELECT * FROM get_user_activity_likes(@user_id, @start_date, @end_date)";

            var commandDefinition = new CommandDefinition(
                commandText: sql,
                parameters: new
                {
                    user_id = userId,
                    start_date = startDate,
                    end_date = endDate
                },
                cancellationToken: token
            );

            var result = await _connection.QueryAsync<IdsByDateItemEntity>(commandDefinition);
            return result.Select(i => new IdsByDateItem()
            {
                Id = i.Id.ToString(),
                CreatedAt = Timestamp.FromDateTime(i.CreatedAt)
            });
        }

        public async Task<IEnumerable<TagStat>> GetTagsStatisticsAsync(
            int takeCount,
            DateTime startDate,
            DateTime endDate,
            CancellationToken token = default)
        {
            const string sql = "SELECT * FROM get_tags_statistics(@take_count, @start_date, @end_date)";

            var commandDefinition = new CommandDefinition(
                commandText: sql,
                parameters: new
                {
                    take_count = takeCount,
                    start_date = startDate,
                    end_date = endDate
                },
                cancellationToken: token
            );

            var result = await _connection.QueryAsync<TagStatEntity>(commandDefinition);
            return result.Select(t => new TagStat()
            {
                Id = t.Id.ToString(),
                Name = t.Name,
                PostsCount = t.PostsCount
            });
        }
    }
}