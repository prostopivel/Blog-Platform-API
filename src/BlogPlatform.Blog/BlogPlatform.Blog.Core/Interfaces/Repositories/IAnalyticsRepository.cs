using BlogPlatform.Shared.Grpc.Models;

namespace BlogPlatform.Blog.Core.Interfaces.Repositories
{
    public interface IAnalyticsRepository
    {
        Task<IEnumerable<PostsByDateItem>> GetPostsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken token = default);
        Task<IEnumerable<TagStat>> GetTagsStatisticsAsync(int takeCount, DateTime startDate, DateTime endDate, CancellationToken token = default);
        Task<IEnumerable<PostIdByDateItem>> GetUserActivityCommentsAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken token = default);
        Task<IEnumerable<PostIdByDateItem>> GetUserActivityLikesAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken token = default);
        Task<IEnumerable<PostsByDateItem>> GetUserActivityPostsAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken token = default);
    }
}