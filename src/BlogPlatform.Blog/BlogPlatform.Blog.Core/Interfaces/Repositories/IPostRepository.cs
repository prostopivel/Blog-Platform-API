using BlogPlatform.Blog.Core.Models;
using BlogPlatform.Shared.Common.Models;

namespace BlogPlatform.Blog.Core.Interfaces.Repositories
{
    public interface IPostRepository
    {
        Task<Post?> GetByIdAsync(Guid id, CancellationToken token = default);
        Task<PaginatedResult<Post>> GetByTagsAsync(IEnumerable<string> tags,
            int page = 1, int pageSize = 10, CancellationToken token = default);
        Task<PaginatedResult<Post>> GetByUserIdAsync(Guid userId,
            int page = 1, int pageSize = 10, CancellationToken token = default);
        Task<Guid> CreateAsync(Post post, CancellationToken token = default);
        Task<Guid> UpdateAsync(Post post, CancellationToken token = default);
        Task DeleteAsync(Guid id, CancellationToken token = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken token = default);
        Task<bool> IsUserPostAsync(Guid id, Guid userId, CancellationToken token = default);
    }
}
