using BlogPlatform.Shared.Common.Models;

namespace BlogPlatform.Blog.Core.Models
{
    public class Post
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<Tag> Tags { get; set; } = [];
        public PaginatedResult<Comment> Comments { get; set; } = new();
        public int LikesCount { get; set; }
    }
}
