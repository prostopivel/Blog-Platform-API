using BlogPlatform.Analytics.Core.Models;

namespace BlogPlatform.Analytics.Core.Interfaces.Services
{
    public interface IAnalyticsService
    {
        Task<IntervalItem<PostItem>> GetPostsByDateRangeAsync(DateTime startDate,
            DateTime endDate, Interval interval = Interval.Hour4, CancellationToken token = default);
        Task<IntervalItem<PostItem>> GetUserPostsActivityAsync(Guid userId, DateTime startDate,
            DateTime endDate, Interval interval = Interval.Hour4, CancellationToken token = default);
        Task<IntervalItem<IdItem>> GetUserCommentsActivityAsync(Guid userId, DateTime startDate,
            DateTime endDate, Interval interval = Interval.Hour4, CancellationToken token = default);
        Task<IntervalItem<IdItem>> GetUserLikesActivityAsync(Guid userId, DateTime startDate,
            DateTime endDate, Interval interval = Interval.Hour4, CancellationToken token = default);
        Task<UserActivityResult> GetUserActivityAsync(Guid userId, DateTime startDate,
            DateTime endDate, Interval interval = Interval.Hour4, CancellationToken token = default);
        Task<List<TagStatItem>> GetTagsStatisticsAsync(int topCount, DateTime startDate,
            DateTime endDate, CancellationToken token = default);
    }
}
