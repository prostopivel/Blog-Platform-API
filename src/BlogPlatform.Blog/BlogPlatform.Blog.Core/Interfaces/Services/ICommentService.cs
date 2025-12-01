using BlogPlatform.Blog.Core.Models;
using BlogPlatform.Shared.Common.Models;

namespace BlogPlatform.Blog.Core.Interfaces.Services
{
    public interface ICommentService
    {
        Task<PaginatedResult<Comment>> GetByPostIdAsync(Guid postId,
            int page = 1, int pageSize = 20, CancellationToken token = default);
        Task<Comment> CreateAsync(Comment request, Guid userId, CancellationToken token = default);
        Task DeleteAsync(Guid id, Guid userId, CancellationToken token = default);
    }
}
