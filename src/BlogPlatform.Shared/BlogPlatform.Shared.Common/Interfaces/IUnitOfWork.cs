using System.Data;

namespace BlogPlatform.Shared.Common.Interfaces
{
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        IDbConnection Connection { get; }
        IDbTransaction? Transaction { get; }
        Task BeginTransactionAsync(CancellationToken token = default);
        Task CommitAsync(CancellationToken token = default);
        Task RollbackAsync(CancellationToken token = default);
        Task EndTransactionAsync();
    }
}
