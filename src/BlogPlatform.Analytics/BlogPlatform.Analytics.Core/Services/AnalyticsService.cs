using BlogPlatform.Analytics.Core.Interfaces.Services;
using BlogPlatform.Analytics.Core.Models;

namespace BlogPlatform.Analytics.Core.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IBlogGrpcService _grpcService;

        public AnalyticsService(IBlogGrpcService grpcService)
        {
            _grpcService = grpcService;
        }

        public async Task<IntervalItem<PostItem>> GetPostsByDateRangeAsync(DateTime startDate,
            DateTime endDate, Interval interval = Interval.Hour4, CancellationToken token = default)
        {
            var result = await _grpcService.GetPostsByDateRangeAsync(
                startDate, endDate, interval, token: token);

            return result;
        }

        public async Task<IntervalItem<PostItem>> GetUserPostsActivityAsync(Guid userId,
            DateTime startDate, DateTime endDate, Interval interval = Interval.Hour4,
            CancellationToken token = default)
        {
            var result = await _grpcService.GetUserPostsActivityAsync(
                userId, startDate, endDate, interval, token: token);

            return result;
        }

        public async Task<IntervalItem<IdItem>> GetUserCommentsActivityAsync(Guid userId,
            DateTime startDate, DateTime endDate, Interval interval = Interval.Hour4,
            CancellationToken token = default)
        {
            var result = await _grpcService.GetUserCommentsActivityAsync(
                userId, startDate, endDate, interval, token: token);

            return result;
        }

        public async Task<IntervalItem<IdItem>> GetUserLikesActivityAsync(Guid userId,
            DateTime startDate, DateTime endDate, Interval interval = Interval.Hour4,
            CancellationToken token = default)
        {
            var result = await _grpcService.GetUserLikesActivityAsync(
                userId, startDate, endDate, interval, token: token);

            return result;
        }

        public async Task<UserActivityResult> GetUserActivityAsync(Guid userId,
            DateTime startDate, DateTime endDate, Interval interval = Interval.Hour4,
            CancellationToken token = default)
        {
            var result = await _grpcService.GetUserActivityAsync(
                userId, startDate, endDate, interval, token: token);

            return result;
        }

        public async Task<List<TagStatItem>> GetTagsStatisticsAsync(int topCount,
            DateTime startDate, DateTime endDate, CancellationToken token = default)
        {
            var result = await _grpcService.GetTagsStatisticsAsync(
                topCount, startDate, endDate, token: token);

            return result;
        }
    }
}
