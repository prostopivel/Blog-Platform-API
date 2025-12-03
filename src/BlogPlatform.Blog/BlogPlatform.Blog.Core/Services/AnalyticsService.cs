using BlogPlatform.Blog.Core.Interfaces.Repositories;
using BlogPlatform.Blog.Core.Interfaces.Services;
using BlogPlatform.Blog.Core.Models;
using BlogPlatform.Shared.Grpc.Models;

namespace BlogPlatform.Blog.Core.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IAnalyticsRepository _analyticsRepository;

        public AnalyticsService(IAnalyticsRepository analyticsRepository)
        {
            _analyticsRepository = analyticsRepository;
        }

        public async Task<IEnumerable<PostsByDateItem>> GetPostsByDateRangeAsync(
            DateTime startDate,
            DateTime endDate,
            CancellationToken token = default)
        {
            var result = await _analyticsRepository.GetPostsByDateRangeAsync(
                startDate, endDate, token: token);

            return result;
        }

        public async Task<IEnumerable<PostsByDateItem>> GetUserPostsActivity(
            Guid userId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken token = default)
        {
            var postsResult = await _analyticsRepository.GetUserActivityPostsAsync(
                userId, startDate, endDate, token: token);

            return postsResult;
        }

        public async Task<IEnumerable<IdsByDateItem>> GetUserCommentsActivity(
            Guid userId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken token = default)
        {
            var commentsResult = await _analyticsRepository.GetUserActivityCommentsAsync(
                userId, startDate, endDate, token: token);

            return commentsResult;
        }

        public async Task<IEnumerable<IdsByDateItem>> GetUserLikesActivity(
            Guid userId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken token = default)
        {
            var likesResult = await _analyticsRepository.GetUserActivityLikesAsync(
                userId, startDate, endDate, token: token);

            return likesResult;
        }

        public async Task<AnalyticsResult> GetUserActivity(
            Guid userId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken token = default)
        {
            var postsResult = await _analyticsRepository.GetUserActivityPostsAsync(
                userId, startDate, endDate, token: token);
            var commentsResult = await _analyticsRepository.GetUserActivityCommentsAsync(
                userId, startDate, endDate, token: token);
            var likesResult = await _analyticsRepository.GetUserActivityLikesAsync(
                userId, startDate, endDate, token: token);

            return new AnalyticsResult(postsResult, commentsResult, likesResult);
        }

        public async Task<IEnumerable<TagStat>> GetTagsStatisticsAsync(
            int takeCount,
            DateTime startDate,
            DateTime endDate,
            CancellationToken token = default)
        {
            var result = await _analyticsRepository.GetTagsStatisticsAsync(
                takeCount, startDate, endDate, token: token);

            return result;
        }
    }
}
