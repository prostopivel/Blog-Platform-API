using AutoMapper;
using BlogPlatform.Auth.Core.Interfaces.Repositories;
using BlogPlatform.Auth.Core.Models;
using BlogPlatform.Auth.Infrastructure.Entities;
using Dapper;
using System.Data;

namespace BlogPlatform.Auth.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnection _connection;
        private readonly IMapper _mapper;

        public UserRepository(IDbConnection connection,
            IMapper mapper)
        {
            _connection = connection;
            _mapper = mapper;
        }

        public async Task<User?> GetByIdAsync(Guid userId, CancellationToken token = default)
        {
            const string sql = "SELECT * FROM get_user_by_id(@user_id)";

            var commandDefinition = new CommandDefinition(
                commandText: sql,
                parameters: new { user_id = userId },
                cancellationToken: token
            );

            var result = await _connection.QueryAsync<UserEntity>(commandDefinition);
            return _mapper.Map<User?>(result.SingleOrDefault());
        }

        public async Task<User?> GetByEmailAsync(string userEmail,
            CancellationToken token = default)
        {
            const string sql = "SELECT * FROM get_user_by_email(@user_email)";

            var commandDefinition = new CommandDefinition(
                commandText: sql,
                parameters: new { user_email = userEmail },
                cancellationToken: token
            );

            var result = await _connection.QueryAsync<UserEntity>(commandDefinition);
            return _mapper.Map<User?>(result.SingleOrDefault());
        }

        public async Task<Guid> CreateAsync(User user,
            CancellationToken token = default)
        {
            const string sql = "SELECT create_user(@p_id, @p_username, @p_email, @p_password_hash)";

            var commandDefinition = new CommandDefinition(
                commandText: sql,
                parameters: new
                {
                    p_id = user.Id,
                    p_username = user.Username,
                    p_email = user.Email,
                    p_password_hash = user.PasswordHash
                },
                cancellationToken: token
            );
            var result = await _connection.ExecuteScalarAsync<Guid>(commandDefinition);
            return result;
        }

        public async Task<bool> ExistsByEmailAsync(string email,
            CancellationToken token = default)
        {
            const string sql = "SELECT exists_user_by_email(@user_email)";

            var commandDefinition = new CommandDefinition(
                commandText: sql,
                parameters: new { user_email = email },
                cancellationToken: token
            );
            var result = await _connection.ExecuteScalarAsync<bool>(commandDefinition);
            return result;
        }
    }
}
