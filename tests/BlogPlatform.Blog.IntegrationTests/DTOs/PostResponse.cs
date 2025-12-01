using BlogPlatform.Blog.Core.Models;
using BlogPlatform.Shared.Common.Models;
using Docker.DotNet.Models;

namespace BlogPlatform.Blog.IntegrationTests.DTOs
{
    public record PostResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<TagResponse> Tags { get; set; } = [];
        public PaginatedResult<CommentResponse> Comments { get; set; } = new();
        public int LikesCount { get; set; }
    }
}
