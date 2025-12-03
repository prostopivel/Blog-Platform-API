using BlogPlatform.Blog.Core.Constants;
using BlogPlatform.Blog.Core.Interfaces.Repositories;
using BlogPlatform.Blog.Core.Interfaces.Services;
using BlogPlatform.Blog.Core.Models;
using BlogPlatform.Shared.Caching.Interfaces;
using BlogPlatform.Shared.Common.Exceptions;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace BlogPlatform.Blog.Core.Services
{
    public class TagService : ITagService
    {
        private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(60);

        private readonly ITagRepository _tagRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<TagService> _logger;

        public TagService(ITagRepository tagRepository,
            ICacheService cacheService,
            ILogger<TagService> logger)
        {
            _tagRepository = tagRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<Tag> GetByNameAsync(string name,
            CancellationToken token = default)
        {
            var cacheKey = $"{CacheKeys.TAG_BY_NAME}:{name}";
            var cachedTag = await _cacheService.GetAsync<Tag>(cacheKey, token: token);

            if (cachedTag != null)
            {
                _logger.LogInformation("Tag {Name} found in cache", name);
                return cachedTag;
            }

            var result = await _tagRepository.GetByNameAsync(name, token: token)
                ?? throw new NotFoundException($"Tag {name} not found");

            await _cacheService.SetAsync(cacheKey, result,
                CacheExpiration, token: token);
            _logger.LogInformation("Tag {Name} cached", name);

            return result;
        }

        public async Task<IEnumerable<Tag>> GetPostTagsAsync(Guid postId,
            CancellationToken token = default)
        {
            var cacheKey = $"{CacheKeys.TAGS_BY_POST_ID}:{postId}";
            var cachedTags = await _cacheService.GetAsync<IEnumerable<Tag>>(cacheKey, token: token);

            if (cachedTags != null)
            {
                _logger.LogInformation("Post {Id} tags found in cache", postId);
                return cachedTags;
            }

            var result = await _tagRepository.GetPostTagsAsync(postId, token: token);

            await _cacheService.SetAsync(cacheKey, result,
                CacheExpiration, token: token);
            _logger.LogInformation("Post {Id} tags cached", postId);

            return result;
        }

        public async Task<Tag> CreateAsync(Tag tag,
            CancellationToken token = default)
        {
            if (await _tagRepository.ExistsAsync(tag.Name, token: token))
            {
                throw new ConflictException($"Tag with name {tag.Name} already exists");
            }

            await _tagRepository.CreateAsync(tag, token: token);
            _logger.LogInformation("Tag {Id} created", tag.Id);

            await InvalidateTagCache(tag.Name, token: token);

            return tag;
        }

        public async Task<Guid> CreatePostTagsAsync(IEnumerable<string> tagsNames,
            Guid postId,
            CancellationToken token = default)
        {
            var resultTags = new List<Tag>();

            foreach (var name in tagsNames)
            {
                if (!await _tagRepository.ExistsAsync(name, token: token))
                {
                    var tag = new Tag(Guid.NewGuid(), name);
                    await _tagRepository.CreateAsync(tag, token: token);
                    _logger.LogInformation("Tag {Id} created", tag.Id);

                    await InvalidateTagCache(tag.Name, token: token);
                    resultTags.Add(new Tag(tag.Id, tag.Name));
                }
                else
                {
                    var existTag = await _tagRepository.GetByNameAsync(name, token: token);
                    resultTags.Add(existTag!);
                }
            }

            var result = await _tagRepository.CreatePostTagsAsync(resultTags, postId, token: token);
            _logger.LogInformation("Tags added to the post {Id}", postId);

            return result;
        }

        public async Task<Guid> UpdatePostTagsAsync(IEnumerable<string> tagsNames,
            Guid postId,
            CancellationToken token = default)
        {
            var resultTags = new List<Tag>();

            foreach (var name in tagsNames)
            {
                if (!await _tagRepository.ExistsAsync(name, token: token))
                {
                    var tag = new Tag(Guid.NewGuid(), name);
                    await _tagRepository.CreateAsync(tag, token: token);
                    _logger.LogInformation("Tag {Id} created", tag.Id);

                    await InvalidateTagCache(tag.Name, token: token);
                    resultTags.Add(new Tag(tag.Id, tag.Name));
                }
                else
                {
                    var existTag = await _tagRepository.GetByNameAsync(name, token: token);
                    resultTags.Add(existTag!);
                }
            }

            var result = await _tagRepository.UpdatePostTagsAsync(resultTags, postId, token: token);
            _logger.LogInformation("Tags updated in the post {Id}", postId);

            return result;
        }

        private async Task InvalidateTagCache(string name,
            CancellationToken token = default)
        {
            var tasks = new List<Task>
            {
                _cacheService.RemoveAsync($"{CacheKeys.TAG_BY_NAME}:{name}", token: token),
            };

            await Task.WhenAll(tasks);
            _logger.LogInformation("Tag cache invalidated for tag {Name}", name);
        }
    }
}
