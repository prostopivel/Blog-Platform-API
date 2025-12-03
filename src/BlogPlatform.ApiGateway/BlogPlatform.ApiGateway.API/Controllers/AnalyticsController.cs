using BlogPlatform.ApiGateway.API.DTOs;
using BlogPlatform.ApiGateway.API.Filters;
using BlogPlatform.ApiGateway.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.ApiGateway.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : GatewayControllerBase
    {
        public AnalyticsController(IMicroserviceClient microserviceClient)
            : base(microserviceClient)
        {
        }

        [HttpGet("by-date-range")]
        public async Task<IActionResult> GetPostsByDateRangeAsync(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] Interval interval = Interval.Hour4,
            CancellationToken token = default)
        {
            var queryString = $"?startDate={startDate:o}&endDate={endDate:o}&interval={interval}";
            var response = await _microserviceClient.ForwardRequestAsync(
                Request, "Analytics", $"api/analytics/by-date-range{queryString}",
                token: token);

            return await ProcessResponse(response);
        }

        [HttpGet("user-posts")]
        [JwtAuthorize]
        public async Task<IActionResult> GetUserPostsActivityAsync(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] Interval interval = Interval.Hour4,
            CancellationToken token = default)
        {
            var userId = GetUserId();
            var queryString = $"?userId={userId}&startDate={startDate:o}&endDate={endDate:o}&interval={interval}";
            var response = await _microserviceClient.ForwardRequestAsync(
                Request, "Analytics", $"api/analytics/user-posts{queryString}",
                token: token);

            return await ProcessResponse(response);
        }

        [HttpGet("user-comments")]
        [JwtAuthorize]
        public async Task<IActionResult> GetUserCommentsActivityAsync(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] Interval interval = Interval.Hour4,
            CancellationToken token = default)
        {
            var userId = GetUserId();
            var queryString = $"?userId={userId}&startDate={startDate:o}&endDate={endDate:o}&interval={interval}";
            var response = await _microserviceClient.ForwardRequestAsync(
                Request, "Analytics", $"api/analytics/user-comments{queryString}",
                token: token);

            return await ProcessResponse(response);
        }

        [HttpGet("user-likes")]
        [JwtAuthorize]
        public async Task<IActionResult> GetUserLikesActivityAsync(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] Interval interval = Interval.Hour4,
            CancellationToken token = default)
        {
            var userId = GetUserId();
            var queryString = $"?userId={userId}&startDate={startDate:o}&endDate={endDate:o}&interval={interval}";
            var response = await _microserviceClient.ForwardRequestAsync(
                Request, "Analytics", $"api/analytics/user-likes{queryString}",
                token: token);

            return await ProcessResponse(response);
        }

        [HttpGet("user-activity")]
        [JwtAuthorize]
        public async Task<IActionResult> GetUserActivityAsync(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] Interval interval = Interval.Hour4,
            CancellationToken token = default)
        {
            var userId = GetUserId();
            var queryString = $"?userId={userId}&startDate={startDate:o}&endDate={endDate:o}&interval={interval}";
            var response = await _microserviceClient.ForwardRequestAsync(
                Request, "Analytics", $"api/analytics/user-activity{queryString}",
                token: token);

            return await ProcessResponse(response);
        }

        [HttpGet("tag-statisics")]
        public async Task<IActionResult> GetTagsStatisticsAsync(
            [FromQuery] int topCount,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            CancellationToken token = default)
        {
            var queryString = $"?topCount={topCount}&startDate={startDate:o}&endDate={endDate:o}";
            var response = await _microserviceClient.ForwardRequestAsync(
                Request, "Analytics", $"api/analytics/tag-statisics{queryString}",
                token: token);

            return await ProcessResponse(response);
        }
    }
}
