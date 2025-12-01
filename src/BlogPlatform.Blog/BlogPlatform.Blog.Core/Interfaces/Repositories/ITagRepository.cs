using BlogPlatform.Blog.Core.Models;

namespace BlogPlatform.Blog.Core.Interfaces.Repositories
{
    public interface ITagRepository
    {
        Task<Tag?> GetByNameAsync(string name, CancellationToken token = default);
        Task<IEnumerable<Tag>> GetPostTagsAsync(Guid postId, CancellationToken token = default);
        Task<Guid> CreateAsync(Tag tag, CancellationToken token = default);
        Task<Guid> CreatePostTagsAsync(IEnumerable<Tag> tags, Guid postId, CancellationToken token = default);
        Task<Guid> UpdatePostTagsAsync(IEnumerable<Tag> tags, Guid postId, CancellationToken token = default);
        Task<bool> ExistsAsync(string name, CancellationToken token = default);
    }
}
