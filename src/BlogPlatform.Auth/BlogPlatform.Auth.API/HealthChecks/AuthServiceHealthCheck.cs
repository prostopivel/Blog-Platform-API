using BlogPlatform.Auth.Core.Interfaces.Services;
using BlogPlatform.Shared.Caching.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BlogPlatform.Auth.API.HealthChecks
{
    public class AuthServiceHealthCheck : IHealthCheck
    {
        private readonly IAuthService _authService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<AuthServiceHealthCheck> _logger;

        public AuthServiceHealthCheck(IAuthService authService,
            ICacheService cacheService,
            ILogger<AuthServiceHealthCheck> logger)
        {
            _authService = authService;
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

                // Test token service
                var authHealth = await CheckTokenService(token);
                healthCheckData["Auth"] = authHealth.Status.ToString();
                if (authHealth.Status == HealthStatus.Degraded) degradedServices.Add("Auth");
                if (authHealth.Status == HealthStatus.Unhealthy) unhealthyServices.Add("Auth");

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

                _logger.LogInformation("All auth services are healthy");
                return HealthCheckResult.Healthy("All blog services are healthy", healthCheckData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Auth service health check failed");
                return HealthCheckResult.Unhealthy("Auth service health check failed", ex, healthCheckData);
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

        private async Task<HealthCheckResult> CheckTokenService(CancellationToken token)
        {
            try
            {
                var testToken = "test";
                await _authService.ValidateTokenAsync(testToken, token: token);
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Auth service error", ex);
            }
        }
    }
}
