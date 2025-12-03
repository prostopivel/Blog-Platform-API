using BlogPlatform.Analytics.Core.Interfaces.Services;
using BlogPlatform.Analytics.Core.Models;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;

namespace BlogPlatform.Analytics.Grpc.Services
{
    public class GrpcBlogDataService : IBlogDataService, IDisposable
    {
        private readonly GrpcChannel _channel;
        private readonly BlogAnalyticsService.BlogAnalyticsServiceClient _client;
        private readonly ILogger<GrpcBlogDataService> _logger;

        public GrpcBlogDataService(string blogServiceUrl,
            ILogger<GrpcBlogDataService> logger)
        {
            _logger = logger;
            _channel = GrpcChannel.ForAddress(blogServiceUrl);
            _client = new BlogAnalyticsService.BlogAnalyticsServiceClient(_channel);
        }

        public async Task<IEnumerable<Post>> GetPostsWithDetailsAsync(DateTime? startDate = null,
            DateTime? endDate = null,
            CancellationToken token = default)
        {
            try
            {
                var request = new DateRangeRequest();

                if (startDate.HasValue)
                {
                    request.StartDate = Google.Protobuf.WellKnownTypes.Timestamp
                        .FromDateTime(startDate.Value.ToUniversalTime());
                }

                if (endDate.HasValue)
                {
                    request.EndDate = Google.Protobuf.WellKnownTypes.Timestamp
                        .FromDateTime(endDate.Value.ToUniversalTime());
                }

                var response = await _client.GetPostsByDateRangeAsync(request);

                // Преобразование из gRPC моделей в локальные
                // В реальности здесь будет более сложная логика
                return new List<Post>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching posts from Blog service");
                throw;
            }
        }

        public async Task<IEnumerable<Comment>> GetCommentsAsync(DateTime? startDate = null,
            DateTime? endDate = null,
            CancellationToken token = default)
        {
            // Реализация получения комментариев
            return new List<Comment>();
        }

        public async Task<IEnumerable<Tag>> GetTagsAsync(CancellationToken token = default)
        {
            // Реализация получения тегов
            return new List<Tag>();
        }

        public async Task<IEnumerable<User>> GetUsersAsync(CancellationToken token = default)
        {
            // Реализация получения пользователей
            return new List<User>();
        }

        public void Dispose()
        {
            _channel?.Dispose();
        }
    }
}
