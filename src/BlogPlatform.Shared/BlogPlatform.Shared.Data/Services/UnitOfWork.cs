using BlogPlatform.Shared.Common.Interfaces;
using System.Data;
using System.Data.Common;

namespace BlogPlatform.Shared.Data.Services
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbConnection _connection;
        private DbTransaction? _transaction;
        private bool _disposed = false;
        private bool _isOpenedHere = false;

        public UnitOfWork(DbConnection connection)
        {
            _connection = connection;
        }

        public IDbConnection Connection => _connection;
        public IDbTransaction? Transaction => _transaction;

        public async Task BeginTransactionAsync(CancellationToken token = default)
        {
            if (_connection.State != ConnectionState.Open)
            {
                await _connection.OpenAsync(token);
                _isOpenedHere = true;
            }

            _transaction = await _connection.BeginTransactionAsync(token);
        }

        public async Task CommitAsync(CancellationToken token = default)
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("Transaction not started");
            }

            await _transaction.CommitAsync(token);
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public async Task RollbackAsync(CancellationToken token = default)
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("Transaction not started");
            }

            await _transaction.RollbackAsync(token);
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public async Task EndTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        /// <summary>
        /// Disposing without Dispose connection because we didn`t open it there
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }

            await EndTransactionAsync();
            if (_isOpenedHere && _connection.State == ConnectionState.Open)
            {
                await _connection.CloseAsync();
            }
            _disposed = true;
        }

        /// <summary>
        /// Disposing without Dispose connection because we didn`t open it there
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _transaction?.Dispose();
            if (_isOpenedHere && _connection.State == ConnectionState.Open)
            {
                _connection.Close();
            }
            _transaction = null;
            _disposed = true;
        }
    }
}
