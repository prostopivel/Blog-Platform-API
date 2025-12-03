using BlogPlatform.Blog.Core.Models;
using BlogPlatform.Shared.Common.Models;

namespace BlogPlatform.Blog.Core.Interfaces.Repositories
{
    public interface ICommentRepository
    {
        Task<Comment> GetByIdAsync(Guid id, CancellationToken token = default);
        Task<PaginatedResult<Comment>> GetByPostIdAsync(Guid postId,
            int page = 1, int pageSize = 20, CancellationToken token = default);
        Task<Guid> CreateAsync(Comment comment, CancellationToken token = default);
        Task DeleteAsync(Guid id, CancellationToken token = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken token = default);
    }
}
