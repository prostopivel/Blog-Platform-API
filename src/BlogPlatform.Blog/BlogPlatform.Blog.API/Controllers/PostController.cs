using AutoMapper;
using BlogPlatform.Blog.API.DTOs;
using BlogPlatform.Blog.Core.Interfaces.Services;
using BlogPlatform.Blog.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.Blog.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly IMapper _mapper;

        public PostsController(IPostService postService,
            IMapper mapper)
        {
            _postService = postService;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPost(
            Guid id,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken token = default)
        {
            var post = await _postService.GetByIdAsync(id, page, pageSize, token);
            return Ok(post);
        }

        [HttpGet("by-tags")]
        public async Task<IActionResult> GetPostsByTags(
            [FromQuery] List<string> tags,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken token = default)
        {
            var result = await _postService.GetByTagsAsync(tags, page, pageSize, token);
            return Ok(result);
        }

        [HttpGet("by-user/{userId}")]
        public async Task<IActionResult> GetPostsByUser(
            Guid userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken token = default)
        {
            var result = await _postService.GetByUserIdAsync(userId, page, pageSize, token);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost(
            [FromBody] CreatePostRequest post,
            Guid userId,
            CancellationToken token = default)
        {
            var mappedPost = _mapper.Map<Post>(post);
            mappedPost.Id = Guid.NewGuid();
            await _postService.CreateAsync(
                mappedPost, userId, token);
            return Created();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(
            Guid id,
            Guid userId,
            [FromBody] UpdatePostRequest post,
            CancellationToken token = default)
        {
            var mappedPost = _mapper.Map<Post>(post);
            mappedPost.Id = id;
            var updatedPost = await _postService.UpdateAsync(
                mappedPost, userId, token);
            return Ok(updatedPost);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(
            Guid id,
            Guid userId,
            CancellationToken token = default)
        {
            await _postService.DeleteAsync(id, userId, token);
            return NoContent();
        }

        [HttpPost("{postId}/like/toggle")]
        public async Task<IActionResult> ToggleLike(
            Guid postId,
            Guid userId,
            CancellationToken token = default)
        {
            var result = await _postService.ToggleLikeAsync(postId, userId, token);
            return Ok(result);
        }

        [HttpGet("{postId}/like/status")]
        public async Task<IActionResult> GetLikeStatus(
            Guid postId,
            Guid userId,
            CancellationToken token = default)
        {
            var result = await _postService.IsUserLikeAsync(postId, userId, token);
            return Ok(result);
        }
    }
}