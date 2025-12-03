using AutoMapper;
using BlogPlatform.Blog.Core.Interfaces.Repositories;
using BlogPlatform.Blog.Core.Models;
using BlogPlatform.Blog.Infrastructure.Entities;
using Dapper;
using System.Data;

namespace BlogPlatform.Blog.Infrastructure.Repositories
{
    public class TagRepository : ITagRepository
    {
        private readonly IDbConnection _connection;
        private readonly IMapper _mapper;

        public TagRepository(IDbConnection connection,
            IMapper mapper)
        {
            _connection = connection;
            _mapper = mapper;
        }

        public async Task<Tag?> GetByNameAsync(string name,
            CancellationToken token = default)
        {
            const string sql = "SELECT * FROM get_tag_by_name(@check_name)";

            var commandDefinition = new CommandDefinition(
                commandText: sql,
                parameters: new { check_name = name },
                cancellationToken: token
            );

            var result = await _connection.QueryAsync<TagEntity>(commandDefinition);
            return _mapper.Map<Tag?>(result?.SingleOrDefault());
        }

        public async Task<IEnumerable<Tag>> GetPostTagsAsync(Guid postId,
            CancellationToken token = default)
        {
            const string sql = "SELECT * FROM get_post_tags(@p_post_id)";

            var commandDefinition = new CommandDefinition(
                commandText: sql,
                parameters: new { p_post_id = postId },
                cancellationToken: token
            );

            var result = await _connection.QueryAsync<TagEntity>(commandDefinition);
            return _mapper.Map<IEnumerable<Tag>>(result);
        }

        public async Task<Guid> CreateAsync(Tag tag,
            CancellationToken token = default)
        {
            const string sql = "SELECT create_tag(@p_id, @p_name)";

            var commandDefinition = new CommandDefinition(
                commandText: sql,
                parameters: new
                {
                    p_id = tag.Id,
                    p_name = tag.Name,
                },
                cancellationToken: token
            );

            var result = await _connection.ExecuteScalarAsync<Guid>(commandDefinition);
            return result;
        }

        public async Task<Guid> CreatePostTagsAsync(IEnumerable<Tag> tags,
            Guid postId,
            CancellationToken token = default)
        {
            const string sql = "SELECT create_post_tags(@p_post_id, VARIADIC @p_tag_names)";

            var commandDefinition = new CommandDefinition(
                commandText: sql,
                parameters: new
                {
                    p_post_id = postId,
                    p_tag_names = tags.Select(t => t.Name).ToArray()
                },
                cancellationToken: token
            );

            var result = await _connection.ExecuteScalarAsync<Guid>(commandDefinition);
            return result;
        }

        public async Task<Guid> UpdatePostTagsAsync(IEnumerable<Tag> tags,
            Guid postId,
            CancellationToken token = default)
        {
            const string sql = "SELECT update_post_tags(@p_post_id, VARIADIC @p_tag_names)";

            var commandDefinition = new CommandDefinition(
                commandText: sql,
                parameters: new
                {
                    p_post_id = postId,
                    p_tag_names = tags.Select(t => t.Name).ToArray()
                },
                cancellationToken: token
            );

            var result = await _connection.ExecuteScalarAsync<Guid>(commandDefinition);
            return result;
        }

        public async Task<bool> ExistsAsync(string name, CancellationToken token = default)
        {
            const string sql = "SELECT exists_tag_by_name(@check_name)";

            var commandDefinition = new CommandDefinition(
                commandText: sql,
                parameters: new { check_name = name },
                cancellationToken: token
            );

            var result = await _connection.ExecuteScalarAsync<bool>(commandDefinition);
            return result;
        }
    }
}
