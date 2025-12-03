using AutoMapper;
using BlogPlatform.Blog.API.DTOs;
using BlogPlatform.Blog.Core.Interfaces.Services;
using BlogPlatform.Blog.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.Blog.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly IMapper _mapper;

        public CommentsController(ICommentService commentService,
            IMapper mapper)
        {
            _commentService = commentService;
            _mapper = mapper;
        }

        [HttpGet("by-post/{postId}")]
        public async Task<IActionResult> GetCommentsByPost(
            Guid postId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken token = default)
        {
            var result = await _commentService.GetByPostIdAsync(
                postId, page, pageSize, token: token);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateComment(
            [FromBody] CreateCommentRequest comment,
            Guid userId,
            CancellationToken token = default)
        {
            var mappedComment = _mapper.Map<Comment>(comment);
            mappedComment.Id = Guid.NewGuid();
            await _commentService.CreateAsync(mappedComment, userId, token: token);
            return Created();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(
            Guid id,
            Guid userId,
            CancellationToken token = default)
        {
            await _commentService.DeleteAsync(id, userId, token: token);
            return NoContent();
        }
    }
}