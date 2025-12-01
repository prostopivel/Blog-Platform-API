using BlogPlatform.Blog.Core.Interfaces.Repositories;
using BlogPlatform.Blog.Core.Interfaces.Services;
using BlogPlatform.Blog.Core.Models;
using BlogPlatform.Shared.Common.Exceptions;
using BlogPlatform.Shared.Common.Models;
using Microsoft.Extensions.Logging;

namespace BlogPlatform.Blog.Core.Services
{
    public class CommentService : ICommentService
    {
        private readonly IPostRepository _postRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly ILogger<PostService> _logger;

        public CommentService(IPostRepository postRepository,
            ICommentRepository commentRepository,
            ILogger<PostService> logger)
        {
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _logger = logger;
        }

        public async Task<PaginatedResult<Comment>> GetByPostIdAsync(Guid postId,
            int page = 1,
            int pageSize = 20,
            CancellationToken token = default)
        {
            if (!await _postRepository.ExistsAsync(postId, token: token))
            {
                throw new NotFoundException($"Post {postId} not found");
            }

            var result = await _commentRepository.GetByPostIdAsync(postId, page, pageSize, token: token)!;
            _logger.LogInformation("Comments for post {PostId} received, count: {Count}", postId, result.Items.Count());

            return result;
        }

        public async Task<Comment> CreateAsync(Comment comment,
            Guid userId,
            CancellationToken token = default)
        {
            if (await _postRepository.ExistsAsync(comment.PostId, token: token))
            {
                throw new ConflictException($"Comment {comment.Id} already exists");
            }
            comment.UserId = userId;

            await _commentRepository.CreateAsync(comment, token: token)!;
            _logger.LogInformation("Comment with id {Id} created", comment.Id);

            return comment;
        }

        public async Task DeleteAsync(Guid id,
            Guid userId,
            CancellationToken token = default)
        {
            if (!await _commentRepository.ExistsAsync(id, token: token))
            {
                throw new NotFoundException($"Comment {id} not found");
            }
            if ((await _commentRepository.GetByIdAsync(id, token: token))!.UserId != userId)
            {
                throw new ForbidException($"You don`t have access to the comment {id}");
            }

            await _commentRepository.DeleteAsync(id, token: token)!;
            _logger.LogInformation("Comment with id {Id} deleted", id);
        }
    }
}
