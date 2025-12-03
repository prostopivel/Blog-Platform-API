using BlogPlatform.ApiGateway.API.DTOs;
using BlogPlatform.ApiGateway.API.Filters;
using BlogPlatform.ApiGateway.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.ApiGateway.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : GatewayControllerBase
    {
        public PostsController(IMicroserviceClient microserviceClient)
            : base(microserviceClient)
        {
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPost(Guid id,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken token = default)
        {
            var queryString = $"?page={page}&pageSize={pageSize}";
            var response = await _microserviceClient.ForwardRequestAsync(
                Request, "Blog", $"api/posts/{id}{queryString}",
                token: token);

            return await ProcessResponse(response);
        }

        [HttpGet("by-tags")]
        public async Task<IActionResult> GetPostsByTags(
            [FromQuery] List<string> tags,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken token = default)
        {
            var tagsQuery = string.Join("&", tags.Select(t => $"tags={Uri.EscapeDataString(t)}"));
            var queryString = $"?page={page}&pageSize={pageSize}&{tagsQuery}";
            var response = await _microserviceClient.ForwardRequestAsync(
                Request, "Blog", $"api/posts/by-tags{queryString}",
                token: token);

            return await ProcessResponse(response);
        }

        [HttpGet("by-user/{userId}")]
        public async Task<IActionResult> GetPostsByUser(
            Guid userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken token = default)
        {
            var queryString = $"?page={page}&pageSize={pageSize}";
            var response = await _microserviceClient.ForwardRequestAsync(
                Request, "Blog", $"api/posts/by-user/{userId}{queryString}",
                token: token);

            return await ProcessResponse(response);
        }

        [HttpPost]
        [JwtAuthorize]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest post,
            CancellationToken token = default)
        {
            var userId = GetUserId();
            var queryString = $"?userId={userId}";
            var response = await _microserviceClient.ForwardRequestAsync(
                Request, "Blog", $"api/posts{queryString}", post,
                token: token);

            return await ProcessResponse(response);
        }

        [HttpPut("{id}")]
        [JwtAuthorize]
        public async Task<IActionResult> UpdatePost(
            Guid id,
            [FromBody] UpdatePostRequest post,
            CancellationToken token = default)
        {
            var userId = GetUserId();
            var queryString = $"?userId={userId}";
            var response = await _microserviceClient.ForwardRequestAsync(
                Request, "Blog", $"api/posts/{id}{queryString}", post,
                token: token);

            return await ProcessResponse(response);
        }

        [HttpDelete("{id}")]
        [JwtAuthorize]
        public async Task<IActionResult> DeletePost(Guid id,
            CancellationToken token = default)
        {
            var userId = GetUserId();
            var queryString = $"?userId={userId}";
            var response = await _microserviceClient.ForwardRequestAsync(
                Request, "Blog", $"api/posts/{id}{queryString}",
                token: token);

            return await ProcessResponse(response);
        }

        [HttpPost("{postId}/like/toggle")]
        [JwtAuthorize]
        public async Task<IActionResult> ToggleLike(Guid postId,
            CancellationToken token = default)
        {
            var userId = GetUserId();
            var queryString = $"?userId={userId}";
            var response = await _microserviceClient.ForwardRequestAsync(
                Request, "Blog", $"api/posts/{postId}/like/toggle{queryString}",
                body: (object?)null, token: token);

            return await ProcessResponse(response);
        }

        [HttpGet("{postId}/like/status")]
        [JwtAuthorize]
        public async Task<IActionResult> GetLikeStatus(Guid postId,
            CancellationToken token = default)
        {
            var userId = GetUserId();
            var queryString = $"?userId={userId}";
            var response = await _microserviceClient.ForwardRequestAsync(
                Request, "Blog", $"api/posts/{postId}/like/status{queryString}",
                token: token);

            return await ProcessResponse(response);
        }
    }
}
