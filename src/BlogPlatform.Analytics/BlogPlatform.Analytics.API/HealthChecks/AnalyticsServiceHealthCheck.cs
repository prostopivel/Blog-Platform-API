using BlogPlatform.Analytics.Core.Interfaces.Services;
using BlogPlatform.Analytics.Core.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BlogPlatform.Analytics.API.HealthChecks
{
    public class AnalyticsServiceHealthCheck : IHealthCheck
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogger<AnalyticsServiceHealthCheck> _logger;

        public AnalyticsServiceHealthCheck(IAnalyticsService analyticsService,
            ILogger<AnalyticsServiceHealthCheck> logger)
        {
            _analyticsService = analyticsService;
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
                // Check comment service
                var commentHealth = await CheckAnalyticsService(token: token);
                healthCheckData["AnalyticsService"] = commentHealth.Status.ToString();
                if (commentHealth.Status == HealthStatus.Degraded) degradedServices.Add("AnalyticsService");
                if (commentHealth.Status == HealthStatus.Unhealthy) unhealthyServices.Add("AnalyticsService");

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

                _logger.LogInformation("All analytics services are healthy");
                return HealthCheckResult.Healthy("All analytics services are healthy", healthCheckData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Analytics service health check failed");
                return HealthCheckResult.Unhealthy("Analytics service health check failed", ex, healthCheckData);
            }
        }

        private async Task<HealthCheckResult> CheckAnalyticsService(CancellationToken token)
        {
            try
            {
                await _analyticsService.GetPostsByDateRangeAsync(
                    DateTime.Now.AddDays(-1), DateTime.Now, Interval.Hour4, token: token);
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Analytics service error", ex);
            }
        }
    }
}