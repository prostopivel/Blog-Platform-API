using BlogPlatform.Blog.Core.Interfaces.Services;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace BlogPlatform.Blog.Grpc.Services
{
    public class BlogGrpcService : BlogService.BlogServiceBase
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogger<BlogGrpcService> _logger;

        public BlogGrpcService(IAnalyticsService analyticsService,
            ILogger<BlogGrpcService> logger)
        {
            _analyticsService = analyticsService;
            _logger = logger;
        }

        public async override Task<PostsByDateResponse> GetPostsByDateRange(
            DateRangeRequest request,
            ServerCallContext context)
        {
            try
            {
                var result = await _analyticsService.GetPostsByDateRangeAsync(
                request.StartDate.ToDateTime(),
                request.EndDate.ToDateTime(),
                context.CancellationToken);

                return new PostsByDateResponse
                {
                    Items = { result }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting posts by date range");
                return new PostsByDateResponse();
            }
        }

        public async override Task<AllActivityResponse> GetUserActivity(
            UserDateRangeRequest request,
            ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out var postId))
                {
                    _logger.LogError("Error parsing user id");
                    return new AllActivityResponse();
                }
                var result = await _analyticsService.GetUserActivity(
                    postId,
                    request.StartDate.ToDateTime(),
                    request.EndDate.ToDateTime(),
                    context.CancellationToken);

                return new AllActivityResponse
                {
                    PItems = { result.PostsByDateItems },
                    CItems = { result.CommentsByDateItems },
                    LItems = { result.LikesByDateItems }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user activity");
                return new AllActivityResponse();
            }
        }

        public async override Task<PostsActivityResponse> GetUserPostsActivity(
            UserDateRangeRequest request,
            ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out var postId))
                {
                    _logger.LogError("Error parsing user id");
                    return new PostsActivityResponse();
                }
                var result = await _analyticsService.GetUserActivity(
                    postId,
                    request.StartDate.ToDateTime(),
                    request.EndDate.ToDateTime(),
                    context.CancellationToken);

                return new PostsActivityResponse
                {
                    Items = { result.PostsByDateItems }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user posts activity");
                return new PostsActivityResponse();
            }
        }

        public async override Task<IdsActivityResponse> GetUserCommentsActivity(
            UserDateRangeRequest request,
            ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out var postId))
                {
                    _logger.LogError("Error parsing user id");
                    return new IdsActivityResponse();
                }
                var result = await _analyticsService.GetUserActivity(
                    postId,
                    request.StartDate.ToDateTime(),
                    request.EndDate.ToDateTime(),
                    context.CancellationToken);

                return new IdsActivityResponse
                {
                    Items = { result.CommentsByDateItems }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user comments activity");
                return new IdsActivityResponse();
            }
        }

        public async override Task<IdsActivityResponse> GetUserLikesActivity(
            UserDateRangeRequest request,
            ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out var postId))
                {
                    _logger.LogError("Error parsing user id");
                    return new IdsActivityResponse();
                }
                var result = await _analyticsService.GetUserActivity(
                    postId,
                    request.StartDate.ToDateTime(),
                    request.EndDate.ToDateTime(),
                    context.CancellationToken);

                return new IdsActivityResponse
                {
                    Items = { result.LikesByDateItems }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user likes activity");
                return new IdsActivityResponse();
            }
        }

        public async override Task<TagsStatisticsResponse> GetTagsStatistics(
            TopRequest request,
            ServerCallContext context)
        {
            try
            {
                var result = await _analyticsService.GetTagsStatisticsAsync(
                    request.Count,
                    request.StartDate.ToDateTime(),
                    request.EndDate.ToDateTime(),
                    context.CancellationToken);

                return new TagsStatisticsResponse
                {
                    Items = { result }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tags statystics");
                return new TagsStatisticsResponse();
            }
        }
    }
}
