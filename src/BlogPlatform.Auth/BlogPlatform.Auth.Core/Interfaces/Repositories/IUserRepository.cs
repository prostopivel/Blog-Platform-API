using BlogPlatform.Auth.Core.Models;

namespace BlogPlatform.Auth.Core.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id, CancellationToken token = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken token = default);
        Task<Guid> CreateAsync(User user, CancellationToken token = default);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken token = default);
    }
}
