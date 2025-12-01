using BlogPlatform.Blog.Core.Models;
using BlogPlatform.Shared.Common.Models;

namespace BlogPlatform.Blog.Core.Interfaces.Services
{
    public interface IPostService
    {
        Task<Post> GetByIdAsync(Guid id,
            int page = 1, int pageSize = 20, CancellationToken token = default);
        Task<PaginatedResult<Post>> GetByTagsAsync(IEnumerable<string> tags,
            int page = 1, int pageSize = 10, CancellationToken token = default);
        Task<PaginatedResult<Post>> GetByUserIdAsync(Guid userId,
            int page = 1, int pageSize = 10, CancellationToken token = default);
        Task<Post> CreateAsync(Post request, Guid userId, CancellationToken token = default);
        Task<Post> UpdateAsync(Post request, Guid userId, CancellationToken token = default);
        Task DeleteAsync(Guid id, Guid userId, CancellationToken token = default);
        Task<LikeResult> IsUserLikeAsync(Guid postId, Guid userId, CancellationToken token = default);
        Task<LikeResult> ToggleLikeAsync(Guid postId, Guid userId, CancellationToken token = default);
    }
}
