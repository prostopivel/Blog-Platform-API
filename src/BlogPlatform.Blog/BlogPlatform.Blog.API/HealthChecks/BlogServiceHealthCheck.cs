using BlogPlatform.Blog.Core.Interfaces.Services;
using BlogPlatform.Shared.Caching.Interfaces;
using BlogPlatform.Shared.Common.Exceptions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BlogPlatform.Blog.API.HealthChecks
{
    public class BlogServiceHealthCheck : IHealthCheck
    {
        private readonly ITagService _tagService;
        private readonly ICommentService _commentService;
        private readonly IPostService _postService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<BlogServiceHealthCheck> _logger;

        public BlogServiceHealthCheck(ITagService tagService,
            ICommentService commentService,
            IPostService postService,
            ICacheService cacheService,
            ILogger<BlogServiceHealthCheck> logger)
        {
            _tagService = tagService;
            _commentService = commentService;
            _postService = postService;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken token = default)
        {
            var healthCheckData = new Dictionary<string, object>();
            var degradedServices = new List<string>();
            var unhealthyServices = new List<string>();

            try
            {
                // Check cache service
                var cacheHealth = await CheckCacheService(token);
                healthCheckData["Cache"] = cacheHealth.Status.ToString();
                if (cacheHealth.Status == HealthStatus.Degraded) degradedServices.Add("Cache");
                if (cacheHealth.Status == HealthStatus.Unhealthy) unhealthyServices.Add("Cache");

                // Check post service
                var postHealth = await CheckPostService(token);
                healthCheckData["PostService"] = postHealth.Status.ToString();
                if (postHealth.Status == HealthStatus.Degraded) degradedServices.Add("PostService");
                if (postHealth.Status == HealthStatus.Unhealthy) unhealthyServices.Add("PostService");

                // Check tag service
                var tagHealth = await CheckTagService(token);
                healthCheckData["TagService"] = tagHealth.Status.ToString();
                if (tagHealth.Status == HealthStatus.Degraded) degradedServices.Add("TagService");
                if (tagHealth.Status == HealthStatus.Unhealthy) unhealthyServices.Add("TagService");

                // Check comment service
                var commentHealth = await CheckCommentService(token);
                healthCheckData["CommentService"] = commentHealth.Status.ToString();
                if (commentHealth.Status == HealthStatus.Degraded) degradedServices.Add("CommentService");
                if (commentHealth.Status == HealthStatus.Unhealthy) unhealthyServices.Add("CommentService");

                // Determine overall status
                if (unhealthyServices.Count != 0)
                {
                    return HealthCheckResult.Unhealthy(
                        $"Unhealthy services: {string.Join(", ", unhealthyServices)}",
                        data: healthCheckData);
                }

                if (degradedServices.Count != 0)
                {
                    return HealthCheckResult.Degraded(
                        $"Degraded services: {string.Join(", ", degradedServices)}",
                        data: healthCheckData);
                }

                _logger.LogInformation("All blog services are healthy");
                return HealthCheckResult.Healthy("All blog services are healthy", healthCheckData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Blog service health check failed");
                return HealthCheckResult.Unhealthy("Blog service health check failed", ex, healthCheckData);
            }
        }

        private async Task<HealthCheckResult> CheckCacheService(CancellationToken token)
        {
            try
            {
                var testKey = $"composite_health_check_cache_{Guid.NewGuid()}";
                var testValue = $"test_{DateTime.UtcNow:yyyyMMddHHmmss}";

                await _cacheService.SetAsync(testKey, testValue, TimeSpan.FromSeconds(5), token: token);
                var cachedValue = await _cacheService.GetAsync<string>(testKey, token: token);
                await _cacheService.RemoveAsync(testKey, token: token);

                return cachedValue == testValue
                    ? HealthCheckResult.Healthy()
                    : HealthCheckResult.Degraded("Cache data mismatch");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Cache service error", ex);
            }
        }

        private async Task<HealthCheckResult> CheckPostService(CancellationToken token)
        {
            try
            {
                await _postService.GetByTagsAsync([], 1, 1, token);
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Post service error", ex);
            }
        }

        private async Task<HealthCheckResult> CheckTagService(CancellationToken token)
        {
            try
            {
                var tags = await _tagService.GetPostTagsAsync(Guid.NewGuid(), token);
                return tags != null
                    ? HealthCheckResult.Healthy()
                    : HealthCheckResult.Degraded("Null response");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Tag service error", ex);
            }
        }

        private async Task<HealthCheckResult> CheckCommentService(CancellationToken token)
        {
            try
            {
                await _commentService.GetByPostIdAsync(Guid.NewGuid(), 1, 1, token);
                return HealthCheckResult.Healthy();
            }
            catch (NotFoundException)
            {
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Comment service error", ex);
            }
        }
    }
}