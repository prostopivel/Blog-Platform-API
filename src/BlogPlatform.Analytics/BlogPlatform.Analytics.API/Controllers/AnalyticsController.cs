using BlogPlatform.Analytics.Core.Interfaces.Services;
using BlogPlatform.Analytics.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.Analytics.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet("by-date-range")]
        public async Task<IActionResult> GetPostsByDateRangeAsync(DateTime startDate,
            DateTime endDate, Interval interval = Interval.Hour4,
            CancellationToken token = default)
        {
            var result = await _analyticsService.GetPostsByDateRangeAsync(
                startDate, endDate, interval, token: token);

            return Ok(result);
        }

        [HttpGet("user-posts")]
        public async Task<IActionResult> GetUserPostsActivityAsync(Guid userId,
            DateTime startDate, DateTime endDate, Interval interval = Interval.Hour4,
            CancellationToken token = default)
        {
            var result = await _analyticsService.GetUserPostsActivityAsync(
                userId, startDate, endDate, interval, token: token);

            return Ok(result);
        }

        [HttpGet("user-comments")]
        public async Task<IActionResult> GetUserCommentsActivityAsync(Guid userId,
            DateTime startDate, DateTime endDate, Interval interval = Interval.Hour4,
            CancellationToken token = default)
        {
            var result = await _analyticsService.GetUserCommentsActivityAsync(
                userId, startDate, endDate, interval, token: token);

            return Ok(result);
        }

        [HttpGet("user-likes")]
        public async Task<IActionResult> GetUserLikesActivityAsync(Guid userId,
            DateTime startDate, DateTime endDate, Interval interval = Interval.Hour4,
            CancellationToken token = default)
        {
            var result = await _analyticsService.GetUserLikesActivityAsync(
                userId, startDate, endDate, interval, token: token);

            return Ok(result);
        }

        [HttpGet("user-activity")]
        public async Task<IActionResult> GetUserActivityAsync(Guid userId,
            DateTime startDate, DateTime endDate, Interval interval = Interval.Hour4,
            CancellationToken token = default)
        {
            var result = await _analyticsService.GetUserActivityAsync(
                userId, startDate, endDate, interval, token: token);

            return Ok(result);
        }

        [HttpGet("tag-statisics")]
        public async Task<IActionResult> GetTagsStatisticsAsync(int topCount,
            DateTime startDate, DateTime endDate, CancellationToken token = default)
        {
            var result = await _analyticsService.GetTagsStatisticsAsync(
                topCount, startDate, endDate, token: token);

            return Ok(result);
        }
    }
}
