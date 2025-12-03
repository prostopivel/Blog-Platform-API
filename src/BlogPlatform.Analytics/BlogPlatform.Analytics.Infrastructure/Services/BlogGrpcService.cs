using BlogPlatform.Analytics.Core.Interfaces.Services;
using BlogPlatform.Analytics.Core.Models;
using BlogPlatform.Blog.Grpc;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;

namespace BlogPlatform.Analytics.Infrastructure.Services
{
    public class BlogGrpcService : IBlogGrpcService
    {
        private readonly BlogService.BlogServiceClient _client;
        private readonly IIntervalService _intervalService;
        private readonly ILogger<BlogGrpcService> _logger;

        public BlogGrpcService(BlogService.BlogServiceClient client,
            IIntervalService intervalService,
            ILogger<BlogGrpcService> logger)
        {
            _client = client;
            _intervalService = intervalService;
            _logger = logger;
        }

        public async Task<IntervalItem<PostItem>> GetPostsByDateRangeAsync(DateTime startDate,
            DateTime endDate, Interval interval, CancellationToken token)
        {
            try
            {
                var request = new DateRangeRequest
                {
                    StartDate = Timestamp.FromDateTime(startDate.ToUniversalTime()),
                    EndDate = Timestamp.FromDateTime(endDate.ToUniversalTime())
                };

                var response = await _client.GetPostsByDateRangeAsync(request, cancellationToken: token);

                var result = _intervalService.SplitListByIntervals(
                    [.. response.Items], startDate, endDate, interval,
                    p => p.CreatedAt.ToDateTime(),
                    p => new PostItem()
                    {
                        Id = Guid.Parse(p.Id),
                        CreatedAt = p.CreatedAt.ToDateTime(),
                        CommentCount = p.CommentCount,
                        LikeCount = p.LikeCount
                    });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling gRPC service");
                throw;
            }
        }

        public async Task<IntervalItem<PostItem>> GetUserPostsActivityAsync(Guid userId,
            DateTime startDate, DateTime endDate, Interval interval = Interval.Hour4,
            CancellationToken token = default)
        {
            try
            {
                var request = new UserDateRangeRequest
                {
                    Id = userId.ToString(),
                    StartDate = Timestamp.FromDateTime(startDate.ToUniversalTime()),
                    EndDate = Timestamp.FromDateTime(endDate.ToUniversalTime())
                };

                var response = await _client.GetUserPostsActivityAsync(request, cancellationToken: token);

                var result = _intervalService.SplitListByIntervals(
                    [.. response.Items], startDate, endDate, interval,
                    p => p.CreatedAt.ToDateTime(),
                    p => new PostItem()
                    {
                        Id = Guid.Parse(p.Id),
                        CreatedAt = p.CreatedAt.ToDateTime(),
                        CommentCount = p.CommentCount,
                        LikeCount = p.LikeCount
                    });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling gRPC service");
                throw;
            }
        }

        public async Task<IntervalItem<IdItem>> GetUserCommentsActivityAsync(Guid userId,
            DateTime startDate, DateTime endDate, Interval interval = Interval.Hour4,
            CancellationToken token = default)
        {
            try
            {
                var request = new UserDateRangeRequest
                {
                    Id = userId.ToString(),
                    StartDate = Timestamp.FromDateTime(startDate.ToUniversalTime()),
                    EndDate = Timestamp.FromDateTime(endDate.ToUniversalTime())
                };

                var response = await _client.GetUserCommentsActivityAsync(request, cancellationToken: token);

                var result = _intervalService.SplitListByIntervals(
                    [.. response.Items], startDate, endDate, interval,
                    p => p.CreatedAt.ToDateTime(),
                    p => new IdItem()
                    {
                        Id = Guid.Parse(p.Id),
                        CreatedAt = p.CreatedAt.ToDateTime()
                    });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling gRPC service");
                throw;
            }
        }

        public async Task<IntervalItem<IdItem>> GetUserLikesActivityAsync(Guid userId,
            DateTime startDate, DateTime endDate, Interval interval = Interval.Hour4,
            CancellationToken token = default)
        {
            try
            {
                var request = new UserDateRangeRequest
                {
                    Id = userId.ToString(),
                    StartDate = Timestamp.FromDateTime(startDate.ToUniversalTime()),
                    EndDate = Timestamp.FromDateTime(endDate.ToUniversalTime())
                };

                var response = await _client.GetUserLikesActivityAsync(request, cancellationToken: token);

                var result = _intervalService.SplitListByIntervals(
                    [.. response.Items], startDate, endDate, interval,
                    p => p.CreatedAt.ToDateTime(),
                    p => new IdItem()
                    {
                        Id = Guid.Parse(p.Id),
                        CreatedAt = p.CreatedAt.ToDateTime()
                    });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling gRPC service");
                throw;
            }
        }

        public async Task<UserActivityResult> GetUserActivityAsync(Guid userId,
            DateTime startDate, DateTime endDate, Interval interval = Interval.Hour4,
            CancellationToken token = default)
        {
            try
            {
                var request = new UserDateRangeRequest
                {
                    Id = userId.ToString(),
                    StartDate = Timestamp.FromDateTime(startDate.ToUniversalTime()),
                    EndDate = Timestamp.FromDateTime(endDate.ToUniversalTime())
                };

                var response = await _client.GetUserActivityAsync(request, cancellationToken: token);

                var postsResult = _intervalService.SplitListByIntervals(
                    [.. response.PItems], startDate, endDate, interval,
                    p => p.CreatedAt.ToDateTime(),
                    p => new PostItem()
                    {
                        Id = Guid.Parse(p.Id),
                        CreatedAt = p.CreatedAt.ToDateTime(),
                        CommentCount = p.CommentCount,
                        LikeCount = p.LikeCount
                    });
                var commentsResult = _intervalService.SplitListByIntervals(
                   [.. response.CItems], startDate, endDate, interval,
                   p => p.CreatedAt.ToDateTime(),
                   p => new IdItem()
                   {
                       Id = Guid.Parse(p.Id),
                       CreatedAt = p.CreatedAt.ToDateTime()
                   });
                var likesResult = _intervalService.SplitListByIntervals(
                   [.. response.LItems], startDate, endDate, interval,
                   p => p.CreatedAt.ToDateTime(),
                   p => new IdItem()
                   {
                       Id = Guid.Parse(p.Id),
                       CreatedAt = p.CreatedAt.ToDateTime()
                   });

                return new UserActivityResult()
                {
                    PostItems = postsResult,
                    CommentItems = commentsResult,
                    LikeItems = likesResult
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling gRPC service");
                throw;
            }
        }

        public async Task<List<TagStatItem>> GetTagsStatisticsAsync(int topCount,
            DateTime startDate, DateTime endDate, CancellationToken token = default)
        {
            try
            {
                var request = new TopRequest
                {
                    Count = topCount,
                    StartDate = Timestamp.FromDateTime(startDate.ToUniversalTime()),
                    EndDate = Timestamp.FromDateTime(endDate.ToUniversalTime())
                };

                var response = await _client.GetTagsStatisticsAsync(request, cancellationToken: token);

                var result = response.Items.Select(t => new TagStatItem()
                {
                    Id = Guid.Parse(t.Id),
                    Name = t.Name,
                    PostsCount = t.PostsCount
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling gRPC service");
                throw;
            }
        }
    }
}