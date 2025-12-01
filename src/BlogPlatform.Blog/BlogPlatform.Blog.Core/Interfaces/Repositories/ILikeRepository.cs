namespace BlogPlatform.Blog.Core.Interfaces.Repositories
{
    public interface ILikeRepository
    {
        Task<bool> ToggleLikeAsync(Guid postId, Guid userId, CancellationToken token = default);
        Task<bool> IsLikedAsync(Guid postId, Guid userId, CancellationToken token = default);
        Task<int> GetLikesCountAsync(Guid postId, CancellationToken token = default);
    }
}
