using BlogPlatform.Blog.Core.Models;
using BlogPlatform.Shared.Grpc.Models;

namespace BlogPlatform.Blog.Core.Interfaces.Services
{
    public interface IAnalyticsService
    {
        Task<IEnumerable<PostsByDateItem>> GetPostsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken token = default);
        Task<IEnumerable<TagStat>> GetTagsStatisticsAsync(int takeCount, DateTime startDate, DateTime endDate, CancellationToken token = default);
        Task<AnalyticsResult> GetUserActivity(Guid userId, DateTime startDate, DateTime endDate, CancellationToken token = default);
    }
}