using BlogPlatform.ApiGateway.API.DTOs;
using BlogPlatform.ApiGateway.API.Filters;
using BlogPlatform.ApiGateway.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.ApiGateway.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : GatewayControllerBase
    {
        public CommentsController(IMicroserviceClient microserviceClient)
            : base(microserviceClient)
        {
        }

        [HttpGet("by-post/{postId}")]
        public async Task<IActionResult> GetCommentsByPost(
            Guid postId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken token = default)
        {
            var queryString = $"?page={page}&pageSize={pageSize}";
            var response = await _microserviceClient.ForwardRequestAsync(
                Request, "Blog", $"api/comments/by-post/{postId}{queryString}",
                token: token);

            return await ProcessResponse(response);
        }

        [HttpPost]
        [JwtAuthorize]
        public async Task<IActionResult> CreateComment([FromBody] CreateCommentRequest comment,
            CancellationToken token = default)
        {
            var userId = GetUserId();
            var queryString = $"?userId={userId}";
            var response = await _microserviceClient.ForwardRequestAsync(
                Request, "Blog", $"api/comments{queryString}", comment,
                token: token);

            return await ProcessResponse(response);
        }

        [HttpDelete("{id}")]
        [JwtAuthorize]
        public async Task<IActionResult> DeleteComment(Guid id,
            CancellationToken token = default)
        {
            var userId = GetUserId();
            var queryString = $"?userId={userId}";
            var response = await _microserviceClient.ForwardRequestAsync(
                Request, "Blog", $"api/comments/{id}{queryString}",
                token: token);

            return await ProcessResponse(response);
        }
    }
}
