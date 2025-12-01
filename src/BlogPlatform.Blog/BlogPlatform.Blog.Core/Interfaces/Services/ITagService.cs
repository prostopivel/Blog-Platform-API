using BlogPlatform.Blog.Core.Models;

namespace BlogPlatform.Blog.Core.Interfaces.Services
{
    public interface ITagService
    {
        Task<Tag> GetByNameAsync(string name, CancellationToken token = default);
        Task<IEnumerable<Tag>> GetPostTagsAsync(Guid postId, CancellationToken token = default);
        Task<Tag> CreateAsync(Tag tag, CancellationToken token = default);
        Task<Guid> CreatePostTagsAsync(IEnumerable<string> tagsNames, Guid postId, CancellationToken token = default);
        Task<Guid> UpdatePostTagsAsync(IEnumerable<string> tagsNames, Guid postId, CancellationToken token = default);
    }
}
