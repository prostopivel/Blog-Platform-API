using BlogPlatform.Blog.Core.Constants;
using BlogPlatform.Blog.Core.Interfaces.Repositories;
using BlogPlatform.Blog.Core.Interfaces.Services;
using BlogPlatform.Blog.Core.Models;
using BlogPlatform.Shared.Caching.Interfaces;
using BlogPlatform.Shared.Common.Exceptions;
using BlogPlatform.Shared.Common.Interfaces;
using BlogPlatform.Shared.Common.Models;
using Microsoft.Extensions.Logging;

namespace BlogPlatform.Blog.Core.Services
{
    public class PostService : IPostService
    {
        private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(30);

        private readonly IPostRepository _postRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly ILikeRepository _likeRepository;
        private readonly ITagService _tagService;
        private readonly ICacheService _cacheService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PostService> _logger;

        public PostService(IPostRepository postRepository,
            ICommentRepository commentRepository,
            ILikeRepository likeRepository,
            ITagService tagService,
            ICacheService cacheService,
            IUnitOfWork unitOfWork,
            ILogger<PostService> logger)
        {
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _likeRepository = likeRepository;
            _tagService = tagService;
            _cacheService = cacheService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Post> GetByIdAsync(Guid id,
            int page = 1,
            int pageSize = 20,
            CancellationToken token = default)
        {
            var cacheKey = $"{CacheKeys.POST_BY_ID}:{id}";
            var cachedPost = await _cacheService.GetAsync<Post>(cacheKey, token: token);

            if (cachedPost != null)
            {
                _logger.LogInformation("Post {Id} found in cache", id);
                return cachedPost;
            }

            var result = await _postRepository.GetByIdAsync(id, token: token)
                ?? throw new NotFoundException($"Post {id} not found");

            await ConfigurePost(result, page, pageSize, token: token);

            await _cacheService.SetAsync(cacheKey, result,
                CacheExpiration, token: token);
            _logger.LogInformation("Post {Id} cached", id);

            return result;
        }

        public async Task<PaginatedResult<Post>> GetByTagsAsync(IEnumerable<string> tags,
            int page = 1,
            int pageSize = 10,
            CancellationToken token = default)
        {
            var result = await _postRepository.GetByTagsAsync(tags, page, pageSize, token: token);

            if (!result.Items.Any())
            {
                return new PaginatedResult<Post>
                {
                    Items = [],
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = 0
                };
            }

            foreach (var post in result.Items)
            {
                await ConfigurePost(post, 1, 3, token: token);
            }

            _logger.LogInformation("Post by {TagsCount} tags received, count: {Count}", tags.Count(), result.Items.Count());

            return result;
        }

        public async Task<PaginatedResult<Post>> GetByUserIdAsync(Guid userId,
            int page = 1,
            int pageSize = 10,
            CancellationToken token = default)
        {
            var result = await _postRepository.GetByUserIdAsync(userId, page, pageSize, token: token);

            if (!result.Items.Any())
            {
                return new PaginatedResult<Post>
                {
                    Items = [],
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = 0
                };
            }

            foreach (var post in result.Items)
            {
                await ConfigurePost(post, 1, 3, token: token);
            }

            _logger.LogInformation("Posts by user id {UserId} received, count: {Count}", userId, result.Items.Count());

            return result;
        }

        public async Task<Post> CreateAsync(Post post,
            Guid userId,
            CancellationToken token = default)
        {
            if (await _postRepository.ExistsAsync(post.Id, token: token))
            {
                throw new ConflictException($"Post with id {post.Id} already exist");
            }
            post.UserId = userId;

            await _unitOfWork.BeginTransactionAsync(token: token);
            try
            {
                await _postRepository.CreateAsync(post, token: token);
                await _tagService.CreatePostTagsAsync(post.Tags.Select(t => t.Name), post.Id, token: token);

                await _unitOfWork.CommitAsync(token: token);
                _logger.LogInformation("Post with id {Id} created", post.Id);

                await InvalidatePostCache(post.Id, token: token);

                return post;
            }
            catch
            {
                await _unitOfWork.RollbackAsync(token: token);
                throw;
            }
            finally
            {
                await _unitOfWork.EndTransactionAsync();
            }

        }

        public async Task<Post> UpdateAsync(Post post,
            Guid userId,
            CancellationToken token = default)
        {
            if (!await _postRepository.ExistsAsync(post.Id, token: token))
            {
                throw new NotFoundException($"Post {post.Id} not found");
            }
            if (!await _postRepository.IsUserPostAsync(post.Id, userId, token: token))
            {
                throw new ForbidException($"You don`t have access to the post {post.Id}");
            }

            await _unitOfWork.BeginTransactionAsync(token: token);
            try
            {
                await _postRepository.UpdateAsync(post, token: token);
                await _tagService.UpdatePostTagsAsync(post.Tags.Select(t => t.Name), post.Id, token: token);

                await _unitOfWork.CommitAsync(token: token);
                _logger.LogInformation("Post with id {Id} updated", post.Id);

                await InvalidatePostCache(post.Id, token: token);

                return post;
            }
            catch
            {
                await _unitOfWork.RollbackAsync(token: token);
                throw;
            }
            finally
            {
                await _unitOfWork.EndTransactionAsync();
            }
        }

        public async Task DeleteAsync(Guid id,
            Guid userId,
            CancellationToken token = default)
        {
            if (!await _postRepository.ExistsAsync(id, token: token))
            {
                throw new NotFoundException($"Post {id} not found");
            }
            if (!await _postRepository.IsUserPostAsync(id, userId, token: token))
            {
                throw new ForbidException($"You don`t have access to the post {id}");
            }

            await _postRepository.DeleteAsync(id, token: token);
            _logger.LogInformation("Post with id {Id} deleted", id);

            await InvalidatePostCache(id, token: token);
        }

        public async Task<LikeResult> IsUserLikeAsync(Guid postId,
            Guid userId,
            CancellationToken token = default)
        {
            if (!await _postRepository.ExistsAsync(postId, token: token))
            {
                throw new NotFoundException($"Post {postId} not found");
            }

            var result = await _likeRepository.IsLikedAsync(postId, userId, token: token);
            _logger.LogInformation("User {UserId} is liked post {PostId}: {Result}", userId, postId, result);

            var count = await _likeRepository.GetLikesCountAsync(postId, token: token);

            return new LikeResult(result, count);
        }

        public async Task<LikeResult> ToggleLikeAsync(Guid postId,
            Guid userId,
            CancellationToken token = default)
        {
            if (!await _postRepository.ExistsAsync(postId, token: token))
            {
                throw new NotFoundException($"Post {postId} not found");
            }

            var result = await _likeRepository.ToggleLikeAsync(postId, userId, token: token);
            _logger.LogInformation("User {UserId} like post {PostId}: {Result}", userId, postId, result);

            var count = await _likeRepository.GetLikesCountAsync(postId, token: token);

            return new LikeResult(result, count);
        }

        private async Task InvalidatePostCache(Guid postId,
            CancellationToken token = default)
        {
            var tasks = new List<Task>
            {
                _cacheService.RemoveAsync($"{CacheKeys.POST_BY_ID}:{postId}", token: token),
                _cacheService.RemoveAsync($"{CacheKeys.TAGS_BY_POST_ID}:{postId}", token: token)
            };

            await Task.WhenAll(tasks);
            _logger.LogInformation("Tag cache invalidated for post {Id}", postId);
        }

        private async Task ConfigurePost(Post post,
            int commentPage,
            int commentPageSize,
            CancellationToken token = default)
        {
            post.Tags = [.. await _tagService.GetPostTagsAsync(post.Id, token: token) ?? []];
            post.Comments = await _commentRepository.GetByPostIdAsync(
                post.Id, commentPage, commentPageSize, token: token);
            post.LikesCount = await _likeRepository.GetLikesCountAsync(post.Id, token: token);
        }
    }
}
